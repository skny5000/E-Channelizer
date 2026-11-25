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
    /// Batch Edit EPG ViewModel
    /// </summary>
    public partial class BatchEditEpgViewModel : ScreenViewModel
    {
        private readonly ILogger<BatchEditEpgViewModel> _logger;
        private readonly IDialogService _dialogService;

        public BatchEditEpgViewModel(ILogger<BatchEditEpgViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Title = "Batch Edit EPG";
        }

        [ObservableProperty]
        private ChannelRepository _repository = new();

        [ObservableProperty]
        private ObservableCollection<EpgSource> _epgSources = new();

        [ObservableProperty]
        private EpgSource _selectedEpgSource = null!;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _mappedCount;

        [ObservableProperty]
        private int _unmappedXmlCount;

        [ObservableProperty]
        private int _unmappedChannelCount;

        public override void Initialize()
        {
            base.Initialize();
            LoadEpgSources();
        }

        private void LoadEpgSources()
        {
            if (Repository != null)
            {
                EpgSources.Clear();
                foreach (var src in Repository.EpgSources)
                {
                    EpgSources.Add(src);
                }
            }
        }

        [RelayCommand]
        private async Task AutoMapAsync()
        {
            // Auto-map EPG to channels
            SetBusy(true, "Auto-mapping EPG...");
            try
            {
                await Task.Delay(500);
                MappedCount = 100; // Placeholder
                UnmappedCount = 5; // Placeholder
                SetStatusMessage($"Auto-mapped {MappedCount} channels, {UnmappedCount} unmapped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-map failed");
                await _dialogService.ShowErrorAsync("Error", ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        [RelayCommand]
        private async Task ClearMappingsAsync()
        {
            var confirm = await _dialogService.ShowConfirmAsync("Clear Mappings", "Clear all EPG mappings?");
            if (confirm)
            {
                // Clear mappings
                MappedCount = 0;
                UnmappedCount = Repository.AllChannels.Count;
                SetStatusMessage("EPG mappings cleared");
            }
        }

        [RelayCommand]
        private async Task ImportEpgAsync()
        {
            var path = await _dialogService.ShowOpenFileDialogAsync("Import EPG", "XMLTV Files (*.xml)|*.xml|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                // Import EPG
                await _dialogService.ShowInfoAsync("Import", "EPG import not yet implemented");
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