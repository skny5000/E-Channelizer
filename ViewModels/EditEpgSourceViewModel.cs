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
    /// Edit EPG Source ViewModel
    /// </summary>
    public partial class EditEpgSourceViewModel : ScreenViewModel
    {
        private readonly ILogger<EditEpgSourceViewModel> _logger;
        private readonly IDialogService _dialogService;

        public EditEpgSourceViewModel(ILogger<EditEpgSourceViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Title = "Edit EPG Source";
        }

        [ObservableProperty]
        private EpgSource _epgSource = new();

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private ObservableCollection<string> _sourceTypes = new() { "XMLTV", "DVB EIT", "MHW", "EPGDAT", "Custom" };

        public override void Initialize()
        {
            base.Initialize();
        }

        [RelayCommand]
        private async Task BrowseFileAsync()
        {
            var path = await _dialogService.ShowOpenFileDialogAsync("Select EPG File", "XML Files (*.xml)|*.xml|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                EpgSource.FilePath = path;
            }
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

            if (string.IsNullOrWhiteSpace(EpgSource.Name))
                errors.Add("Source name is required");

            if (EpgSource.Type == EpgSourceType.XMLTV && string.IsNullOrEmpty(EpgSource.FilePath))
                errors.Add("File path is required for XMLTV sources");

            ValidationErrors = string.Join("\n", errors);
            return errors.Count == 0;
        }
    }
}