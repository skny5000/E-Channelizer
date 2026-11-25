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
    /// Edit Satellite ViewModel
    /// </summary>
    public partial class EditSatelliteViewModel : ScreenViewModel
    {
        private readonly ILogger<EditSatelliteViewModel> _logger;
        private readonly IDialogService _dialogService;

        public EditSatelliteViewModel(ILogger<EditSatelliteViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Title = "Edit Satellite";

            Regions = new ObservableCollection<string> { "Europe", "Asia", "Africa", "Americas", "Middle East", "Oceania" };
            Countries = new ObservableCollection<string> { "Germany", "France", "UK", "Spain", "Italy", "Netherlands", "Poland", "Turkey", "Russia", "Saudi Arabia", "Egypt", "USA", "Brazil", "Japan", "China" };
            Languages = new ObservableCollection<string> { "en", "de", "fr", "es", "it", "nl", "pl", "tr", "ru", "ar", "zh", "ja", "ko" };
            Directions = new ObservableCollection<SatelliteDirection> { SatelliteDirection.East, SatelliteDirection.West };
        }

        [ObservableProperty]
        private Satellite _satellite = new();

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private ObservableCollection<string> _regions = new();

        [ObservableProperty]
        private ObservableCollection<string> _countries = new();

        [ObservableProperty]
        private ObservableCollection<string> _languages = new();

        [ObservableProperty]
        private ObservableCollection<SatelliteDirection> _directions = new();

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

        public ObservableCollection<SatelliteDirection> Directions
        {
            get => _directions;
            set => SetProperty(ref _directions, value);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        [RelayCommand]
        private void OK()
        {
            if (Validate())
            {
                Close(true);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }

        private bool Validate()
        {
            var errors = new System.Collections.Generic.List<string>();

            if (string.IsNullOrWhiteSpace(Satellite.Name))
                errors.Add("Satellite name is required");

            if (Satellite.OrbitalPosition < -180 || Satellite.OrbitalPosition > 180)
                errors.Add("Orbital position must be between -180 and 180");

            ValidationErrors = string.Join("\n", errors);
            return errors.Count == 0;
        }
    }
}