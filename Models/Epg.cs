using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using PropertyChanged;

namespace EChannelizer.Models
{
    /// <summary>
    /// EPG Source configuration
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class EpgSource : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = string.Empty;
        public string UniqueKey { get; set; } = Guid.NewGuid().ToString();
        public EpgSourceType Type { get; set; } = EpgSourceType.XMLTV;
        public string Url { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public int UpdateInterval { get; set; } = 24; // hours
        public DateTime LastUpdate { get; set; }
        public DateTime NextUpdate { get; set; }
        public List<string> ChannelMappings { get; set; } = new();
        public Dictionary<string, string> CustomParameters { get; set; } = new();
        public bool IsSelected { get; set; }

        public object Clone()
        {
            return new EpgSource
            {
                Name = Name,
                UniqueKey = Guid.NewGuid().ToString(),
                Type = Type,
                Url = Url,
                FilePath = FilePath,
                Username = Username,
                Password = Password,
                IsEnabled = IsEnabled,
                UpdateInterval = UpdateInterval,
                LastUpdate = LastUpdate,
                NextUpdate = NextUpdate,
                ChannelMappings = new List<string>(ChannelMappings),
                CustomParameters = new Dictionary<string, string>(CustomParameters)
            };
        }
    }

    /// <summary>
    /// EPG Program / Event
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class EpgProgram : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string UniqueKey { get; set; } = Guid.NewGuid().ToString();
        public int ServiceId { get; set; }
        public int TransportStreamId { get; set; }
        public int NetworkId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public List<string> Categories { get; set; } = new();
        public string Language { get; set; } = string.Empty;
        public int ParentalRating { get; set; } = 0;
        public string EpisodeNumber { get; set; } = string.Empty;
        public string SeasonNumber { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string EpisodeId { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public bool IsLive { get; set; }
        public bool IsRepeat { get; set; }
        public bool IsPremiere { get; set; }
        public bool IsFinale { get; set; }
        public List<string> Actors { get; set; } = new();
        public List<string> Directors { get; set; } = new();
        public List<string> Writers { get; set; } = new();
        public string ImageUrl { get; set; } = string.Empty;
        public Dictionary<string, object> ExtendedData { get; set; } = new();

        public bool IsCurrentlyAiring => DateTime.Now >= StartTime && DateTime.Now <= EndTime;
        public bool IsUpcoming => DateTime.Now < StartTime;
        public bool HasEnded => DateTime.Now > EndTime;

        public object Clone()
        {
            return new EpgProgram
            {
                UniqueKey = Guid.NewGuid().ToString(),
                ServiceId = ServiceId,
                TransportStreamId = TransportStreamId,
                NetworkId = NetworkId,
                Title = Title,
                Description = Description,
                ShortDescription = ShortDescription,
                StartTime = StartTime,
                EndTime = EndTime,
                Categories = new List<string>(Categories),
                Language = Language,
                ParentalRating = ParentalRating,
                EpisodeNumber = EpisodeNumber,
                SeasonNumber = SeasonNumber,
                SeriesId = SeriesId,
                EpisodeId = EpisodeId,
                IsNew = IsNew,
                IsLive = IsLive,
                IsRepeat = IsRepeat,
                IsPremiere = IsPremiere,
                IsFinale = IsFinale,
                Actors = new List<string>(Actors),
                Directors = new List<string>(Directors),
                Writers = new List<string>(Writers),
                ImageUrl = ImageUrl,
                ExtendedData = new Dictionary<string, object>(ExtendedData)
            };
        }

        public override string ToString() => $"{StartTime:HH:mm} - {Title}";
    }

    /// <summary>
    /// XMLTV Channel mapping
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class XmlTvChannel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> DisplayNames { get; set; } = new();
        public string IconUrl { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public int TransportStreamId { get; set; }
        public int NetworkId { get; set; }
        public bool IsMapped { get; set; }
        public IChannel? MappedChannel { get; set; }
    }

    /// <summary>
    /// XMLTV Program
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class XmlTvProgram : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string ChannelId { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new();
        public string Language { get; set; } = string.Empty;
        public int ParentalRating { get; set; }
        public string EpisodeNum { get; set; } = string.Empty;
        public string SeasonNum { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string EpisodeId { get; set; } = string.Empty;
        public bool New { get; set; }
        public bool Live { get; set; }
        public bool Repeat { get; set; }
        public List<XmlTvCredit> Credits { get; set; } = new();
        public List<XmlTvIcon> Icons { get; set; } = new();
        public Dictionary<string, string> ExtendedData { get; set; } = new();
    }

    /// <summary>
    /// XMLTV Credit
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class XmlTvCredit : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Role { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// XMLTV Icon
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class XmlTvIcon : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Src { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// EPG Manager for handling EPG data
    /// </summary>
    public class EpgManager
    {
        private readonly Dictionary<int, List<EpgProgram>> _epgData = new();
        private readonly object _lock = new();

        public void AddProgram(EpgProgram program)
        {
            lock (_lock)
            {
                var key = program.ServiceId;
                if (!_epgData.ContainsKey(key))
                    _epgData[key] = new List<EpgProgram>();
                _epgData[key].Add(program);
            }
        }

        public void AddPrograms(IEnumerable<EpgProgram> programs)
        {
            lock (_lock)
            {
                foreach (var program in programs)
                {
                    var key = program.ServiceId;
                    if (!_epgData.ContainsKey(key))
                        _epgData[key] = new List<EpgProgram>();
                    _epgData[key].Add(program);
                }
            }
        }

        public List<EpgProgram> GetProgramsForService(int serviceId, DateTime? startTime = null, DateTime? endTime = null)
        {
            lock (_lock)
            {
                if (!_epgData.TryGetValue(serviceId, out var programs))
                    return new List<EpgProgram>();

                var query = programs.AsEnumerable();

                if (startTime.HasValue)
                    query = query.Where(p => p.EndTime >= startTime.Value);

                if (endTime.HasValue)
                    query = query.Where(p => p.StartTime <= endTime.Value);

                return query.OrderBy(p => p.StartTime).ToList();
            }
        }

        public EpgProgram? GetCurrentProgram(int serviceId)
        {
            lock (_lock)
            {
                if (!_epgData.TryGetValue(serviceId, out var programs))
                    return null;

                var now = DateTime.Now;
                return programs.FirstOrDefault(p => p.StartTime <= now && p.EndTime >= now);
            }
        }

        public EpgProgram? GetNextProgram(int serviceId)
        {
            lock (_lock)
            {
                if (!_epgData.TryGetValue(serviceId, out var programs))
                    return null;

                var now = DateTime.Now;
                return programs.Where(p => p.StartTime > now).OrderBy(p => p.StartTime).FirstOrDefault();
            }
        }

        public void ClearService(int serviceId)
        {
            lock (_lock)
            {
                _epgData.Remove(serviceId);
            }
        }

        public void ClearAll()
        {
            lock (_lock)
            {
                _epgData.Clear();
            }
        }

        public void RemoveOldPrograms(DateTime beforeDate)
        {
            lock (_lock)
            {
                foreach (var key in _epgData.Keys.ToList())
                {
                    _epgData[key].RemoveAll(p => p.EndTime < beforeDate);
                    if (_epgData[key].Count == 0)
                        _epgData.Remove(key);
                }
            }
        }

        public int TotalPrograms => _epgData.Values.Sum(list => list.Count);
        public int ServicesWithEpg => _epgData.Count;
    }
}