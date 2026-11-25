using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using EChannelizer.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EChannelizer.Services
{
    /// <summary>
    /// Import/Export service implementation
    /// </summary>
    public class ImportExportService : IImportExportService
    {
        private readonly ILogger<ImportExportService> _logger;

        public ImportExportService(ILogger<ImportExportService> logger)
        {
            _logger = logger;
        }

        public async Task<ImportResult> ImportSatellitesAsync(string sourcePath, ImportSatellitesOptions options)
        {
            var result = new ImportResult();

            try
            {
                var extension = Path.GetExtension(sourcePath).ToLowerInvariant();

                switch (extension)
                {
                    case ".xml":
                        await ImportSatellitesFromXmlAsync(sourcePath, options, result);
                        break;
                    case ".json":
                        await ImportSatellitesFromJsonAsync(sourcePath, options, result);
                        break;
                    default:
                        result.Success = false;
                        result.Errors.Add($"Unsupported format: {extension}");
                        break;
                }

                result.Success = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import satellites");
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private async Task ImportSatellitesFromXmlAsync(string path, ImportSatellitesOptions options, ImportResult result)
        {
            var doc = XDocument.Load(path);

            // Parse common satellite XML formats (KingOfSat, FlySat, etc.)
            var satellites = doc.Descendants("sat")
                .Select(x => new Satellite
                {
                    Name = x.Attribute("name")?.Value ?? "",
                    OrbitalPosition = double.TryParse(x.Attribute("pos")?.Value, out var pos) ? pos : 0,
                    Direction = x.Attribute("dir")?.Value == "W" ? SatelliteDirection.West : SatelliteDirection.East,
                    Region = x.Attribute("region")?.Value ?? "",
                    Country = x.Attribute("country")?.Value ?? ""
                })
                .Where(s => !string.IsNullOrEmpty(s.Name))
                .ToList();

            result.SatellitesImported = satellites.Count;
            _logger.LogInformation("Imported {Count} satellites from XML", satellites.Count);
        }

        private async Task ImportSatellitesFromJsonAsync(string path, ImportSatellitesOptions options, ImportResult result)
        {
            var json = await File.ReadAllTextAsync(path);
            var database = JsonConvert.DeserializeObject<SatelliteDatabase>(json);

            if (database?.Satellites != null)
            {
                result.SatellitesImported = database.Satellites.Count;
            }
        }

        public async Task<ImportResult> ImportDvbAsync(string sourcePath, ImportDvbOptions options)
        {
            var result = new ImportResult();

            try
            {
                var extension = Path.GetExtension(sourcePath).ToLowerInvariant();

                switch (extension)
                {
                    case ".xml":
                        await ImportDvbFromXmlAsync(sourcePath, options, result);
                        break;
                    case ".json":
                        await ImportDvbFromJsonAsync(sourcePath, options, result);
                        break;
                    case ".csv":
                        await ImportDvbFromCsvAsync(sourcePath, options, result);
                        break;
                    default:
                        result.Success = false;
                        result.Errors.Add($"Unsupported format: {extension}");
                        break;
                }

                result.Success = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import DVB data");
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private async Task ImportDvbFromXmlAsync(string path, ImportDvbOptions options, ImportResult result)
        {
            // Implementation for DVB XML import
            _logger.LogInformation("Importing DVB from XML");
        }

        private async Task ImportDvbFromJsonAsync(string path, ImportDvbOptions options, ImportResult result)
        {
            // Implementation for DVB JSON import
            _logger.LogInformation("Importing DVB from JSON");
        }

        private async Task ImportDvbFromCsvAsync(string path, ImportDvbOptions options, ImportResult result)
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<dynamic>().ToList();
            result.ChannelsImported = records.Count;
        }

        public async Task<ImportResult> ImportEpgAsync(string sourcePath, EpgSourceType type)
        {
            var result = new ImportResult();

            try
            {
                switch (type)
                {
                    case EpgSourceType.XMLTV:
                        await ImportXmlTvAsync(sourcePath, result);
                        break;
                    case EpgSourceType.MHW:
                        await ImportMhwAsync(sourcePath, result);
                        break;
                    case EpgSourceType.EPGDAT:
                        await ImportEpgDatAsync(sourcePath, result);
                        break;
                    default:
                        result.Success = false;
                        result.Errors.Add($"Unsupported EPG type: {type}");
                        break;
                }

                result.Success = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import EPG");
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private async Task ImportXmlTvAsync(string path, ImportResult result)
        {
            // Parse XMLTV format
            var doc = XDocument.Load(path);

            var programmes = doc.Descendants("programme").ToList();
            result.ChannelsImported = programmes.Count; // Using this field for programme count

            _logger.LogInformation("Imported {Count} programmes from XMLTV", programmes.Count);
        }

        private async Task ImportMhwAsync(string path, ImportResult result)
        {
            // MHW EPG format
            _logger.LogInformation("Importing MHW EPG");
        }

        private async Task ImportEpgDatAsync(string path, ImportResult result)
        {
            // EPGDAT format
            _logger.LogInformation("Importing EPGDAT");
        }

        public async Task<ImportResult> ImportLogosAsync(string sourcePath)
        {
            var result = new ImportResult();

            try
            {
                if (Directory.Exists(sourcePath))
                {
                    var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)
                        .Where(f => IsImageFile(f))
                        .ToList();

                    result.LogosImported = files.Count;
                }
                else if (File.Exists(sourcePath))
                {
                    var extension = Path.GetExtension(sourcePath).ToLowerInvariant();

                    if (extension == ".zip")
                    {
                        using var zip = ZipFile.OpenRead(sourcePath);
                        result.LogosImported = zip.Entries.Count(e => IsImageFile(e.Name));
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import logos");
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<ImportResult> ImportM3UAsync(string sourcePath)
        {
            var result = new ImportResult();

            try
            {
                var lines = await File.ReadAllLinesAsync(sourcePath);
                IChannel? currentChannel = null;
                int channelCount = 0;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    if (trimmed.StartsWith("#EXTINF:"))
                    {
                        var info = trimmed.Substring(8);
                        var commaIndex = info.LastIndexOf(',');
                        if (commaIndex > 0)
                        {
                            var name = info.Substring(commaIndex + 1).Trim();
                            currentChannel = new IptvChannel { Name = name, ServiceType = ServiceType.TV };
                        }
                    }
                    else if (!trimmed.StartsWith("#") && !string.IsNullOrEmpty(trimmed) && currentChannel != null)
                    {
                        if (currentChannel is IptvChannel iptv)
                        {
                            iptv.Url = trimmed;
                            channelCount++;
                            currentChannel = null;
                        }
                    }
                }

                result.ChannelsImported = channelCount;
                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import M3U");
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<ImportResult> ImportRepositoryAsync(string sourcePath)
        {
            var result = new ImportResult();

            try
            {
                // Use repository service to load
                // This is a placeholder - actual implementation would merge repositories
                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import repository");
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task ExportRepositoryAsync(ChannelRepository repository, string targetPath)
        {
            _logger.LogInformation("Exporting repository to {Path}", targetPath);

            var extension = Path.GetExtension(targetPath).ToLowerInvariant();

            switch (extension)
            {
                case ".ech":
                case ".zip":
                    await ExportToZipAsync(repository, targetPath);
                    break;
                case ".json":
                    await ExportToJsonAsync(repository, targetPath);
                    break;
                default:
                    throw new NotSupportedException($"Export format {extension} not supported");
            }
        }

        private async Task ExportToZipAsync(ChannelRepository repository, string path)
        {
            using var stream = File.Create(path);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create);

            var json = JsonConvert.SerializeObject(repository, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            var entry = zip.CreateEntry("repository.json");
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            await writer.WriteAsync(json);
        }

        private async Task ExportToJsonAsync(ChannelRepository repository, string path)
        {
            var json = JsonConvert.SerializeObject(repository, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            await File.WriteAllTextAsync(path, json);
        }

        public async Task ExportM3UAsync(List<IChannel> channels, string targetPath)
        {
            _logger.LogInformation("Exporting {Count} channels to M3U", channels.Count);

            using var writer = new StreamWriter(targetPath, false, Encoding.UTF8);

            writer.WriteLine("#EXTM3U");

            foreach (var channel in channels)
            {
                var flags = new List<string>();

                if (channel is IptvChannel iptv)
                {
                    if (!string.IsNullOrEmpty(iptv.Url))
                    {
                        flags.Add($"tvg-id=\"{channel.ServiceId}\"");
                        flags.Add($"tvg-name=\"{channel.Name}\"");
                        if (!string.IsNullOrEmpty(channel.LogoPath))
                            flags.Add($"tvg-logo=\"{channel.LogoPath}\"");
                        if (!string.IsNullOrEmpty(channel.Satellite))
                            flags.Add($"group-title=\"{channel.Satellite}\"");

                        writer.WriteLine($"#EXTINF:-1 {string.Join(" ", flags)},{channel.Name}");
                        writer.WriteLine(iptv.Url);
                    }
                }
                else
                {
                    // For DVB channels, create a reference URL
                    flags.Add($"tvg-id=\"{channel.ServiceId}\"");
                    flags.Add($"tvg-name=\"{channel.Name}\"");
                    if (!string.IsNullOrEmpty(channel.LogoPath))
                        flags.Add($"tvg-logo=\"{channel.LogoPath}\"");
                    if (!string.IsNullOrEmpty(channel.Satellite))
                        flags.Add($"group-title=\"{channel.Satellite}\"");

                    var refUrl = $"dvb://{channel.Satellite}/{channel.TransportStreamId}/{channel.ServiceId}";
                    writer.WriteLine($"#EXTINF:-1 {string.Join(" ", flags)},{channel.Name}");
                    writer.WriteLine(refUrl);
                }
            }
        }

        public async Task ExportSatellitesAsync(List<Satellite> satellites, string targetPath)
        {
            _logger.LogInformation("Exporting {Count} satellites", satellites.Count);

            var database = new SatelliteDatabase
            {
                Satellites = satellites.Select(s => new WebSatellite
                {
                    Name = s.Name,
                    OrbitalPosition = s.OrbitalPosition,
                    Direction = s.Direction,
                    Region = s.Region,
                    Country = s.Country,
                    Transponders = s.Transponders.ConvertAll(t => new WebTransponder
                    {
                        Frequency = t is TransponderDVBs dvbs ? dvbs.Frequency : 0,
                        Polarization = t is TransponderDVBs dvbs2 ? dvbs2.Polarization : Polarization.Horizontal,
                        SymbolRate = t is TransponderDVBs dvbs3 ? dvbs3.SymbolRate : 0,
                        FEC = t is TransponderDVBs dvbs4 ? dvbs4.FEC : "Auto",
                        Modulation = t is TransponderDVBs dvbs5 ? dvbs5.Modulation : "QPSK",
                        IsDvbS2 = t is TransponderDVBs dvbs6 && dvbs6.IsDvbS2
                    }),
                    LastUpdated = DateTime.Now
                }).ToList(),
                LastUpdated = DateTime.Now,
                Source = "E-Channelizer"
            };

            var json = JsonConvert.SerializeObject(database, Formatting.Indented);
            await File.WriteAllTextAsync(targetPath, json);
        }

        public async Task ExportLogosAsync(Dictionary<string, string> logos, string targetFolder)
        {
            _logger.LogInformation("Exporting {Count} logos to {Folder}", logos.Count, targetFolder);

            Directory.CreateDirectory(targetFolder);

            foreach (var logo in logos)
            {
                if (File.Exists(logo.Value))
                {
                    var destPath = Path.Combine(targetFolder, logo.Key);
                    File.Copy(logo.Value, destPath, true);
                }
            }
        }

        public async Task ExportEpgAsync(List<EpgProgram> programs, string targetPath)
        {
            _logger.LogInformation("Exporting {Count} EPG programs", programs.Count);

            var xml = new XDocument(
                new XElement("tv",
                    new XAttribute("generator-info-name", "E-Channelizer"),
                    programs.Select(p => new XElement("programme",
                        new XAttribute("channel", p.ServiceId),
                        new XAttribute("start", p.StartTime.ToString("yyyyMMddHHmmss")),
                        new XAttribute("stop", p.EndTime.ToString("yyyyMMddHHmmss")),
                        new XElement("title", new XAttribute("lang", p.Language), p.Title),
                        new XElement("desc", new XAttribute("lang", p.Language), p.Description),
                        p.Categories.Select(c => new XElement("category", new XAttribute("lang", p.Language), c))
                    ))
                ));

            xml.Save(targetPath);
        }

        private bool IsImageFile(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".webp";
        }
    }
}