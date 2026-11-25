using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using PropertyChanged;

namespace EChannelizer.Models
{
    /// <summary>
    /// Satellite information
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class Satellite : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = string.Empty;
        public string UniqueKey { get; set; } = Guid.NewGuid().ToString();
        public double OrbitalPosition { get; set; } // degrees
        public SatelliteDirection Direction { get; set; } = SatelliteDirection.East;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public bool IsFavorite { get; set; }
        public List<TransponderBase> Transponders { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();

        // Calculated properties
        public string PositionDisplay => $"{OrbitalPosition:F1}°{(Direction == SatelliteDirection.East ? "E" : "W")}";
        public int TransponderCount => Transponders.Count;
        public int ChannelCount => Transponders.Sum(t => t.ServiceIds.Count);

        public object Clone()
        {
            return new Satellite
            {
                Name = Name,
                UniqueKey = Guid.NewGuid().ToString(),
                OrbitalPosition = OrbitalPosition,
                Direction = Direction,
                Region = Region,
                Country = Country,
                Language = Language,
                IsEnabled = IsEnabled,
                IsFavorite = IsFavorite,
                Transponders = Transponders.ConvertAll(t => t.Clone()),
                Metadata = new Dictionary<string, object>(Metadata)
            };
        }

        public override string ToString() => $"{Name} ({PositionDisplay})";

        public override bool Equals(object? obj)
        {
            if (obj is Satellite other)
            {
                return UniqueKey == other.UniqueKey ||
                       (Math.Abs(OrbitalPosition - other.OrbitalPosition) < 0.1 && Direction == other.Direction);
            }
            return false;
        }

        public override int GetHashCode() => UniqueKey.GetHashCode();
    }

    /// <summary>
    /// Web Satellite (from online databases)
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class WebSatellite : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = string.Empty;
        public double OrbitalPosition { get; set; }
        public SatelliteDirection Direction { get; set; } = SatelliteDirection.East;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public List<WebTransponder> Transponders { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public string SourceUrl { get; set; } = string.Empty;
        public bool IsSelected { get; set; }

        public string PositionDisplay => $"{OrbitalPosition:F1}°{(Direction == SatelliteDirection.East ? "E" : "W")}";

        public Satellite ToSatellite()
        {
            return new Satellite
            {
                Name = Name,
                OrbitalPosition = OrbitalPosition,
                Direction = Direction,
                Region = Region,
                Country = Country,
                Transponders = Transponders.ConvertAll(t => t.ToTransponder())
            };
        }
    }

    /// <summary>
    /// Web Transponder (from online databases)
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class WebTransponder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public long Frequency { get; set; }
        public Polarization Polarization { get; set; } = Polarization.Horizontal;
        public int SymbolRate { get; set; }
        public string FEC { get; set; } = "Auto";
        public string Modulation { get; set; } = "QPSK";
        public bool IsDvbS2 { get; set; } = true;
        public List<WebService> Services { get; set; } = new();
        public bool IsSelected { get; set; }

        public TransponderBase ToTransponder()
        {
            return new TransponderDVBs
            {
                Frequency = Frequency,
                Polarization = Polarization,
                SymbolRate = SymbolRate,
                FEC = FEC,
                Modulation = Modulation,
                IsDvbS2 = IsDvbS2,
                ServiceIds = Services.ConvertAll(s => s.ServiceId)
            };
        }
    }

    /// <summary>
    /// Web Service (from online databases)
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class WebService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public ServiceType ServiceType { get; set; } = ServiceType.TV;
        public bool IsScrambled { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Satellite Database for online imports
    /// </summary>
    public class SatelliteDatabase
    {
        public List<WebSatellite> Satellites { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public string Version { get; set; } = "1.0";
        public string Source { get; set; } = string.Empty;

        public static SatelliteDatabase LoadFromXml(string xmlPath)
        {
            // Implementation for loading from XML (like satellites.xml from KingOfSat, FlySat, etc.)
            return new SatelliteDatabase();
        }

        public static SatelliteDatabase LoadFromJson(string jsonPath)
        {
            var json = System.IO.File.ReadAllText(jsonPath);
            return JsonConvert.DeserializeObject<SatelliteDatabase>(json) ?? new SatelliteDatabase();
        }

        public void SaveToJson(string jsonPath)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(jsonPath, json);
        }
    }
}