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
    /// Edit Network Device ViewModel
    /// </summary>
    public partial class EditDeviceViewModel : ScreenViewModel
    {
        private readonly ILogger<EditDeviceViewModel> _logger;
        private readonly IDialogService _dialogService;

        public EditDeviceViewModel(ILogger<EditDeviceViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Title = "Edit Device";
        }

        [ObservableProperty]
        private NetworkDevice _device = new();

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private ObservableCollection<string> _deviceTypes = new() { "Enigma2", "Dreambox", "Neutrino", "VDR", "TVHeadend", "Custom" };

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

            if (string.IsNullOrWhiteSpace(Device.Name))
                errors.Add("Device name is required");

            if (string.IsNullOrWhiteSpace(Device.IpAddress))
                errors.Add("IP address is required");

            if (Device.Port <= 0 || Device.Port > 65535)
                errors.Add("Valid port is required");

            if (string.IsNullOrWhiteSpace(Device.Username))
                errors.Add("Username is required");

            ValidationErrors = string.Join("\n", errors);
            return errors.Count == 0;
        }
    }
}