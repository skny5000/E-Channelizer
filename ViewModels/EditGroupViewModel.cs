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
    /// Edit Channel Group ViewModel
    /// </summary>
    public partial class EditGroupViewModel : ScreenViewModel
    {
        private readonly ILogger<EditGroupViewModel> _logger;
        private readonly IDialogService _dialogService;

        public EditGroupViewModel(ILogger<EditGroupViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Title = "Edit Group";
        }

        [ObservableProperty]
        private ChannelGroup _group = new();

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private ObservableCollection<string> _sources = new() { "User", "Provider", "Satellite" };

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

            if (string.IsNullOrWhiteSpace(Group.Name))
                errors.Add("Group name is required");

            ValidationErrors = string.Join("\n", errors);
            return errors.Count == 0;
        }
    }
}