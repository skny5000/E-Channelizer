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
    /// Satellites management ViewModel
    /// </summary>
    public partial class SatellitesViewModel : ScreenViewModel
    {
        private readonly ILogger<SatellitesViewModel> _logger;
        private readonly IDialogService _dialogService;

        public SatellitesViewModel(ILogger<SatellitesViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Title = "Manage Satellites";

            // Initialize collections
            Regions = new ObservableCollection<string> { "Europe", "Asia", "Africa", "Americas", "Middle East", "Oceania" };
            Countries = new ObservableCollection<string> { "Germany", "France", "UK", "Spain", "Italy", "Netherlands", "Poland", "Turkey", "Russia", "Saudi Arabia", "Egypt", "USA", "Brazil", "Japan", "China" };
            Languages = new ObservableCollection<string> { "en", "de", "fr", "es", "it", "nl", "pl", "tr", "ru", "ar", "zh", "ja", "ko" };
        }

        [ObservableProperty]
        private ObservableCollection<Satellite> _satellites = new();

        [ObservableProperty]
        private Satellite _selectedSatellite = null!;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showEnabledOnly = true;

        // Additional properties for XAML
        [ObservableProperty]
        private ObservableCollection<string> _regions = new();

        [ObservableProperty]
        private ObservableCollection<string> _countries = new();

        [ObservableProperty]
        private ObservableCollection<string> _languages = new();

        public ObservableCollection<string> Regions
        {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        public ObservableCollection<string> Countries
        {
            get => _countries;
            set => SetProperty(ref _countries, value);
        }

        public ObservableCollection<string> Languages
        {
            get => _languages;
            set => SetProperty(ref _languages, value);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        partial void OnSearchTextChanged(string value)
        {
            // Filter satellites
        }

        partial void OnShowEnabledOnlyChanged(bool value)
        {
            // Filter satellites
        }

        [RelayCommand]
        private async Task AddSatelliteAsync()
        {
            var vm = new EditSatelliteViewModel(_logger, _dialogService);
            vm.Satellite = new Satellite { Name = "New Satellite", IsEnabled = true };
            vm.IsNew = true;

            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                Satellites.Add(vm.Satellite);
            }
        }

        [RelayCommand]
        private async Task EditSatelliteAsync()
        {
            if (SelectedSatellite == null) return;

            var vm = new EditSatelliteViewModel(_logger, _dialogService);
            vm.Satellite = SelectedSatellite.Clone() as Satellite ?? new Satellite();
            vm.IsNew = false;

            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                var index = Satellites.IndexOf(SelectedSatellite);
                if (index >= 0)
                {
                    Satellites[index] = vm.Satellite;
                }
            }
        }

        [RelayCommand]
        private async Task DeleteSatelliteAsync()
        {
            if (SelectedSatellite == null) return;

            var confirm = await _dialogService.ShowConfirmAsync("Delete Satellite", $"Delete satellite '{SelectedSatellite.Name}'?");
            if (confirm)
            {
                Satellites.Remove(SelectedSatellite);
            }
        }

        [RelayCommand]
        private async Task ImportSatellitesAsync()
        {
            var path = await _dialogService.ShowOpenFileDialogAsync("Import Satellites", "XML Files (*.xml)|*.xml|JSON Files (*.json)|*.json|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                // Import logic would go here
                await _dialogService.ShowInfoAsync("Import", "Satellite import not yet implemented");
            }
        }

        [RelayCommand]
        private async Task ExportSatellitesAsync()
        {
            var path = await _dialogService.ShowSaveFileDialogAsync("Export Satellites", "JSON Files (*.json)|*.json|XML Files (*.xml)|*.xml");
            if (!string.IsNullOrEmpty(path))
            {
                // Export logic would go here
                await _dialogService.ShowInfoAsync("Export", "Satellite export not yet implemented");
            }
        }

        [RelayCommand]
        private async Task DownloadSatellitesAsync()
        {
            SetBusy(true, "Downloading satellite list...");
            try
            {
                // Would download from online source
                await Task.Delay(1000);
                await _dialogService.ShowInfoAsync("Download", "Satellite list download not yet implemented");
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