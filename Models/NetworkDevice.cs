using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using PropertyChanged;

namespace EChannelizer.Models
{
    /// <summary>
    /// Network Device (Enigma2, Dreambox, etc.)
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class NetworkDevice : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = string.Empty;
        public string UniqueKey { get; set; } = Guid.NewGuid().ToString();
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 80;
        public string Username { get; set; } = "root";
        public string Password { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
        public string DeviceType { get; set; } = "Enigma2"; // Enigma2, Neutrino, Dreambox, etc.
        public bool UseHttps { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsConnected { get; set; }
        public DateTime LastConnected { get; set; }
        public InstallationStatus InstallationStatus { get; set; } = InstallationStatus.NotInstalled;
        public Dictionary<string, object> Capabilities { get; set; } = new();
        public List<ChannelGroup> Bouquets { get; set; } = new();
        public Dictionary<string, string> Settings { get; set; } = new();

        public string ConnectionUrl => $"{(UseHttps ? "https" : "http")}://{IpAddress}:{Port}";

        public object Clone()
        {
            return new NetworkDevice
            {
                Name = Name,
                UniqueKey = Guid.NewGuid().ToString(),
                IpAddress = IpAddress,
                Port = Port,
                Username = Username,
                Password = Password,
                Model = Model,
                FirmwareVersion = FirmwareVersion,
                DeviceType = DeviceType,
                UseHttps = UseHttps,
                IsEnabled = IsEnabled,
                IsConnected = false,
                LastConnected = DateTime.MinValue,
                InstallationStatus = InstallationStatus.NotInstalled,
                Capabilities = new Dictionary<string, object>(Capabilities),
                Bouquets = Bouquets.ConvertAll(b => b.Clone()),
                Settings = new Dictionary<string, string>(Settings)
            };
        }

        public override string ToString() => $"{Name} ({IpAddress}:{Port})";
    }

    /// <summary>
    /// Device Info from network device
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class DeviceInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Model { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
        public string KernelVersion { get; set; } = string.Empty;
        public string DriverVersion { get; set; } = string.Empty;
        public string CpuInfo { get; set; } = string.Empty;
        public string MemoryInfo { get; set; } = string.Empty;
        public string DiskInfo { get; set; } = string.Empty;
        public List<string> Tuners { get; set; } = new();
        public List<string> Frontends { get; set; } = new();
        public Dictionary<string, string> Settings { get; set; } = new();
    }
}