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
    /// Assign Logos to Channels ViewModel
    /// </summary>
    public partial class AssignLogosViewModel : ScreenViewModel
    {
        private readonly ILogger<AssignLogosViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly ILogoService _logoService;

        public AssignLogosViewModel(ILogger<AssignLogosViewModel> logger, IDialogService dialogService, ILogoService logoService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _logoService = logoService;

            Title = "Assign Logos";
        }

        [ObservableProperty]
        private ChannelRepository _repository = new();

        [ObservableProperty]
        private string _logosFolder = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ChannelLogoInfo> _channels = new();

        [ObservableProperty]
        private ChannelLogoInfo _selectedChannel = null!;

        [ObservableProperty]
        private int _assignedCount;

        [ObservableProperty]
        private int _missingCount;

        [ObservableProperty]
        private bool _downloadMissing = true;

        public override void Initialize()
        {
            base.Initialize();
            LoadChannels();
        }

        private void LoadChannels()
        {
            if (Repository != null)
            {
                Channels.Clear();
                foreach (var ch in Repository.AllChannels.Where(c => c.IsVisible))
                {
                    var info = new ChannelLogoInfo
                    {
                        Channel = ch,
                        HasLogo = !string.IsNullOrEmpty(ch.LogoPath) && File.Exists(ch.LogoPath),
                        LogoPath = ch.LogoPath
                    };
                    Channels.Add(info);
                }
                UpdateCounts();
            }
        }

        private void UpdateCounts()
        {
            AssignedCount = Channels.Count(c => c.HasLogo);
            MissingCount = Channels.Count(c => !c.HasLogo);
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
        private async Task ScanLogosAsync()
        {
            if (string.IsNullOrEmpty(LogosFolder) || !Directory.Exists(LogosFolder))
            {
                await _dialogService.ShowWarningAsync("Invalid Folder", "Please select a valid logos folder");
                return;
            }

            SetBusy(true, "Scanning logos...");
            try
            {
                foreach (var info in Channels)
                {
                    if (!info.HasLogo)
                    {
                        var logoPath = await _logoService.FindLocalLogoAsync(info.Channel.Name, LogosFolder);
                        if (!string.IsNullOrEmpty(logoPath))
                        {
                            info.HasLogo = true;
                            info.LogoPath = logoPath;
                            info.Channel.LogoPath = logoPath;
                        }
                    }
                }
                UpdateCounts();
                Repository.IsModified = true;
                SetStatusMessage($"Scanned logos: {AssignedCount} assigned, {MissingCount} missing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logo scan failed");
                await _dialogService.ShowErrorAsync("Error", ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        [RelayCommand]
        private async Task DownloadMissingLogosAsync()
        {
            if (MissingCount == 0) return;

            SetBusy(true, "Downloading logos...");
            try
            {
                var downloaded = 0;
                foreach (var info in Channels.Where(c => !c.HasLogo))
                {
                    var logoPath = await _logoService.DownloadLogoAsync(info.Channel.Name, info.Channel.Provider, info.Channel.Satellite);
                    if (!string.IsNullOrEmpty(logoPath))
                    {
                        info.HasLogo = true;
                        info.LogoPath = logoPath;
                        info.Channel.LogoPath = logoPath;
                        downloaded++;
                    }
                }
                UpdateCounts();
                Repository.IsModified = true;
                SetStatusMessage($"Downloaded {downloaded} logos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logo download failed");
                await _dialogService.ShowErrorAsync("Error", ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        [RelayCommand]
        private async Task AssignSelectedLogoAsync()
        {
            if (SelectedChannel == null) return;

            var path = await _dialogService.ShowOpenFileDialogAsync("Select Logo", "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                SelectedChannel.LogoPath = path;
                SelectedChannel.HasLogo = true;
                SelectedChannel.Channel.LogoPath = path;
                UpdateCounts();
                Repository.IsModified = true;
            }
        }

        [RelayCommand]
        private void ClearSelectedLogo()
        {
            if (SelectedChannel != null)
            {
                SelectedChannel.LogoPath = string.Empty;
                SelectedChannel.HasLogo = false;
                SelectedChannel.Channel.LogoPath = string.Empty;
                UpdateCounts();
                Repository.IsModified = true;
            }
        }

        [RelayCommand]
        private async Task ExportLogosAsync()
        {
            var folder = await _dialogService.ShowOpenFileDialogAsync("Select Export Folder", "Folders|*.*");
            if (!string.IsNullOrEmpty(folder))
            {
                var exportPath = Path.GetDirectoryName(folder) ?? folder;
                await _logoService.ExportLogosAsync(Repository.Logos, exportPath);
                await _dialogService.ShowInfoAsync("Export", $"Logos exported to {exportPath}");
            }
        }

        [RelayCommand]
        private async Task CleanupUnusedAsync()
        {
            if (string.IsNullOrEmpty(LogosFolder)) return;

            var confirm = await _dialogService.ShowConfirmAsync("Cleanup", "Delete unused logo files?");
            if (confirm)
            {
                await _logoService.CleanupUnusedLogosAsync(LogosFolder, Repository);
                await _dialogService.ShowInfoAsync("Cleanup", "Unused logos cleaned up");
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

        public class ChannelLogoInfo
        {
            public IChannel Channel { get; set; } = null!;
            public bool HasLogo { get; set; }
            public string LogoPath { get; set; } = string.Empty;
        }
    }
}