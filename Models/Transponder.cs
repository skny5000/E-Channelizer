using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using PropertyChanged;

namespace EChannelizer.Models
{
    /// <summary>
    /// Base transponder class
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public abstract class TransponderBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = string.Empty;
        public string UniqueKey { get; set; } = Guid.NewGuid().ToString();
        public DvbStandard Standard { get; set; }
        public string Satellite { get; set; } = string.Empty;
        public int NetworkId { get; set; }
        public int TransportStreamId { get; set; }
        public bool IsScanned { get; set; }
        public DateTime LastScanned { get; set; }
        public List<int> ServiceIds { get; set; } = new();
        public ServiceFlags Flags { get; set; } = ServiceFlags.None;

        public virtual TransponderBase Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject(json, GetType()) as TransponderBase!;
        }
    }

    /// <summary>
    /// DVB-S/S2 Satellite Transponder
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class TransponderDVBs : TransponderBase
    {
        public long Frequency { get; set; } // kHz
        public Polarization Polarization { get; set; } = Polarization.Horizontal;
        public int SymbolRate { get; set; } // ksym/s
        public string FEC { get; set; } = "Auto";
        public string Modulation { get; set; } = "QPSK";
        public int Pilot { get; set; } = 0;
        public int RollOff { get; set; } = 35;
        public bool IsDvbS2 { get; set; } = true;
        public int PLPId { get; set; } = -1;
        public string T2MI { get; set; } = string.Empty;
        public SatelliteDirection Direction { get; set; } = SatelliteDirection.East;
        public double OrbitalPosition { get; set; } // degrees

        // Calculated properties
        public string FrequencyDisplay => $"{Frequency / 1000.0:F3} GHz";
        public string SymbolRateDisplay => $"{SymbolRate / 1000.0:F3} Msym/s";
        public string PolarizationDisplay => Polarization.ToString();
        public string StandardDisplay => IsDvbS2 ? "DVB-S2" : "DVB-S";

        public override TransponderBase Clone()
        {
            return new TransponderDVBs
            {
                Name = Name,
                UniqueKey = Guid.NewGuid().ToString(),
                Standard = Standard,
                Satellite = Satellite,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                IsScanned = IsScanned,
                LastScanned = LastScanned,
                ServiceIds = new List<int>(ServiceIds),
                Flags = Flags,
                Frequency = Frequency,
                Polarization = Polarization,
                SymbolRate = SymbolRate,
                FEC = FEC,
                Modulation = Modulation,
                Pilot = Pilot,
                RollOff = RollOff,
                IsDvbS2 = IsDvbS2,
                PLPId = PLPId,
                T2MI = T2MI,
                Direction = Direction,
                OrbitalPosition = OrbitalPosition
            };
        }
    }

    /// <summary>
    /// DVB-C/C2 Cable Transponder
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class TransponderDVBc : TransponderBase
    {
        public long Frequency { get; set; } // kHz
        public int SymbolRate { get; set; } // ksym/s
        public string Modulation { get; set; } = "256QAM";
        public string FEC { get; set; } = "Auto";
        public bool IsDvbC2 { get; set; } = false;
        public int PLPId { get; set; } = -1;

        public string FrequencyDisplay => $"{Frequency / 1000.0:F3} GHz";
        public string SymbolRateDisplay => $"{SymbolRate / 1000.0:F3} Msym/s";
        public string StandardDisplay => IsDvbC2 ? "DVB-C2" : "DVB-C";

        public override TransponderBase Clone()
        {
            return new TransponderDVBc
            {
                Name = Name,
                UniqueKey = Guid.NewGuid().ToString(),
                Standard = Standard,
                Satellite = Satellite,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                IsScanned = IsScanned,
                LastScanned = LastScanned,
                ServiceIds = new List<int>(ServiceIds),
                Flags = Flags,
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
    /// DVB-T/T2 Terrestrial Transponder
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class TransponderDVBt : TransponderBase
    {
        public long Frequency { get; set; } // kHz
        public int Bandwidth { get; set; } = 8; // MHz
        public string Modulation { get; set; } = "64QAM";
        public string FEC { get; set; } = "Auto";
        public string GuardInterval { get; set; } = "1/32";
        public string FFTMode { get; set; } = "8K";
        public bool IsDvbT2 { get; set; } = true;
        public int PLPId { get; set; } = -1;
        public int CellId { get; set; } = -1;

        public string FrequencyDisplay => $"{Frequency / 1000.0:F3} GHz";
        public string StandardDisplay => IsDvbT2 ? "DVB-T2" : "DVB-T";

        public override TransponderBase Clone()
        {
            return new TransponderDVBt
            {
                Name = Name,
                UniqueKey = Guid.NewGuid().ToString(),
                Standard = Standard,
                Satellite = Satellite,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                IsScanned = IsScanned,
                LastScanned = LastScanned,
                ServiceIds = new List<int>(ServiceIds),
                Flags = Flags,
                Frequency = Frequency,
                Bandwidth = Bandwidth,
                Modulation = Modulation,
                FEC = FEC,
                GuardInterval = GuardInterval,
                FFTMode = FFTMode,
                IsDvbT2 = IsDvbT2,
                PLPId = PLPId,
                CellId = CellId
            };
        }
    }

    /// <summary>
    /// ATSC Transponder
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class TransponderATSC : TransponderBase
    {
        public long Frequency { get; set; } // kHz
        public int MajorChannel { get; set; }
        public int MinorChannel { get; set; }
        public string Modulation { get; set; } = "8VSB";
        public int ProgramNumber { get; set; }

        public string FrequencyDisplay => $"{Frequency / 1000.0:F3} GHz";
        public string StandardDisplay => "ATSC";

        public override TransponderBase Clone()
        {
            return new TransponderATSC
            {
                Name = Name,
                UniqueKey = Guid.NewGuid().ToString(),
                Standard = Standard,
                Satellite = Satellite,
                NetworkId = NetworkId,
                TransportStreamId = TransportStreamId,
                IsScanned = IsScanned,
                LastScanned = LastScanned,
                ServiceIds = new List<int>(ServiceIds),
                Flags = Flags,
                Frequency = Frequency,
                MajorChannel = MajorChannel,
                MinorChannel = MinorChannel,
                Modulation = Modulation,
                ProgramNumber = ProgramNumber
            };
        }
    }

    /// <summary>
    /// Transponder factory
    /// </summary>
    public static class TransponderFactory
    {
        public static TransponderBase Create(DvbStandard standard)
        {
            return standard switch
            {
                DvbStandard.DVBS or DvbStandard.DVBS2 => new TransponderDVBs { Standard = standard, IsDvbS2 = standard == DvbStandard.DVBS2 },
                DvbStandard.DVBC or DvbStandard.DVBC2 => new TransponderDVBc { Standard = standard, IsDvbC2 = standard == DvbStandard.DVBC2 },
                DvbStandard.DVBT or DvbStandard.DVBT2 => new TransponderDVBt { Standard = standard, IsDvbT2 = standard == DvbStandard.DVBT2 },
                DvbStandard.ATSC => new TransponderATSC { Standard = standard },
                _ => throw new ArgumentException($"Unsupported standard: {standard}")
            };
        }

        public static TransponderBase CreateFromChannel(IChannel channel)
        {
            return channel switch
            {
                DvbSChannel => new TransponderDVBs { Standard = DvbStandard.DVBS2, IsDvbS2 = true },
                DvbCChannel => new TransponderDVBc { Standard = DvbStandard.DVBC2, IsDvbC2 = true },
                DvbTChannel => new TransponderDVBt { Standard = DvbStandard.DVBT2, IsDvbT2 = true },
                AtscChannel => new TransponderATSC { Standard = DvbStandard.ATSC },
                _ => throw new ArgumentException($"Unsupported channel type: {channel.GetType().Name}")
            };
        }
    }
}