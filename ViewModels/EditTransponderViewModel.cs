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
    /// Edit Transponder ViewModel
    /// </summary>
    public partial class EditTransponderViewModel : ScreenViewModel
    {
        private readonly ILogger<EditTransponderViewModel> _logger;
        private readonly IDialogService _dialogService;

        public EditTransponderViewModel(ILogger<EditTransponderViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Title = "Edit Transponder";
        }

        [ObservableProperty]
        private TransponderBase _transponder = null!;

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private ObservableCollection<string> _standards = new() { "DVB-S", "DVB-S2", "DVB-C", "DVB-C2", "DVB-T", "DVB-T2", "ATSC" };

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

            if (string.IsNullOrWhiteSpace(Transponder.Name))
                errors.Add("Transponder name is required");

            if (Transponder.Frequency <= 0)
                errors.Add("Valid frequency is required");

            ValidationErrors = string.Join("\n", errors);
            return errors.Count == 0;
        }
    }
}