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
    /// Import dialog ViewModel
    /// </summary>
    public partial class ImportViewModel : ScreenViewModel
    {
        private readonly ILogger<ImportViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly IImportExportService _importExportService;

        public ImportViewModel(ILogger<ImportViewModel> logger, IDialogService dialogService, IImportExportService importExportService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _importExportService = importExportService;

            Title = "Import Data";

            // Initialize collections
            SatelliteOptions = new ObservableCollection<string>
            {
                "All (Transponders, Channels, Services, Logos, EPG)",
                "Transponders Only",
                "Channels Only",
                "Services Only",
                "Logos Only",
                "EPG Only"
            };
            SelectedSatelliteOption = SatelliteOptions[0];

            DvbOptions = new ObservableCollection<string>
            {
                "All (TV, Radio, Data, HD, UHD, Scrambled, FTA)",
                "TV Only",
                "Radio Only",
                "Data Only",
                "HD Only",
                "UHD Only",
                "FTA Only"
            };
            SelectedDvbOption = DvbOptions[0];

            LogoFormats = new ObservableCollection<string> { "png", "jpg", "bmp", "webp" };
            Bandwidths = new ObservableCollection<int> { 6, 7, 8 };
            Polarizations = new ObservableCollection<Polarization> { Polarization.Horizontal, Polarization.Vertical, Polarization.CircularLeft, Polarization.CircularRight };
            FecOptions = new ObservableCollection<string> { "Auto", "1/2", "2/3", "3/4", "5/6", "7/8" };
            Modulations = new ObservableCollection<string> { "QPSK", "8PSK", "16APSK", "32APSK", "QAM16", "QAM32", "QAM64", "QAM128", "QAM256" };
            StreamTypes = new ObservableCollection<StreamTsType> { StreamTsType.HLS, StreamTsType.M3U, StreamTsType.M3U8, StreamTsType.TS, StreamTsType.DASH, StreamTsType.RTMP, StreamTsType.RTSP, StreamTsType.HTTP, StreamTsType.UDP, StreamTsType.RTP };
            BufferingOptions = new ObservableCollection<StreamBuffering> { StreamBuffering.None, StreamBuffering.Low, StreamBuffering.Medium, StreamBuffering.High };
            ServiceTypes = new ObservableCollection<ServiceType> { ServiceType.TV, ServiceType.Radio, ServiceType.Data, ServiceType.Mosaic, ServiceType.AdvancedCodecHD, ServiceType.AdvancedCodecSD, ServiceType.AdvancedCodecRadio, ServiceType.HEVC_UHD, ServiceType.HEVC_HD };
            Directions = new ObservableCollection<SatelliteDirection> { SatelliteDirection.East, SatelliteDirection.West };
        }

        [ObservableProperty]
        private ChannelRepository _repository = new();

        [ObservableProperty]
        private string _sourcePath = string.Empty;

        [ObservableProperty]
        private ImportType _selectedImportType = ImportType.Satellites;

        [ObservableProperty]
        private ImportSatellitesOptions _satelliteOptions = ImportSatellitesOptions.All;

        [ObservableProperty]
        private ImportDvbOptions _dvbOptions = ImportDvbOptions.All;

        [ObservableProperty]
        private bool _createBackup = true;

        [ObservableProperty]
        private bool _createGroups = true;

        [ObservableProperty]
        private string _targetSatellite = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _availableSatellites = new();

        [ObservableProperty]
        private ImportResult? _lastResult;

        [ObservableProperty]
        private bool _isImporting;

        // Additional properties for XAML
        [ObservableProperty]
        private ObservableCollection<string> _satelliteOptionsList = new();

        [ObservableProperty]
        private string _selectedSatelliteOption = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _dvbOptionsList = new();

        [ObservableProperty]
        private string _selectedDvbOption = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _logoFormats = new();

        [ObservableProperty]
        private ObservableCollection<int> _bandwidths = new();

        [ObservableProperty]
        private ObservableCollection<Polarization> _polarizations = new();

        [ObservableProperty]
        private ObservableCollection<string> _fecOptions = new();

        [ObservableProperty]
        private ObservableCollection<string> _modulations = new();

        [ObservableProperty]
        private ObservableCollection<StreamTsType> _streamTypes = new();

        [ObservableProperty]
        private ObservableCollection<StreamBuffering> _bufferingOptions = new();

        [ObservableProperty]
        private ObservableCollection<ServiceType> _serviceTypes = new();

        [ObservableProperty]
        private ObservableCollection<SatelliteDirection> _directions = new();

        [ObservableProperty]
        private bool _downloadMissingLogos = true;

        public enum ImportType
        {
            Satellites,
            Transponders,
            Channels,
            Epg,
            Logos,
            M3U,
            Repository
        }

        public ObservableCollection<string> SatelliteOptions
        {
            get => _satelliteOptionsList;
            set => SetProperty(ref _satelliteOptionsList, value);
        }

        public string SelectedSatelliteOption
        {
            get => _selectedSatelliteOption;
            set => SetProperty(ref _selectedSatelliteOption, value);
        }

        public ObservableCollection<string> DvbOptions
        {
            get => _dvbOptionsList;
            set => SetProperty(ref _dvbOptionsList, value);
        }

        public string SelectedDvbOption
        {
            get => _selectedDvbOption;
            set => SetProperty(ref _selectedDvbOption, value);
        }

        public ObservableCollection<string> LogoFormats
        {
            get => _logoFormats;
            set => SetProperty(ref _logoFormats, value);
        }

        public ObservableCollection<int> Bandwidths
        {
            get => _bandwidths;
            set => SetProperty(ref _bandwidths, value);
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

        public ObservableCollection<SatelliteDirection> Directions
        {
            get => _directions;
            set => SetProperty(ref _directions, value);
        }

        public bool DownloadMissingLogos
        {
            get => _downloadMissingLogos;
            set => SetProperty(ref _downloadMissingLogos, value);
        }

        partial void OnSelectedImportTypeChanged(ImportType value)
        {
            UpdateUIForImportType(value);
        }

        private void UpdateUIForImportType(ImportType type)
        {
            // Show/hide relevant options based on import type
        }

        [RelayCommand]
        private async Task BrowseSourceAsync()
        {
            string filter = SelectedImportType switch
            {
                ImportType.Satellites => "XML Files (*.xml)|*.xml|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                ImportType.Transponders => "XML Files (*.xml)|*.xml|JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                ImportType.Channels => "XML Files (*.xml)|*.xml|JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                ImportType.Epg => "XMLTV Files (*.xml)|*.xml|MHW Files (*.mhw)|*.mhw|EPGDAT Files (*.dat)|*.dat|All Files (*.*)|*.*",
                ImportType.Logos => "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|ZIP Files (*.zip)|*.zip|All Files (*.*)|*.*",
                ImportType.M3U => "M3U Files (*.m3u;*.m3u8)|*.m3u;*.m3u8|All Files (*.*)|*.*",
                ImportType.Repository => "E-Channelizer Files (*.ech)|*.ech|ZIP Files (*.zip)|*.zip|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                _ => "All Files (*.*)|*.*"
            };

            var path = await _dialogService.ShowOpenFileDialogAsync("Select Import File", filter);
            if (!string.IsNullOrEmpty(path))
            {
                SourcePath = path;
            }
        }

        [RelayCommand]
        private async Task ImportAsync()
        {
            if (string.IsNullOrEmpty(SourcePath) || !File.Exists(SourcePath))
            {
                await _dialogService.ShowErrorAsync("Error", "Please select a valid source file");
                return;
            }

            IsImporting = true;
            SetBusy(true, "Importing...");

            try
            {
                var options = new ImportOptions
                {
                    SatelliteOptions = SatelliteOptions,
                    DvbOptions = DvbOptions,
                    CreateBackup = CreateBackup,
                    CreateGroups = CreateGroups,
                    TargetSatellite = TargetSatellite
                };

                ImportResult result;

                switch (SelectedImportType)
                {
                    case ImportType.Satellites:
                        result = await _importExportService.ImportSatellitesAsync(SourcePath, SatelliteOptions);
                        break;
                    case ImportType.Transponders:
                    case ImportType.Channels:
                        result = await _importExportService.ImportDvbAsync(SourcePath, DvbOptions);
                        break;
                    case ImportType.Epg:
                        result = await _importExportService.ImportEpgAsync(SourcePath, EpgSourceType.XMLTV);
                        break;
                    case ImportType.Logos:
                        result = await _importExportService.ImportLogosAsync(SourcePath);
                        break;
                    case ImportType.M3U:
                        result = await _importExportService.ImportM3UAsync(SourcePath);
                        break;
                    case ImportType.Repository:
                        result = await _importExportService.ImportRepositoryAsync(SourcePath);
                        break;
                    default:
                        result = new ImportResult { Success = false, Errors = { "Unknown import type" } };
                        break;
                }

                LastResult = result;

                if (result.Success)
                {
                    SetStatusMessage($"Import successful: {result.ChannelsImported} channels, {result.GroupsCreated} groups");
                    await _dialogService.ShowInfoAsync("Import Complete", GetResultMessage(result));
                    Close(true);
                }
                else
                {
                    SetStatusMessage("Import failed");
                    await _dialogService.ShowErrorAsync("Import Failed", string.Join("\n", result.Errors));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import failed");
                await _dialogService.ShowErrorAsync("Error", ex.Message);
            }
            finally
            {
                IsImporting = false;
                SetBusy(false);
            }
        }

        private string GetResultMessage(ImportResult result)
        {
            var msg = new System.Text.StringBuilder();
            msg.AppendLine($"Import completed successfully!");
            msg.AppendLine();

            if (result.ChannelsImported > 0)
                msg.AppendLine($"Channels imported: {result.ChannelsImported}");
            if (result.GroupsCreated > 0)
                msg.AppendLine($"Groups created: {result.GroupsCreated}");
            if (result.TranspondersImported > 0)
                msg.AppendLine($"Transponders imported: {result.TranspondersImported}");
            if (result.SatellitesImported > 0)
                msg.AppendLine($"Satellites imported: {result.SatellitesImported}");
            if (result.LogosImported > 0)
                msg.AppendLine($"Logos imported: {result.LogosImported}");
            if (result.EpgSourcesImported > 0)
                msg.AppendLine($"EPG sources imported: {result.EpgSourcesImported}");

            if (result.Warnings.Count > 0)
            {
                msg.AppendLine();
                msg.AppendLine("Warnings:");
                foreach (var warning in result.Warnings)
                    msg.AppendLine($"  - {warning}");
            }

            return msg.ToString();
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }
    }
}