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
    /// Network Devices management ViewModel
    /// </summary>
    public partial class NetworkDevicesViewModel : ScreenViewModel
    {
        private readonly ILogger<NetworkDevicesViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly INetworkDeviceService _deviceService;

        public NetworkDevicesViewModel(ILogger<NetworkDevicesViewModel> logger, IDialogService dialogService, INetworkDeviceService deviceService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _deviceService = deviceService;

            Title = "Network Devices";
        }

        [ObservableProperty]
        private ObservableCollection<NetworkDevice> _devices = new();

        [ObservableProperty]
        private NetworkDevice _selectedDevice = null!;

        [ObservableProperty]
        private bool _isTestingConnection;

        public override void Initialize()
        {
            base.Initialize();
        }

        [RelayCommand]
        private async Task AddDeviceAsync()
        {
            var vm = new EditDeviceViewModel(_logger, _dialogService);
            vm.Device = new NetworkDevice { Name = "New Device" };
            vm.IsNew = true;

            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                Devices.Add(vm.Device);
            }
        }

        [RelayCommand]
        private async Task EditDeviceAsync()
        {
            if (SelectedDevice == null) return;

            var vm = new EditDeviceViewModel(_logger, _dialogService);
            vm.Device = SelectedDevice.Clone() as NetworkDevice ?? new NetworkDevice();
            vm.IsNew = false;

            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                // Update the device in the collection
                var index = Devices.IndexOf(SelectedDevice);
                if (index >= 0)
                {
                    Devices[index] = vm.Device;
                }
            }
        }

        [RelayCommand]
        private async Task DeleteDeviceAsync()
        {
            if (SelectedDevice == null) return;

            var confirm = await _dialogService.ShowConfirmAsync("Delete Device", $"Delete device '{SelectedDevice.Name}'?");
            if (confirm)
            {
                Devices.Remove(SelectedDevice);
            }
        }

        [RelayCommand]
        private async Task TestConnectionAsync()
        {
            if (SelectedDevice == null) return;

            IsTestingConnection = true;
            SetBusy(true, "Testing connection...");

            try
            {
                var success = await _deviceService.TestConnectionAsync(SelectedDevice);
                SelectedDevice.IsConnected = success;
                SelectedDevice.LastConnected = success ? DateTime.Now : DateTime.MinValue;

                if (success)
                {
                    await _dialogService.ShowInfoAsync("Connection Test", $"Successfully connected to {SelectedDevice.Name}");
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Connection Test", $"Failed to connect to {SelectedDevice.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed for {Device}", SelectedDevice.Name);
                await _dialogService.ShowErrorAsync("Connection Test", ex.Message);
            }
            finally
            {
                IsTestingConnection = false;
                SetBusy(false);
            }
        }

        [RelayCommand]
        private async Task GetDeviceInfoAsync()
        {
            if (SelectedDevice == null) return;

            SetBusy(true, "Getting device info...");

            try
            {
                var info = await _deviceService.GetDeviceInfoAsync(SelectedDevice);
                SelectedDevice.Model = info.Model;
                SelectedDevice.FirmwareVersion = info.FirmwareVersion;

                var message = new System.Text.StringBuilder();
                message.AppendLine($"Model: {info.Model}");
                message.AppendLine($"Firmware: {info.FirmwareVersion}");
                message.AppendLine($"Kernel: {info.KernelVersion}");
                message.AppendLine($"CPU: {info.CpuInfo}");
                message.AppendLine($"Memory: {info.MemoryInfo}");
                message.AppendLine($"Disk: {info.DiskInfo}");

                await _dialogService.ShowInfoAsync("Device Info", message.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get device info for {Device}", SelectedDevice.Name);
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