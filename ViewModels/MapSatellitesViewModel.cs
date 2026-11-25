using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EChannelizer.Models;
using EChannelizer.Services;
using Microsoft.Extensions.Logging;

namespace EChannelizer.ViewModels
{
    /// <summary>
    /// Map Satellites ViewModel
    /// </summary>
    public partial class MapSatellitesViewModel : ScreenViewModel
    {
        private readonly ILogger<MapSatellitesViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly IImportExportService _importExportService;

        public MapSatellitesViewModel(ILogger<MapSatellitesViewModel> logger, IDialogService dialogService, IImportExportService importExportService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _importExportService = importExportService;

            Title = "Map Satellites";
        }

        [ObservableProperty]
        private ChannelRepository _repository = new();

        [ObservableProperty]
        private ObservableCollection<WebSatellite> _webSatellites = new();

        [ObservableProperty]
        private WebSatellite _selectedWebSatellite = null!;

        [ObservableProperty]
        private ObservableCollection<Satellite> _localSatellites = new();

        [ObservableProperty]
        private Satellite _selectedLocalSatellite = null!;

        [ObservableProperty]
        private string _downloadUrl = "https://raw.githubusercontent.com/satellite-data/satellites/main/satellites.json";

        public override void Initialize()
        {
            base.Initialize();
            LoadLocalSatellites();
        }

        private void LoadLocalSatellites()
        {
            if (Repository != null)
            {
                LocalSatellites.Clear();
                foreach (var sat in Repository.Satellites)
                {
                    LocalSatellites.Add(sat);
                }
            }
        }

        [RelayCommand]
        private async Task DownloadSatellitesAsync()
        {
            if (string.IsNullOrEmpty(DownloadUrl)) return;

            SetBusy(true, "Downloading satellite list...");
            try
            {
                // In a real implementation, this would download and parse the satellite data
                // For now, just add some sample data
                WebSatellites.Clear();

                // Sample satellites
                var samples = new[]
                {
                    new WebSatellite { Name = "Astra 19.2°E", OrbitalPosition = 19.2, Direction = SatelliteDirection.East, Region = "Europe", Country = "Germany" },
                    new WebSatellite { Name = "Hotbird 13°E", OrbitalPosition = 13.0, Direction = SatelliteDirection.East, Region = "Europe", Country = "Europe" },
                    new WebSatellite { Name = "Eutelsat 9°E", OrbitalPosition = 9.0, Direction = SatelliteDirection.East, Region = "Europe", Country = "Europe" },
                    new WebSatellite { Name = "Turksat 42°E", OrbitalPosition = 42.0, Direction = SatelliteDirection.East, Region = "Europe/Asia", Country = "Turkey" },
                    new WebSatellite { Name = "Arabsat 26°E", OrbitalPosition = 26.0, Direction = SatelliteDirection.East, Region = "Middle East", Country = "Saudi Arabia" },
                    new WebSatellite { Name = "Nilesat 7°W", OrbitalPosition = 7.0, Direction = SatelliteDirection.West, Region = "Middle East/Africa", Country = "Egypt" },
                };

                foreach (var sat in samples)
                {
                    WebSatellites.Add(sat);
                }

                SetStatusMessage($"Loaded {WebSatellites.Count} satellites");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download satellites");
                await _dialogService.ShowErrorAsync("Error", ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        [RelayCommand]
        private async Task ImportSelectedAsync()
        {
            if (SelectedWebSatellite == null) return;

            var satellite = SelectedWebSatellite.ToSatellite();
            Repository.Satellites.Add(satellite);
            LocalSatellites.Add(satellite);
            Repository.IsModified = true;

            SetStatusMessage($"Imported {satellite.Name}");
        }

        [RelayCommand]
        private async Task ImportAllAsync()
        {
            foreach (var webSat in WebSatellites.Where(w => w.IsSelected))
            {
                var satellite = webSat.ToSatellite();
                Repository.Satellites.Add(satellite);
                LocalSatellites.Add(satellite);
            }
            Repository.IsModified = true;
            SetStatusMessage($"Imported {WebSatellites.Count(w => w.IsSelected)} satellites");
        }

        [RelayCommand]
        private async Task MapTranspondersAsync()
        {
            if (SelectedLocalSatellite == null) return;

            // Map transponders from web satellite to local satellite
            await _dialogService.ShowInfoAsync("Map Transponders", "Transponder mapping not yet implemented");
        }

        [RelayCommand]
        private void OK()
        {
            Close(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }
    }
}