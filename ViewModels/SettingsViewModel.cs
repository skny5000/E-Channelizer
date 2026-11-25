using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EChannelizer.Models;
using EChannelizer.Services;
using Microsoft.Extensions.Logging;

namespace EChannelizer.ViewModels
{
    /// <summary>
    /// Settings dialog ViewModel
    /// </summary>
    public partial class SettingsViewModel : ScreenViewModel
    {
        private readonly ILogger<SettingsViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly IProfileService _profileService;

        public SettingsViewModel(ILogger<SettingsViewModel> logger, IDialogService dialogService, IProfileService profileService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _profileService = profileService;

            Title = "Settings";

            // Initialize collections
            ChannelSortOptions = new ObservableCollection<ChannelSort> { ChannelSort.Number, ChannelSort.Name, ChannelSort.Frequency, ChannelSort.ServiceId, ChannelSort.Type, ChannelSort.Provider, ChannelSort.Satellite, ChannelSort.Favorites };
            ServiceSortOptions = new ObservableCollection<ServicesSort> { ServicesSort.Name, ServicesSort.ServiceId, ServicesSort.Type, ServicesSort.Provider, ServicesSort.Frequency, ServicesSort.Number };
            LogoFormats = new ObservableCollection<string> { "png", "jpg", "bmp", "webp" };
            BufferingOptions = new ObservableCollection<StreamBuffering> { StreamBuffering.None, StreamBuffering.Low, StreamBuffering.Medium, StreamBuffering.High };
        }

        [ObservableProperty]
        private Profile _profile = new();

        // UI Settings
        [ObservableProperty]
        private string _language = "en";

        [ObservableProperty]
        private ObservableCollection<string> _languages = new() { "en", "de", "fr", "es", "it", "nl", "pl", "tr", "ru", "ar" };

        [ObservableProperty]
        private string _theme = "Dark";

        [ObservableProperty]
        private ObservableCollection<string> _themes = new() { "Light", "Dark", "System" };

        [ObservableProperty]
        private string _accentColor = "Blue";

        [ObservableProperty]
        private ObservableCollection<string> _accentColors = new() { "Blue", "Red", "Green", "Orange", "Purple", "Teal", "Pink" };

        [ObservableProperty]
        private int _fontSize = 12;

        [ObservableProperty]
        private string _fontFamily = "Segoe UI";

        [ObservableProperty]
        private ObservableCollection<string> _fontFamilies = new() { "Segoe UI", "Arial", "Consolas", "Courier New", "Tahoma", "Verdana" };

        // Channel Settings
        [ObservableProperty]
        private bool _autoAssignNumbers = true;

        [ObservableProperty]
        private bool _autoAssignEpg = true;

        [ObservableProperty]
        private bool _autoDownloadLogos = true;

        [ObservableProperty]
        private string _logosFolder = string.Empty;

        [ObservableProperty]
        private int _maxLogoSize = 200;

        [ObservableProperty]
        private string _logoFormat = "png";

        // Additional properties for XAML
        [ObservableProperty]
        private ObservableCollection<ChannelSort> _channelSortOptions = new();

        [ObservableProperty]
        private ChannelSort _defaultChannelSort = ChannelSort.Number;

        [ObservableProperty]
        private ObservableCollection<ServicesSort> _serviceSortOptions = new();

        [ObservableProperty]
        private ObservableCollection<string> _logoFormats = new();

        [ObservableProperty]
        private ObservableCollection<StreamBuffering> _bufferingOptions = new();

        // Stream Settings
        [ObservableProperty]
        private string _streamlinkPath = string.Empty;

        [ObservableProperty]
        private string _playerPath = string.Empty;

        [ObservableProperty]
        private string _playerArguments = string.Empty;

        [ObservableProperty]
        private StreamBuffering _defaultBuffering = StreamBuffering.Medium;

        [ObservableProperty]
        private int _defaultBufferSize = 1024;

        [ObservableProperty]
        private int _streamTimeout = 30;

        // Network Settings
        [ObservableProperty]
        private int _connectionTimeout = 10;

        [ObservableProperty]
        private int _operationTimeout = 30;

        [ObservableProperty]
        private bool _autoConnect = false;

        // EPG Settings
        [ObservableProperty]
        private int _epgUpdateInterval = 24;

        [ObservableProperty]
        private int _epgRetentionDays = 7;

        [ObservableProperty]
        private bool _epgAutoUpdate = true;

        // Advanced Settings
        [ObservableProperty]
        private bool _enableLogging = true;

        [ObservableProperty]
        private string _logLevel = "Info";

        [ObservableProperty]
        private ObservableCollection<string> _logLevels = new() { "Trace", "Debug", "Info", "Warning", "Error", "Critical" };

        [ObservableProperty]
        private int _maxLogFiles = 10;

        [ObservableProperty]
        private int _maxLogSize = 10;

        [ObservableProperty]
        private bool _checkForUpdates = true;

        [ObservableProperty]
        private bool _autoUpdate = false;

        [ObservableProperty]
        private string _updateChannel = "Stable";

        [ObservableProperty]
        private ObservableCollection<string> _updateChannels = new() { "Stable", "Beta", "Alpha" };

        public ObservableCollection<ChannelSort> ChannelSortOptions
        {
            get => _channelSortOptions;
            set => SetProperty(ref _channelSortOptions, value);
        }

        public ChannelSort DefaultChannelSort
        {
            get => _defaultChannelSort;
            set => SetProperty(ref _defaultChannelSort, value);
        }

        public ObservableCollection<ServicesSort> ServiceSortOptions
        {
            get => _serviceSortOptions;
            set => SetProperty(ref _serviceSortOptions, value);
        }

        public ObservableCollection<string> LogoFormats
        {
            get => _logoFormats;
            set => SetProperty(ref _logoFormats, value);
        }

        public ObservableCollection<StreamBuffering> BufferingOptions
        {
            get => _bufferingOptions;
            set => SetProperty(ref _bufferingOptions, value);
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadFromProfile();
        }

        private void LoadFromProfile()
        {
            if (Profile?.Settings != null)
            {
                var s = Profile.Settings;

                Language = s.Language;
                Theme = s.Theme;
                AccentColor = s.AccentColor;
                FontSize = s.FontSize;
                FontFamily = s.FontFamily;

                AutoAssignNumbers = s.AutoAssignNumbers;
                AutoAssignEpg = s.AutoAssignEpg;
                AutoDownloadLogos = s.AutoDownloadLogos;
                LogosFolder = s.LogosFolder;
                MaxLogoSize = s.MaxLogoSize;
                LogoFormat = s.LogoFormat;

                StreamlinkPath = s.StreamlinkPath;
                PlayerPath = s.PlayerPath;
                PlayerArguments = s.PlayerArguments;
                DefaultBuffering = s.DefaultBuffering;
                DefaultBufferSize = s.DefaultBufferSize;
                StreamTimeout = s.StreamTimeout;

                ConnectionTimeout = s.ConnectionTimeout;
                OperationTimeout = s.OperationTimeout;
                AutoConnect = s.AutoConnect;

                EpgUpdateInterval = s.EpgUpdateInterval;
                EpgRetentionDays = s.EpgRetentionDays;
                EpgAutoUpdate = s.EpgAutoUpdate;

                EnableLogging = s.EnableLogging;
                LogLevel = s.LogLevel;
                MaxLogFiles = s.MaxLogFiles;
                MaxLogSize = s.MaxLogSize;
                CheckForUpdates = s.CheckForUpdates;
                AutoUpdate = s.AutoUpdate;
                UpdateChannel = s.UpdateChannel;
            }
        }

        private void SaveToProfile()
        {
            if (Profile?.Settings != null)
            {
                var s = Profile.Settings;

                s.Language = Language;
                s.Theme = Theme;
                s.AccentColor = AccentColor;
                s.FontSize = FontSize;
                s.FontFamily = FontFamily;

                s.AutoAssignNumbers = AutoAssignNumbers;
                s.AutoAssignEpg = AutoAssignEpg;
                s.AutoDownloadLogos = AutoDownloadLogos;
                s.LogosFolder = LogosFolder;
                s.MaxLogoSize = MaxLogoSize;
                s.LogoFormat = LogoFormat;

                s.StreamlinkPath = StreamlinkPath;
                s.PlayerPath = PlayerPath;
                s.PlayerArguments = PlayerArguments;
                s.DefaultBuffering = DefaultBuffering;
                s.DefaultBufferSize = DefaultBufferSize;
                s.StreamTimeout = StreamTimeout;

                s.ConnectionTimeout = ConnectionTimeout;
                s.OperationTimeout = OperationTimeout;
                s.AutoConnect = AutoConnect;

                s.EpgUpdateInterval = EpgUpdateInterval;
                s.EpgRetentionDays = EpgRetentionDays;
                s.EpgAutoUpdate = EpgAutoUpdate;

                s.EnableLogging = EnableLogging;
                s.LogLevel = LogLevel;
                s.MaxLogFiles = MaxLogFiles;
                s.MaxLogSize = MaxLogSize;
                s.CheckForUpdates = CheckForUpdates;
                s.AutoUpdate = AutoUpdate;
                s.UpdateChannel = UpdateChannel;
            }
        }

        [RelayCommand]
        private async Task BrowseLogosFolderAsync()
        {
            var folder = await _dialogService.ShowOpenFileDialogAsync("Select Logos Folder", "Folders|*.*");
            if (!string.IsNullOrEmpty(folder))
            {
                LogosFolder = Path.GetDirectoryName(folder) ?? folder;
            }
        }

        [RelayCommand]
        private async Task BrowseStreamlinkAsync()
        {
            var path = await _dialogService.ShowOpenFileDialogAsync("Select Streamlink", "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                StreamlinkPath = path;
            }
        }

        [RelayCommand]
        private async Task BrowsePlayerAsync()
        {
            var path = await _dialogService.ShowOpenFileDialogAsync("Select Media Player", "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                PlayerPath = path;
            }
        }

        [RelayCommand]
        private async Task BrowseLogFolderAsync()
        {
            var folder = await _dialogService.ShowOpenFileDialogAsync("Select Log Folder", "Folders|*.*");
            // Implementation for log folder
        }

        [RelayCommand]
        private async Task TestPlayerAsync()
        {
            if (!string.IsNullOrEmpty(PlayerPath) && File.Exists(PlayerPath))
            {
                try
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = PlayerPath,
                            Arguments = PlayerArguments.Replace("{url}", "https://test-streams.mux.dev/x36xhzz/x36xhzz.m3u8").Replace("{title}", "Test"),
                            UseShellExecute = false
                        }
                    };
                    process.Start();
                    await _dialogService.ShowInfoAsync("Player Test", "Player launched successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Player Test Failed", ex.Message);
                }
            }
        }

        [RelayCommand]
        private void OK()
        {
            SaveToProfile();
            Close(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }

        [RelayCommand]
        private async Task ResetToDefaultsAsync()
        {
            var confirm = await _dialogService.ShowConfirmAsync("Reset Settings", "Reset all settings to defaults?");
            if (confirm)
            {
                Profile.Settings = new ProfileSettings();
                LoadFromProfile();
            }
        }
    }
}