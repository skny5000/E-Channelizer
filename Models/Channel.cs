using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using PropertyChanged;

namespace EChannelizer.Models
{
    /// <summary>
    /// Base interface for all channel types
    /// </summary>
    public interface IChannel : INotifyPropertyChanged
    {
        string Name { get; set; }
        int Number { get; set; }
        ServiceFlags Flags { get; set; }
        string Provider { get; set; }
        string Satellite { get; set; }
        int ServiceId { get; set; }
        int NetworkId { get; set; }
        int TransportStreamId { get; set; }
        ServiceType ServiceType { get; set; }
        string LogoPath { get; set; }
        List<string> EpgSources { get; set; }
        DateTime LastScanned { get; set; }
        bool IsSelected { get; set; }
        bool IsVisible { get; set; }
        ChannelMarkerType Marker { get; set; }
        object Clone();
    }

    /// <summary>
    /// Base channel class with common properties
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public abstract class ChannelBase : IChannel
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = string.Empty;
        public int Number { get; set; }
        public ServiceFlags Flags { get; set; } = ServiceFlags.None;
        public string Provider { get; set; } = string.Empty;
        public string Satellite { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public int NetworkId { get; set; }
        public int TransportStreamId { get; set; }
        public ServiceType ServiceType { get; set; } = ServiceType.Unknown;
        public string LogoPath { get; set; } = string.Empty;
        public List<string> EpgSources { get; set; } = new();
        public DateTime LastScanned { get; set; } = DateTime.MinValue;
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; } = true;
        public ChannelMarkerType Marker { get; set; } = ChannelMarkerType.None;

        // Additional properties
        public string UniqueKey => $"{Satellite}_{TransportStreamId}_{ServiceId}";
        public bool IsScrambled => Flags.HasFlag(ServiceFlags.Scrambled);
        public bool IsHidden => Flags.HasFlag(ServiceFlags.Hidden);
        public bool IsFavorite => Flags.HasFlag(ServiceFlags.Favorite);
        public bool IsLocked => Flags.HasFlag(ServiceFlags.Locked);
        public bool IsSkip => Flags.HasFlag(ServiceFlags.Skip);
        public bool IsHD => Flags.HasFlag(ServiceFlags.HD);
        public bool IsUHD => Flags.HasFlag(ServiceFlags.UHD);
        public bool IsHEVC => Flags.HasFlag(ServiceFlags.HEVC);
        public bool IsMarker => Marker != ChannelMarkerType.None;

        public virtual object Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject(json, GetType())!;
        }

        public override string ToString() => $"{Number}: {Name} ({ServiceType})";

        public override bool Equals(object? obj)
        {
            if (obj is IChannel other)
            {
                return UniqueKey == other.UniqueKey;
            }
            return false;
        }

        public override int GetHashCode() => UniqueKey.GetHashCode();
    }

    /// <summary>
    /// DVB-S/S2 Satellite Channel
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class DvbSChannel : ChannelBase
    {
        public long Frequency { get; set; } // in kHz
        public Polarization Polarization { get; set; } = Polarization.Horizontal;
        public int SymbolRate { get; set; } // in ksym/s
        public string FEC { get; set; } = "Auto";
        public string Modulation { get; set; } = "QPSK";
        public int Pilot { get; set; } = 0;
        public int RollOff { get; set; } = 35; // 0.35 = 35%
        public bool IsDvbS2 { get; set; } = true;
        public int PLPId { get; set; } = -1; // Physical Layer Pipe ID for DVB-S2X
        public string T2MI { get; set; } = string.Empty;

        public override object Clone()
        {
            return new DvbSChannel
            {
                Name = Name,
                Number = Number,
                Flags = Flags,
                Provider = Provider,
                Satellite = Satellite,
                ServiceId = ServiceId,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                ServiceType = ServiceType,
                LogoPath = LogoPath,
                EpgSources = new List<string>(EpgSources),
                LastScanned = LastScanned,
                IsSelected = IsSelected,
                IsVisible = IsVisible,
                Marker = Marker,
                Frequency = Frequency,
                Polarization = Polarization,
                SymbolRate = SymbolRate,
                FEC = FEC,
                Modulation = Modulation,
                Pilot = Pilot,
                RollOff = RollOff,
                IsDvbS2 = IsDvbS2,
                PLPId = PLPId,
                T2MI = T2MI
            };
        }
    }

    /// <summary>
    /// DVB-C/C2 Cable Channel
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class DvbCChannel : ChannelBase
    {
        public long Frequency { get; set; } // in kHz
        public int SymbolRate { get; set; } // in ksym/s
        public string Modulation { get; set; } = "256QAM";
        public string FEC { get; set; } = "Auto";
        public bool IsDvbC2 { get; set; } = false;
        public int PLPId { get; set; } = -1;

        public override object Clone()
        {
            return new DvbCChannel
            {
                Name = Name,
                Number = Number,
                Flags = Flags,
                Provider = Provider,
                Satellite = Satellite, // Network name for cable
                ServiceId = ServiceId,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                ServiceType = ServiceType,
                LogoPath = LogoPath,
                EpgSources = new List<string>(EpgSources),
                LastScanned = LastScanned,
                IsSelected = IsSelected,
                IsVisible = IsVisible,
                Marker = Marker,
                Frequency = Frequency,
                SymbolRate = SymbolRate,
                Modulation = Modulation,
                FEC = FEC,
                IsDvbC2 = IsDvbC2,
                PLPId = PLPId
            };
        }
    }

    /// <summary>
    /// DVB-T/T2 Terrestrial Channel
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class DvbTChannel : ChannelBase
    {
        public long Frequency { get; set; } // in kHz
        public int Bandwidth { get; set; } = 8; // MHz
        public string Modulation { get; set; } = "64QAM";
        public string FEC { get; set; } = "Auto";
        public string GuardInterval { get; set; } = "1/32";
        public string FFTMode { get; set; } = "8K";
        public bool IsDvbT2 { get; set; } = true;
        public int PLPId { get; set; } = -1;

        public override object Clone()
        {
            return new DvbTChannel
            {
                Name = Name,
                Number = Number,
                Flags = Flags,
                Provider = Provider,
                Satellite = Satellite, // Network name for terrestrial
                ServiceId = ServiceId,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                ServiceType = ServiceType,
                LogoPath = LogoPath,
                EpgSources = new List<string>(EpgSources),
                LastScanned = LastScanned,
                IsSelected = IsSelected,
                IsVisible = IsVisible,
                Marker = Marker,
                Frequency = Frequency,
                Bandwidth = Bandwidth,
                Modulation = Modulation,
                FEC = FEC,
                GuardInterval = GuardInterval,
                FFTMode = FFTMode,
                IsDvbT2 = IsDvbT2,
                PLPId = PLPId
            };
        }
    }

    /// <summary>
    /// ATSC Channel (North America)
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class AtscChannel : ChannelBase
    {
        public long Frequency { get; set; } // in kHz
        public int MajorChannel { get; set; }
        public int MinorChannel { get; set; }
        public string Modulation { get; set; } = "8VSB";
        public int ProgramNumber { get; set; }

        public override object Clone()
        {
            return new AtscChannel
            {
                Name = Name,
                Number = Number,
                Flags = Flags,
                Provider = Provider,
                Satellite = Satellite,
                ServiceId = ServiceId,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                ServiceType = ServiceType,
                LogoPath = LogoPath,
                EpgSources = new List<string>(EpgSources),
                LastScanned = LastScanned,
                IsSelected = IsSelected,
                IsVisible = IsVisible,
                Marker = Marker,
                Frequency = Frequency,
                MajorChannel = MajorChannel,
                MinorChannel = MinorChannel,
                Modulation = Modulation,
                ProgramNumber = ProgramNumber
            };
        }
    }

    /// <summary>
    /// IPTV / HLS Stream Channel
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class IptvChannel : ChannelBase
    {
        public string Url { get; set; } = string.Empty;
        public StreamTsType StreamType { get; set; } = StreamTsType.HLS;
        public string UserAgent { get; set; } = string.Empty;
        public string Referer { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
        public StreamBuffering Buffering { get; set; } = StreamBuffering.Medium;
        public int BufferSize { get; set; } = 1024; // KB
        public int Timeout { get; set; } = 30; // seconds
        public bool UseProxy { get; set; }
        public string ProxyUrl { get; set; } = string.Empty;

        public override object Clone()
        {
            return new IptvChannel
            {
                Name = Name,
                Number = Number,
                Flags = Flags,
                Provider = Provider,
                Satellite = Satellite,
                ServiceId = ServiceId,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                ServiceType = ServiceType,
                LogoPath = LogoPath,
                EpgSources = new List<string>(EpgSources),
                LastScanned = LastScanned,
                IsSelected = IsSelected,
                IsVisible = IsVisible,
                Marker = Marker,
                Url = Url,
                StreamType = StreamType,
                UserAgent = UserAgent,
                Referer = Referer,
                Headers = new Dictionary<string, string>(Headers),
                Buffering = Buffering,
                BufferSize = BufferSize,
                Timeout = Timeout,
                UseProxy = UseProxy,
                ProxyUrl = ProxyUrl
            };
        }
    }

    /// <summary>
    /// Unknown/Generic Channel
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class UnknownChannel : ChannelBase
    {
        public Dictionary<string, object> Properties { get; set; } = new();

        public override object Clone()
        {
            return new UnknownChannel
            {
                Name = Name,
                Number = Number,
                Flags = Flags,
                Provider = Provider,
                Satellite = Satellite,
                ServiceId = ServiceId,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                ServiceType = ServiceType,
                LogoPath = LogoPath,
                EpgSources = new List<string>(EpgSources),
                LastScanned = LastScanned,
                IsSelected = IsSelected,
                IsVisible = IsVisible,
                Marker = Marker,
                Properties = new Dictionary<string, object>(Properties)
            };
        }
    }

    /// <summary>
    /// Channel Group / Bouquet
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class ChannelGroup : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = string.Empty;
        public string UniqueKey { get; set; } = Guid.NewGuid().ToString();
        public int Order { get; set; }
        public ServiceFlags Flags { get; set; } = ServiceFlags.None;
        public string Satellite { get; set; } = string.Empty;
        public BouquetSource Source { get; set; } = BouquetSource.User;
        public List<IChannel> Channels { get; set; } = new();
        public List<ChannelGroup> SubGroups { get; set; } = new();
        public ChannelGroup? Parent { get; set; }
        public bool IsExpanded { get; set; } = true;
        public bool IsSelected { get; set; }
        public string IconPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Modified { get; set; } = DateTime.Now;

        public int ChannelCount => Channels.Count;
        public int TotalChannels => Channels.Count + SubGroups.Sum(g => g.TotalChannels);
        public bool IsRoot => Parent == null;
        public int Level => Parent?.Level + 1 ?? 0;

        public void AddChannel(IChannel channel)
        {
            if (!Channels.Contains(channel))
            {
                Channels.Add(channel);
                Modified = DateTime.Now;
            }
        }

        public void RemoveChannel(IChannel channel)
        {
            Channels.Remove(channel);
            Modified = DateTime.Now;
        }

        public void AddSubGroup(ChannelGroup group)
        {
            group.Parent = this;
            SubGroups.Add(group);
            Modified = DateTime.Now;
        }

        public void RemoveSubGroup(ChannelGroup group)
        {
            group.Parent = null;
            SubGroups.Remove(group);
            Modified = DateTime.Now;
        }

        public IEnumerable<IChannel> GetAllChannels()
        {
            foreach (var channel in Channels)
                yield return channel;
            foreach (var group in SubGroups)
            {
                foreach (var channel in group.GetAllChannels())
                    yield return channel;
            }
        }

        public ChannelGroup Clone()
        {
            var clone = new ChannelGroup
            {
                Name = Name,
                UniqueKey = Guid.NewGuid().ToString(),
                Order = Order,
                Flags = Flags,
                Satellite = Satellite,
                Source = Source,
                IsExpanded = IsExpanded,
                IconPath = IconPath,
                Description = Description,
                Created = Created,
                Modified = Modified
            };

            foreach (var channel in Channels)
                clone.Channels.Add((IChannel)channel.Clone());

            foreach (var group in SubGroups)
                clone.AddSubGroup(group.Clone());

            return clone;
        }
    }

    /// <summary>
    /// Channel Alternatives (for same service on different transponders)
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class ChannelAlternatives
    {
        public IChannel MainChannel { get; set; } = null!;
        public List<IChannel> Alternatives { get; set; } = new();

        public void AddAlternative(IChannel channel)
        {
            if (!Alternatives.Contains(channel))
                Alternatives.Add(channel);
        }

        public void RemoveAlternative(IChannel channel)
        {
            Alternatives.Remove(channel);
        }
    }

    /// <summary>
    /// Channel Repository - manages all channels and groups
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class ChannelRepository : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = "Default";
        public string FilePath { get; set; } = string.Empty;
        public bool IsModified { get; set; }
        public DateTime LastSaved { get; set; }
        public List<IChannel> AllChannels { get; set; } = new();
        public List<ChannelGroup> Groups { get; set; } = new();
        public List<Transponder> Transponders { get; set; } = new();
        public List<Satellite> Satellites { get; set; } = new();
        public List<NetworkDevice> Devices { get; set; } = new();
        public List<EpgSource> EpgSources { get; set; } = new();
        public Dictionary<string, string> Logos { get; set; } = new();

        public int ChannelCount => AllChannels.Count;
        public int GroupCount => Groups.Count;
        public int TransponderCount => Transponders.Count;
        public int SatelliteCount => Satellites.Count;

        public void AddChannel(IChannel channel)
        {
            if (!AllChannels.Contains(channel))
            {
                AllChannels.Add(channel);
                IsModified = true;
            }
        }

        public void RemoveChannel(IChannel channel)
        {
            AllChannels.Remove(channel);
            // Remove from groups
            foreach (var group in Groups)
            {
                RemoveChannelFromGroup(group, channel);
            }
            IsModified = true;
        }

        private void RemoveChannelFromGroup(ChannelGroup group, IChannel channel)
        {
            group.RemoveChannel(channel);
            foreach (var subGroup in group.SubGroups)
                RemoveChannelFromGroup(subGroup, channel);
        }

        public IChannel? FindChannel(string uniqueKey)
        {
            return AllChannels.FirstOrDefault(c => c.UniqueKey == uniqueKey);
        }

        public List<IChannel> FindChannelsByName(string name)
        {
            return AllChannels.Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<IChannel> FindChannelsBySatellite(string satellite)
        {
            return AllChannels.Where(c => c.Satellite == satellite).ToList();
        }

        public List<IChannel> FindChannelsByProvider(string provider)
        {
            return AllChannels.Where(c => c.Provider == provider).ToList();
        }

        public List<IChannel> GetFavorites()
        {
            return AllChannels.Where(c => c.IsFavorite).ToList();
        }

        public List<IChannel> GetScrambled()
        {
            return AllChannels.Where(c => c.IsScrambled).ToList();
        }

        public List<IChannel> GetHDChannels()
        {
            return AllChannels.Where(c => c.IsHD || c.IsUHD).ToList();
        }

        public void SortChannels(ChannelSort sort)
        {
            switch (sort)
            {
                case ChannelSort.Name:
                    AllChannels.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    break;
                case ChannelSort.Number:
                    AllChannels.Sort((a, b) => a.Number.CompareTo(b.Number));
                    break;
                case ChannelSort.Frequency:
                    AllChannels.Sort((a, b) => GetFrequency(a).CompareTo(GetFrequency(b)));
                    break;
                case ChannelSort.ServiceId:
                    AllChannels.Sort((a, b) => a.ServiceId.CompareTo(b.ServiceId));
                    break;
                case ChannelSort.Type:
                    AllChannels.Sort((a, b) => a.ServiceType.CompareTo(b.ServiceType));
                    break;
                case ChannelSort.Provider:
                    AllChannels.Sort((a, b) => string.Compare(a.Provider, b.Provider, StringComparison.Ordinal));
                    break;
                case ChannelSort.Satellite:
                    AllChannels.Sort((a, b) => string.Compare(a.Satellite, b.Satellite, StringComparison.Ordinal));
                    break;
            }
            IsModified = true;
        }

        private long GetFrequency(IChannel channel)
        {
            return channel switch
            {
                DvbSChannel s => s.Frequency,
                DvbCChannel c => c.Frequency,
                DvbTChannel t => t.Frequency,
                AtscChannel a => a.Frequency,
                _ => 0
            };
        }

        public void Cleanup()
        {
            // Remove channels not in any group
            var channelsInGroups = new HashSet<IChannel>();
            foreach (var group in Groups)
            {
                foreach (var channel in group.GetAllChannels())
                    channelsInGroups.Add(channel);
            }

            AllChannels.RemoveAll(c => !channelsInGroups.Contains(c));
            IsModified = true;
        }

        public ChannelRepository Clone()
        {
            var clone = new ChannelRepository
            {
                Name = Name,
                FilePath = FilePath,
                IsModified = false,
                LastSaved = LastSaved
            };

            foreach (var channel in AllChannels)
                clone.AllChannels.Add((IChannel)channel.Clone());

            foreach (var group in Groups)
                clone.Groups.Add(group.Clone());

            foreach (var transponder in Transponders)
                clone.Transponders.Add(transponder.Clone());

            foreach (var satellite in Satellites)
                clone.Satellites.Add(satellite.Clone());

            foreach (var device in Devices)
                clone.Devices.Add(device.Clone());

            foreach (var epg in EpgSources)
                clone.EpgSources.Add(epg.Clone());

            foreach (var logo in Logos)
                clone.Logos.Add(logo.Key, logo.Value);

            return clone;
        }
    }
}