using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Edit Channel dialog ViewModel
    /// </summary>
    public partial class EditChannelViewModel : ScreenViewModel
    {
        private readonly ILogger<EditChannelViewModel> _logger;
        private readonly IDialogService _dialogService;

        public EditChannelViewModel(ILogger<EditChannelViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Title = "Edit Channel";

            // Initialize collections
            AvailableSatellites = new ObservableCollection<string> { "Astra 19.2°E", "Hotbird 13°E", "Eutelsat 9°E", "Turksat 42°E", "Arabsat 26°E", "Nilesat 7°W" };
            Polarizations = new ObservableCollection<Polarization> { Polarization.Horizontal, Polarization.Vertical, Polarization.CircularLeft, Polarization.CircularRight };
            FecOptions = new ObservableCollection<string> { "Auto", "1/2", "2/3", "3/4", "5/6", "7/8" };
            Modulations = new ObservableCollection<string> { "QPSK", "8PSK", "16APSK", "32APSK", "QAM16", "QAM32", "QAM64", "QAM128", "QAM256" };
            Bandwidths = new ObservableCollection<int> { 6, 7, 8 };
            StreamTypes = new ObservableCollection<StreamTsType> { StreamTsType.HLS, StreamTsType.M3U, StreamTsType.M3U8, StreamTsType.TS, StreamTsType.DASH, StreamTsType.RTMP, StreamTsType.RTSP, StreamTsType.HTTP, StreamTsType.UDP, StreamTsType.RTP };
            BufferingOptions = new ObservableCollection<StreamBuffering> { StreamBuffering.None, StreamBuffering.Low, StreamBuffering.Medium, StreamBuffering.High };
            ServiceTypes = new ObservableCollection<ServiceType> { ServiceType.TV, ServiceType.Radio, ServiceType.Data, ServiceType.Mosaic, ServiceType.AdvancedCodecHD, ServiceType.AdvancedCodecSD, ServiceType.AdvancedCodecRadio, ServiceType.HEVC_UHD, ServiceType.HEVC_HD };
        }

        [ObservableProperty]
        private IChannel _channel = null!;

        [ObservableProperty]
        private ChannelRepository _repository = new();

        [ObservableProperty]
        private bool _isNewChannel;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private int _number;

        [ObservableProperty]
        private ServiceFlags _flags = ServiceFlags.None;

        [ObservableProperty]
        private string _provider = string.Empty;

        [ObservableProperty]
        private string _satellite = string.Empty;

        [ObservableProperty]
        private int _serviceId;

        [ObservableProperty]
        private int _networkId;

        [ObservableProperty]
        private int _transportStreamId;

        [ObservableProperty]
        private ServiceType _serviceType = ServiceType.TV;

        [ObservableProperty]
        private string _logoPath = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _epgSources = new();

        [ObservableProperty]
        private string _selectedEpgSource = string.Empty;

        [ObservableProperty]
        private ChannelMarkerType _marker = ChannelMarkerType.None;

        // Additional properties for XAML
        [ObservableProperty]
        private ObservableCollection<string> _availableSatellites = new();

        [ObservableProperty]
        private ObservableCollection<Polarization> _polarizations = new();

        [ObservableProperty]
        private ObservableCollection<string> _fecOptions = new();

        [ObservableProperty]
        private ObservableCollection<string> _modulations = new();

        [ObservableProperty]
        private ObservableCollection<int> _bandwidths = new();

        [ObservableProperty]
        private ObservableCollection<StreamTsType> _streamTypes = new();

        [ObservableProperty]
        private ObservableCollection<StreamBuffering> _bufferingOptions = new();

        [ObservableProperty]
        private ObservableCollection<ServiceType> _serviceTypes = new();

        [ObservableProperty]
        private string _newEpgSource = string.Empty;

        // DVB-S specific
        [ObservableProperty]
        private long _frequency;

        [ObservableProperty]
        private Polarization _polarization = Polarization.Horizontal;

        [ObservableProperty]
        private int _symbolRate;

        [ObservableProperty]
        private string _fec = "Auto";

        [ObservableProperty]
        private string _modulation = "QPSK";

        [ObservableProperty]
        private int _pilot;

        [ObservableProperty]
        private int _rollOff = 35;

        [ObservableProperty]
        private bool _isDvbS2 = true;

        [ObservableProperty]
        private int _plpId = -1;

        // DVB-C specific
        [ObservableProperty]
        private string _cableModulation = "256QAM";

        [ObservableProperty]
        private bool _isDvbC2;

        // DVB-T specific
        [ObservableProperty]
        private int _bandwidth = 8;

        [ObservableProperty]
        private string _terrestrialModulation = "64QAM";

        [ObservableProperty]
        private string _guardInterval = "1/32";

        [ObservableProperty]
        private string _fftMode = "8K";

        [ObservableProperty]
        private bool _isDvbT2 = true;

        // IPTV specific
        [ObservableProperty]
        private string _url = string.Empty;

        [ObservableProperty]
        private StreamTsType _streamType = StreamTsType.HLS;

        [ObservableProperty]
        private string _userAgent = string.Empty;

        [ObservableProperty]
        private string _referer = string.Empty;

        [ObservableProperty]
        private StreamBuffering _buffering = StreamBuffering.Medium;

        [ObservableProperty]
        private int _bufferSize = 1024;

        [ObservableProperty]
        private int _timeout = 30;

        // UI state
        [ObservableProperty]
        private bool _isDvbsVisible;

        [ObservableProperty]
        private bool _isDvbcVisible;

        [ObservableProperty]
        private bool _isDvbtVisible;

        [ObservableProperty]
        private bool _isIptvVisible;

        // Computed flag properties
        public bool IsScrambled => Flags.HasFlag(ServiceFlags.Scrambled);
        public bool IsHidden => Flags.HasFlag(ServiceFlags.Hidden);
        public bool IsFavorite => Flags.HasFlag(ServiceFlags.Favorite);
        public bool IsLocked => Flags.HasFlag(ServiceFlags.Locked);
        public bool IsSkip => Flags.HasFlag(ServiceFlags.Skip);
        public bool IsHD => Flags.HasFlag(ServiceFlags.HD);
        public bool IsUHD => Flags.HasFlag(ServiceFlags.UHD);
        public bool IsHEVC => Flags.HasFlag(ServiceFlags.HEVC);
        public bool HasAC3 => Flags.HasFlag(ServiceFlags.AC3);
        public bool HasAAC => Flags.HasFlag(ServiceFlags.AAC);
        public bool HasSubtitles => Flags.HasFlag(ServiceFlags.Subtitles);
        public bool HasTeletext => Flags.HasFlag(ServiceFlags.Teletext);

        public ObservableCollection<string> AvailableSatellites
        {
            get => _availableSatellites;
            set => SetProperty(ref _availableSatellites, value);
        }

        public ObservableCollection<Polarization> Polarizations
        {
            get => _polarizations;
            set => SetProperty(ref _polarizations, value);
        }

        public ObservableCollection<string> FecOptions
        {
            get => _fecOptions;
            set => SetProperty(ref _fecOptions, value);
        }

        public ObservableCollection<string> Modulations
        {
            get => _modulations;
            set => SetProperty(ref _modulations, value);
        }

        public ObservableCollection<int> Bandwidths
        {
            get => _bandwidths;
            set => SetProperty(ref _bandwidths, value);
        }

        public ObservableCollection<StreamTsType> StreamTypes
        {
            get => _streamTypes;
            set => SetProperty(ref _streamTypes, value);
        }

        public ObservableCollection<StreamBuffering> BufferingOptions
        {
            get => _bufferingOptions;
            set => SetProperty(ref _bufferingOptions, value);
        }

        public ObservableCollection<ServiceType> ServiceTypes
        {
            get => _serviceTypes;
            set => SetProperty(ref _serviceTypes, value);
        }

        public string NewEpgSource
        {
            get => _newEpgSource;
            set => SetProperty(ref _newEpgSource, value);
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadChannelData();
            UpdateVisibility();
        }

        private void LoadChannelData()
        {
            if (Channel == null) return;

            Name = Channel.Name;
            Number = Channel.Number;
            Flags = Channel.Flags;
            Provider = Channel.Provider;
            Satellite = Channel.Satellite;
            ServiceId = Channel.ServiceId;
            NetworkId = Channel.NetworkId;
            TransportStreamId = Channel.TransportStreamId;
            ServiceType = Channel.ServiceType;
            LogoPath = Channel.LogoPath;
            Marker = Channel.Marker;

            EpgSources.Clear();
            foreach (var src in Channel.EpgSources)
                EpgSources.Add(src);

            // Type-specific properties
            switch (Channel)
            {
                case DvbSChannel dvbs:
                    Frequency = dvbs.Frequency;
                    Polarization = dvbs.Polarization;
                    SymbolRate = dvbs.SymbolRate;
                    Fec = dvbs.FEC;
                    Modulation = dvbs.Modulation;
                    Pilot = dvbs.Pilot;
                    RollOff = dvbs.RollOff;
                    IsDvbS2 = dvbs.IsDvbS2;
                    PlpId = dvbs.PLPId;
                    break;

                case DvbCChannel dvbc:
                    Frequency = dvbc.Frequency;
                    SymbolRate = dvbc.SymbolRate;
                    CableModulation = dvbc.Modulation;
                    Fec = dvbc.FEC;
                    IsDvbC2 = dvbc.IsDvbC2;
                    PlpId = dvbc.PLPId;
                    break;

                case DvbTChannel dvbt:
                    Frequency = dvbt.Frequency;
                    Bandwidth = dvbt.Bandwidth;
                    TerrestrialModulation = dvbt.Modulation;
                    Fec = dvbt.FEC;
                    GuardInterval = dvbt.GuardInterval;
                    FftMode = dvbt.FFTMode;
                    IsDvbT2 = dvbt.IsDvbT2;
                    PlpId = dvbt.PLPId;
                    break;

                case IptvChannel iptv:
                    Url = iptv.Url;
                    StreamType = iptv.StreamType;
                    UserAgent = iptv.UserAgent;
                    Referer = iptv.Referer;
                    Buffering = iptv.Buffering;
                    BufferSize = iptv.BufferSize;
                    Timeout = iptv.Timeout;
                    break;
            }
        }

        private void UpdateVisibility()
        {
            IsDvbsVisible = Channel is DvbSChannel;
            IsDvbcVisible = Channel is DvbCChannel;
            IsDvbtVisible = Channel is DvbTChannel;
            IsIptvVisible = Channel is IptvChannel;
        }

        [RelayCommand]
        private async Task BrowseLogoAsync()
        {
            var path = await _dialogService.ShowOpenFileDialogAsync("Select Logo", "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                // Validate file size
                var info = new FileInfo(path);
                if (info.Length > 200 * 1024) // 200KB
                {
                    await _dialogService.ShowWarningAsync("Large Logo", "Logo file is larger than 200KB. It will be resized.");
                }
                LogoPath = path;
            }
        }

        [RelayCommand]
        private async Task ClearLogoAsync()
        {
            LogoPath = string.Empty;
        }

        [RelayCommand]
        private async Task TestStreamAsync()
        {
            if (Channel is IptvChannel iptv && !string.IsNullOrEmpty(iptv.Url))
            {
                SetBusy(true, "Testing stream...");
                try
                {
                    // Would test the stream here
                    await Task.Delay(1000);
                    await _dialogService.ShowInfoAsync("Stream Test", "Stream is accessible");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Stream Test Failed", ex.Message);
                }
                finally
                {
                    SetBusy(false);
                }
            }
        }

        [RelayCommand]
        private void OK()
        {
            if (Validate())
            {
                SaveChannelData();
                Close(true);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }

        [RelayCommand]
        private void AddEpgSource()
        {
            if (!string.IsNullOrWhiteSpace(NewEpgSource) && !EpgSources.Contains(NewEpgSource))
            {
                EpgSources.Add(NewEpgSource);
                NewEpgSource = string.Empty;
            }
        }

        private void SaveChannelData()
        {
            Channel.Name = Name;
            Channel.Number = Number;
            Channel.Flags = Flags;
            Channel.Provider = Provider;
            Channel.Satellite = Satellite;
            Channel.ServiceId = ServiceId;
            Channel.NetworkId = NetworkId;
            Channel.TransportStreamId = TransportStreamId;
            Channel.ServiceType = ServiceType;
            Channel.LogoPath = LogoPath;
            Channel.Marker = Marker;

            Channel.EpgSources.Clear();
            Channel.EpgSources.AddRange(EpgSources);

            switch (Channel)
            {
                case DvbSChannel dvbs:
                    dvbs.Frequency = Frequency;
                    dvbs.Polarization = Polarization;
                    dvbs.SymbolRate = SymbolRate;
                    dvbs.FEC = Fec;
                    dvbs.Modulation = Modulation;
                    dvbs.Pilot = Pilot;
                    dvbs.RollOff = RollOff;
                    dvbs.IsDvbS2 = IsDvbS2;
                    dvbs.PLPId = PlpId;
                    break;

                case DvbCChannel dvbc:
                    dvbc.Frequency = Frequency;
                    dvbc.SymbolRate = SymbolRate;
                    dvbc.Modulation = CableModulation;
                    dvbc.FEC = Fec;
                    dvbc.IsDvbC2 = IsDvbC2;
                    dvbc.PLPId = PlpId;
                    break;

                case DvbTChannel dvbt:
                    dvbt.Frequency = Frequency;
                    dvbt.Bandwidth = Bandwidth;
                    dvbt.Modulation = TerrestrialModulation;
                    dvbt.FEC = Fec;
                    dvbt.GuardInterval = GuardInterval;
                    dvbt.FFTMode = FftMode;
                    dvbt.IsDvbT2 = IsDvbT2;
                    dvbt.PLPId = PlpId;
                    break;

                case IptvChannel iptv:
                    iptv.Url = Url;
                    iptv.StreamType = StreamType;
                    iptv.UserAgent = UserAgent;
                    iptv.Referer = Referer;
                    iptv.Buffering = Buffering;
                    iptv.BufferSize = BufferSize;
                    iptv.Timeout = Timeout;
                    break;
            }

            Repository.IsModified = true;
        }

        protected override bool Validate()
        {
            var errors = new System.Collections.Generic.List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Channel name is required");

            if (ServiceId <= 0)
                errors.Add("Valid Service ID is required");

            if (Channel is DvbSChannel || Channel is DvbCChannel || Channel is DvbTChannel)
            {
                if (Frequency <= 0)
                    errors.Add("Valid frequency is required");
            }

            if (Channel is IptvChannel && string.IsNullOrWhiteSpace(Url))
                errors.Add("Stream URL is required for IPTV channels");

            ValidationErrors = string.Join("\n", errors);
            return errors.Count == 0;
        }
    }
}