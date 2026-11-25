using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using PropertyChanged;

namespace EChannelizer.Models
{
    /// <summary>
    /// User Profile with settings
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class Profile : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = "Default";
        public string UniqueKey { get; set; } = Guid.NewGuid().ToString();
        public string RepositoryPath { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastUsed { get; set; } = DateTime.Now;
        public ProfileSettings Settings { get; set; } = new();
        public List<string> RecentFiles { get; set; } = new();
        public Dictionary<string, object> CustomData { get; set; } = new();

        public object Clone()
        {
            return new Profile
            {
                Name = Name,
                UniqueKey = Guid.NewGuid().ToString(),
                RepositoryPath = RepositoryPath,
                IsActive = false,
                Created = Created,
                LastUsed = DateTime.Now,
                Settings = Settings.Clone(),
                RecentFiles = new List<string>(RecentFiles),
                CustomData = new Dictionary<string, object>(CustomData)
            };
        }
    }

    /// <summary>
    /// Profile Settings
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class ProfileSettings : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // UI Settings
        public string Language { get; set; } = "en";
        public string Theme { get; set; } = "Dark";
        public string AccentColor { get; set; } = "Blue";
        public LayoutComponents VisibleComponents { get; set; } = LayoutComponents.All;
        public ChannelOptions ChannelColumnOptions { get; set; } = ChannelOptions.All;
        public DevicesOptions DeviceColumnOptions { get; set; } = DevicesOptions.All;
        public ServicesSort ServiceSortOrder { get; set; } = ServicesSort.Name;
        public ChannelSort ChannelSortOrder { get; set; } = ChannelSort.Number;
        public bool ShowToolTips { get; set; } = true;
        public bool ShowKeyboardShortcuts { get; set; } = true;
        public int FontSize { get; set; } = 12;
        public string FontFamily { get; set; } = "Segoe UI";

        // Channel Settings
        public bool AutoAssignNumbers { get; set; } = true;
        public bool AutoAssignEpg { get; set; } = true;
        public bool AutoDownloadLogos { get; set; } = true;
        public string LogosFolder { get; set; } = string.Empty;
        public int MaxLogoSize { get; set; } = 200; // KB
        public string LogoFormat { get; set; } = "png";

        // Import/Export Settings
        public ImportSatellitesOptions DefaultImportOptions { get; set; } = ImportSatellitesOptions.All;
        public ImportDvbOptions DefaultDvbImportOptions { get; set; } = ImportDvbOptions.All;
        public bool BackupBeforeImport { get; set; } = true;
        public bool CreateGroupsOnImport { get; set; } = true;

        // Stream Settings
        public string StreamlinkPath { get; set; } = string.Empty;
        public StreamBuffering DefaultBuffering { get; set; } = StreamBuffering.Medium;
        public int DefaultBufferSize { get; set; } = 1024;
        public int StreamTimeout { get; set; } = 30;
        public string PlayerPath { get; set; } = string.Empty;
        public string PlayerArguments { get; set; } = string.Empty;

        // Network Settings
        public int ConnectionTimeout { get; set; } = 10;
        public int OperationTimeout { get; set; } = 30;
        public bool AutoConnect { get; set; } = false;

        // EPG Settings
        public int EpgUpdateInterval { get; set; } = 24;
        public int EpgRetentionDays { get; set; } = 7;
        public bool EpgAutoUpdate { get; set; } = true;

        // Advanced Settings
        public bool EnableLogging { get; set; } = true;
        public string LogLevel { get; set; } = "Info";
        public int MaxLogFiles { get; set; } = 10;
        public int MaxLogSize { get; set; } = 10; // MB
        public bool CheckForUpdates { get; set; } = true;
        public bool AutoUpdate { get; set; } = false;
        public string UpdateChannel { get; set; } = "Stable";

        public ProfileSettings Clone()
        {
            return new ProfileSettings
            {
                Language = Language,
                Theme = Theme,
                AccentColor = AccentColor,
                VisibleComponents = VisibleComponents,
                ChannelColumnOptions = ChannelColumnOptions,
                DeviceColumnOptions = DeviceColumnOptions,
                ServiceSortOrder = ServiceSortOrder,
                ChannelSortOrder = ChannelSortOrder,
                ShowToolTips = ShowToolTips,
                ShowKeyboardShortcuts = ShowKeyboardShortcuts,
                FontSize = FontSize,
                FontFamily = FontFamily,
                AutoAssignNumbers = AutoAssignNumbers,
                AutoAssignEpg = AutoAssignEpg,
                AutoDownloadLogos = AutoDownloadLogos,
                LogosFolder = LogosFolder,
                MaxLogoSize = MaxLogoSize,
                LogoFormat = LogoFormat,
                DefaultImportOptions = DefaultImportOptions,
                DefaultDvbImportOptions = DefaultDvbImportOptions,
                BackupBeforeImport = BackupBeforeImport,
                CreateGroupsOnImport = CreateGroupsOnImport,
                StreamlinkPath = StreamlinkPath,
                DefaultBuffering = DefaultBuffering,
                DefaultBufferSize = DefaultBufferSize,
                StreamTimeout = StreamTimeout,
                PlayerPath = PlayerPath,
                PlayerArguments = PlayerArguments,
                ConnectionTimeout = ConnectionTimeout,
                OperationTimeout = OperationTimeout,
                AutoConnect = AutoConnect,
                EpgUpdateInterval = EpgUpdateInterval,
                EpgRetentionDays = EpgRetentionDays,
                EpgAutoUpdate = EpgAutoUpdate,
                EnableLogging = EnableLogging,
                LogLevel = LogLevel,
                MaxLogFiles = MaxLogFiles,
                MaxLogSize = MaxLogSize,
                CheckForUpdates = CheckForUpdates,
                AutoUpdate = AutoUpdate,
                UpdateChannel = UpdateChannel
            };
        }
    }

    /// <summary>
    /// Application Configuration
    /// </summary>
    public class AppConfiguration
    {
        public string Version { get; set; } = "1.0.0";
        public List<Profile> Profiles { get; set; } = new();
        public string ActiveProfileKey { get; set; } = string.Empty;
        public Dictionary<string, string> GlobalSettings { get; set; } = new();
        public List<string> RecentRepositories { get; set; } = new();
        public DateTime LastUpdateCheck { get; set; }
        public bool FirstRun { get; set; } = true;

        public Profile? ActiveProfile => Profiles.FirstOrDefault(p => p.UniqueKey == ActiveProfileKey) ?? Profiles.FirstOrDefault();
    }
}