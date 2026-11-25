using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EChannelizer.Models;
using Microsoft.Extensions.Logging;

namespace EChannelizer.Services
{
    /// <summary>
    /// EPG service for loading and managing EPG data
    /// </summary>
    public class EpgService : IEpgService
    {
        private readonly ILogger<EpgService> _logger;
        private readonly EpgManager _epgManager = new();

        public EpgService(ILogger<EpgService> logger)
        {
            _logger = logger;
        }

        public EpgManager Manager => _epgManager;

        public async Task<List<EpgProgram>> LoadEpgAsync(EpgSource source)
        {
            var programs = new List<EpgProgram>();

            try
            {
                switch (source.Type)
                {
                    case EpgSourceType.XMLTV:
                        programs = await LoadXmlTvAsync(source.FilePath);
                        break;
                    case EpgSourceType.MHW:
                        programs = await LoadMhwAsync(source.FilePath);
                        break;
                    case EpgSourceType.EPGDAT:
                        programs = await LoadEpgDatAsync(source.FilePath);
                        break;
                    default:
                        _logger.LogWarning("Unsupported EPG source type: {Type}", source.Type);
                        break;
                }

                _logger.LogInformation("Loaded {Count} EPG programs from {Source}", programs.Count, source.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load EPG from {Source}", source.Name);
            }

            return programs;
        }

        public async Task UpdateEpgAsync(EpgSource source)
        {
            if (!source.IsEnabled)
                return;

            try
            {
                var programs = await LoadEpgAsync(source);
                _epgManager.AddPrograms(programs);

                source.LastUpdate = DateTime.Now;
                source.NextUpdate = DateTime.Now.AddHours(source.UpdateInterval);

                _logger.LogInformation("Updated EPG from {Source}: {Count} programs", source.Name, programs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update EPG from {Source}", source.Name);
            }
        }

        public async Task<MapResult> MapEpgToChannelsAsync(List<XmlTvChannel> xmlChannels, List<IChannel> channels)
        {
            var result = new MapResult();

            try
            {
                var channelDict = channels.ToDictionary(c => c.UniqueKey, c => c);

                foreach (var xmlChannel in xmlChannels)
                {
                    var mapped = false;

                    // Try mapping by ServiceId
                    var dvbChannel = channels.OfType<DvbSChannel>()
                        .FirstOrDefault(c => c.ServiceId == xmlChannel.ServiceId &&
                                           c.TransportStreamId == xmlChannel.TransportStreamId &&
                                           c.NetworkId == xmlChannel.NetworkId);

                    if (dvbChannel != null)
                    {
                        xmlChannel.MappedChannel = dvbChannel;
                        xmlChannel.IsMapped = true;
                        mapped = true;
                    }

                    if (!mapped)
                    {
                        // Try mapping by name
                        var byName = channels.FirstOrDefault(c =>
                            c.Name.Equals(xmlChannel.Name, StringComparison.OrdinalIgnoreCase));

                        if (byName != null)
                        {
                            xmlChannel.MappedChannel = byName;
                            xmlChannel.IsMapped = true;
                            mapped = true;
                        }
                    }

                    if (mapped)
                    {
                        result.Mapped++;
                    }
                    else
                    {
                        result.Unmapped++;
                        result.UnmappedXmlChannels.Add(xmlChannel.Name);
                    }
                }

                result.UnmappedChannels = channels.Where(c => !xmlChannels.Any(x => x.MappedChannel == c))
                    .Select(c => c.Name).ToList();

                _logger.LogInformation("EPG mapping: {Mapped} mapped, {Unmapped} unmapped", result.Mapped, result.Unmapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to map EPG to channels");
            }

            return result;
        }

        public async Task<List<EpgProgram>> GetEpgForChannelAsync(IChannel channel, DateTime start, DateTime end)
        {
            return await Task.FromResult(_epgManager.GetProgramsForService(channel.ServiceId, start, end));
        }

        private async Task<List<EpgProgram>> LoadXmlTvAsync(string filePath)
        {
            var programs = new List<EpgProgram>();

            if (!File.Exists(filePath))
                return programs;

            var doc = XDocument.Load(filePath);

            foreach (var programme in doc.Descendants("programme"))
            {
                var program = new EpgProgram
                {
                    ChannelId = programme.Attribute("channel")?.Value ?? "",
                    Start = ParseXmlTvDateTime(programme.Attribute("start")?.Value),
                    Stop = ParseXmlTvDateTime(programme.Attribute("stop")?.Value),
                    Title = programme.Element("title")?.Value ?? "",
                    Description = programme.Element("desc")?.Value ?? "",
                    Language = programme.Element("title")?.Attribute("lang")?.Value ?? "en"
                };

                program.EndTime = program.Stop;
                program.StartTime = program.Start;

                foreach (var category in programme.Elements("category"))
                {
                    program.Categories.Add(category.Value);
                }

                var rating = programme.Element("rating");
                if (rating != null)
                {
                    int.TryParse(rating.Attribute("value")?.Value, out var pr);
                    program.ParentalRating = pr;
                }

                programs.Add(program);
            }

            return programs;
        }

        private async Task<List<EpgProgram>> LoadMhwAsync(string filePath)
        {
            // MHW EPG format parsing
            return new List<EpgProgram>();
        }

        private async Task<List<EpgProgram>> LoadEpgDatAsync(string filePath)
        {
            // EPGDAT format parsing
            return new List<EpgProgram>();
        }

        private DateTime ParseXmlTvDateTime(string? dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr))
                return DateTime.MinValue;

            // XMLTV format: YYYYMMDDHHMMSS +ZZZZ
            if (dateTimeStr.Length >= 14)
            {
                var year = int.Parse(dateTimeStr.Substring(0, 4));
                var month = int.Parse(dateTimeStr.Substring(4, 2));
                var day = int.Parse(dateTimeStr.Substring(6, 2));
                var hour = int.Parse(dateTimeStr.Substring(8, 2));
                var minute = int.Parse(dateTimeStr.Substring(10, 2));
                var second = int.Parse(dateTimeStr.Substring(12, 2));

                var dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

                // Parse timezone offset
                if (dateTimeStr.Length > 14)
                {
                    var tzStr = dateTimeStr.Substring(14);
                    if (tzStr.StartsWith("+") || tzStr.StartsWith("-"))
                    {
                        var sign = tzStr[0] == '+' ? 1 : -1;
                        var tzHours = int.Parse(tzStr.Substring(1, 2));
                        var tzMinutes = int.Parse(tzStr.Substring(3, 2));
                        var offset = TimeSpan.FromHours(sign * tzHours) + TimeSpan.FromMinutes(sign * tzMinutes);
                        dt = dt.Add(offset);
                    }
                }

                return dt;
            }

            return DateTime.MinValue;
        }
    }
}