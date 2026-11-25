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
    /// Profiles management ViewModel
    /// </summary>
    public partial class ProfilesViewModel : ScreenViewModel
    {
        private readonly ILogger<ProfilesViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly IProfileService _profileService;

        public ProfilesViewModel(ILogger<ProfilesViewModel> logger, IDialogService dialogService, IProfileService profileService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _profileService = profileService;

            Title = "Manage Profiles";
        }

        [ObservableProperty]
        private ObservableCollection<Profile> _profiles = new();

        [ObservableProperty]
        private Profile _selectedProfile = null!;

        [ObservableProperty]
        private string _newProfileName = string.Empty;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await LoadProfilesAsync();
        }

        private async Task LoadProfilesAsync()
        {
            var loaded = await _profileService.GetProfilesAsync();
            Profiles.Clear();
            foreach (var profile in loaded)
            {
                Profiles.Add(profile);
            }

            // Select active profile
            var activeProfile = await _profileService.LoadActiveProfileAsync();
            SelectedProfile = Profiles.FirstOrDefault(p => p.UniqueKey == activeProfile.UniqueKey);
        }

        [RelayCommand]
        private async Task CreateProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(NewProfileName))
            {
                await _dialogService.ShowWarningAsync("Invalid Name", "Please enter a profile name");
                return;
            }

            var profile = await _profileService.CreateProfileAsync(NewProfileName);
            Profiles.Add(profile);
            SelectedProfile = profile;
            NewProfileName = string.Empty;
        }

        [RelayCommand]
        private async Task CloneProfileAsync()
        {
            if (SelectedProfile == null) return;

            var name = await _dialogService.ShowInputDialogAsync("Clone Profile", "Enter name for cloned profile:", $"{SelectedProfile.Name} (Copy)");
            if (!string.IsNullOrWhiteSpace(name))
            {
                var profile = await _profileService.CloneProfileAsync(SelectedProfile.UniqueKey, name);
                Profiles.Add(profile);
                SelectedProfile = profile;
            }
        }

        [RelayCommand]
        private async Task DeleteProfileAsync()
        {
            if (SelectedProfile == null) return;

            if (Profiles.Count <= 1)
            {
                await _dialogService.ShowWarningAsync("Cannot Delete", "At least one profile must remain");
                return;
            }

            var confirm = await _dialogService.ShowConfirmAsync("Delete Profile", $"Delete profile '{SelectedProfile.Name}'?");
            if (confirm)
            {
                await _profileService.DeleteProfileAsync(SelectedProfile.UniqueKey);
                Profiles.Remove(SelectedProfile);
                SelectedProfile = Profiles.FirstOrDefault();
            }
        }

        [RelayCommand]
        private async Task SetActiveProfileAsync()
        {
            if (SelectedProfile == null) return;

            await _profileService.SetActiveProfileAsync(SelectedProfile.UniqueKey);
            await _dialogService.ShowInfoAsync("Profile Activated", $"Profile '{SelectedProfile.Name}' is now active. Restart the application to apply changes.");
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