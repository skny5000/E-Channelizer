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
    /// Stream Player ViewModel
    /// </summary>
    public partial class StreamPlayerViewModel : ScreenViewModel
    {
        private readonly ILogger<StreamPlayerViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly IStreamService _streamService;

        public StreamPlayerViewModel(ILogger<StreamPlayerViewModel> logger, IDialogService dialogService, IStreamService streamService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _streamService = streamService;

            Title = "Stream Player";
        }

        [ObservableProperty]
        private IChannel _channel = null!;

        [ObservableProperty]
        private PlayerStatus _status = PlayerStatus.Stopped;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private double _volume = 1.0;

        [ObservableProperty]
        private bool _isMuted;

        [ObservableProperty]
        private string _streamUrl = string.Empty;

        [ObservableProperty]
        private string _streamInfo = string.Empty;

        public override void Initialize()
        {
            base.Initialize();

            _streamService.StreamStatusChanged += StreamService_StreamStatusChanged;

            if (Channel != null)
            {
                PlayAsync();
            }
        }

        private void StreamService_StreamStatusChanged(object? sender, StreamEventArgs e)
        {
            Status = e.Status;
            StatusMessage = e.Message;
        }

        [RelayCommand]
        private async Task PlayAsync()
        {
            if (Channel == null) return;

            try
            {
                await _streamService.PlayAsync(Channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to play channel");
                await _dialogService.ShowErrorAsync("Playback Error", ex.Message);
            }
        }

        [RelayCommand]
        private async Task StopAsync()
        {
            await _streamService.StopAsync();
        }

        [RelayCommand]
        private async Task PauseAsync()
        {
            await _streamService.PauseAsync();
        }

        [RelayCommand]
        private async Task ToggleMuteAsync()
        {
            IsMuted = !IsMuted;
            // Implement mute logic
        }

        [RelayCommand]
        private void ClosePlayer()
        {
            StopAsync().Wait();
            Close(true);
        }

        public override void Dispose()
        {
            _streamService.StreamStatusChanged -= StreamService_StreamStatusChanged;
            StopAsync().Wait();
            base.Dispose();
        }
    }
}