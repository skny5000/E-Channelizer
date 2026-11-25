using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EChannelizer.Models;
using EChannelizer.Services;
using Microsoft.Extensions.Logging;

namespace EChannelizer.ViewModels
{
    /// <summary>
    /// Main application ViewModel
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        private readonly ILogger<MainViewModel> _logger;
        private readonly IRepositoryService _repositoryService;
        private readonly IProfileService _profileService;
        private readonly IDialogService _dialogService;
        private readonly IStreamService _streamService;
        private readonly IImportExportService _importExportService;

        [ObservableProperty]
        private ChannelRepository _repository = new();

        [ObservableProperty]
        private Profile _activeProfile = new();

        [ObservableProperty]
        private ObservableCollection<ChannelGroup> _groups = new();

        [ObservableProperty]
        private ObservableCollection<IChannel> _channels = new();

        [ObservableProperty]
        private ObservableCollection<TransponderBase> _transponders = new();

        [ObservableProperty]
        private ObservableCollection<Satellite> _satellites = new();

        [ObservableProperty]
        private ObservableCollection<NetworkDevice> _devices = new();

        [ObservableProperty]
        private ObservableCollection<EpgSource> _epgSources = new();

        [ObservableProperty]
        private ChannelGroup _selectedGroup = null!;

        [ObservableProperty]
        private IChannel _selectedChannel = null!;

        [ObservableProperty]
        private TransponderBase _selectedTransponder = null!;

        [ObservableProperty]
        private Satellite _selectedSatellite = null!;

        [ObservableProperty]
        private NetworkDevice _selectedDevice = null!;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ChannelSort _currentSort = ChannelSort.Number;

        [ObservableProperty]
        private ServicesSort _serviceSort = ServicesSort.Name;

        [ObservableProperty]
        private bool _showFavoritesOnly = false;

        [ObservableProperty]
        private bool _showScrambledOnly = false;

        [ObservableProperty]
        private bool _showHDOnly = false;

        [ObservableProperty]
        private int _totalChannels = 0;

        [ObservableProperty]
        private int _visibleChannels = 0;

        [ObservableProperty]
        private int _favoriteChannels = 0;

        [ObservableProperty]
        private int _scrambledChannels = 0;

        // Layout visibility
        [ObservableProperty]
        private bool _showGroupsPanel = true;

        [ObservableProperty]
        private bool _showTranspondersPanel = true;

        [ObservableProperty]
        private bool _showServicesPanel = true;

        [ObservableProperty]
        private bool _showFavoritesPanel = true;

        [ObservableProperty]
        private bool _showLogPanel = false;

        [ObservableProperty]
        private bool _showDetailsPanel = true;

        // Sort options
        [ObservableProperty]
        private ObservableCollection<ChannelSort> _channelSortOptions = new();

        [ObservableProperty]
        private ObservableCollection<ServicesSort> _serviceSortOptions = new();

        public MainViewModel(
            ILogger<MainViewModel> logger,
            IRepositoryService repositoryService,
            IProfileService profileService,
            IDialogService dialogService,
            IStreamService streamService,
            IImportExportService importExportService)
        {
            _logger = logger;
            _repositoryService = repositoryService;
            _profileService = profileService;
            _dialogService = dialogService;
            _streamService = streamService;
            _importExportService = importExportService;

            Title = "E-Channelizer";

            // Initialize sort options
            ChannelSortOptions = new ObservableCollection<ChannelSort> { ChannelSort.Number, ChannelSort.Name, ChannelSort.Frequency, ChannelSort.ServiceId, ChannelSort.Type, ChannelSort.Provider, ChannelSort.Satellite, ChannelSort.Favorites };
            ServiceSortOptions = new ObservableCollection<ServicesSort> { ServicesSort.Name, ServicesSort.ServiceId, ServicesSort.Type, ServicesSort.Provider, ServicesSort.Frequency, ServicesSort.Number };
        }

        public ObservableCollection<ChannelSort> ChannelSortOptions
        {
            get => _channelSortOptions;
            set => SetProperty(ref _channelSortOptions, value);
        }

        public ObservableCollection<ServicesSort> ServiceSortOptions
        {
            get => _serviceSortOptions;
            set => SetProperty(ref _serviceSortOptions, value);
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            SetBusy(true, "Loading profile...");
            try
            {
                ActiveProfile = await _profileService.LoadActiveProfileAsync();
                ApplyProfileSettings();

                if (!string.IsNullOrEmpty(ActiveProfile.RepositoryPath) && File.Exists(ActiveProfile.RepositoryPath))
                {
                    await LoadRepositoryAsync(ActiveProfile.RepositoryPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize");
                await _dialogService.ShowErrorAsync("Initialization Error", ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ApplyProfileSettings()
        {
            CurrentSort = ActiveProfile.Settings.ChannelSortOrder;
            ServiceSort = ActiveProfile.Settings.ServiceSortOrder;
        }

        [RelayCommand]
        private async Task NewRepositoryAsync()
        {
            var path = await _dialogService.ShowSaveFileDialogAsync("Create New Repository", "E-Channelizer Files (*.ech)|*.ech|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                await CreateNewRepositoryAsync(path);
            }
        }

        [RelayCommand]
        private async Task OpenRepositoryAsync()
        {
            var path = await _dialogService.ShowOpenFileDialogAsync("Open Repository", "E-Channelizer Files (*.ech)|*.ech|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                await LoadRepositoryAsync(path);
            }
        }

        [RelayCommand]
        private async Task SaveRepositoryAsync()
        {
            if (string.IsNullOrEmpty(Repository.FilePath))
            {
                await SaveRepositoryAsAsync();
            }
            else
            {
                await SaveRepositoryInternalAsync(Repository.FilePath);
            }
        }

        [RelayCommand]
        private async Task SaveRepositoryAsAsync()
        {
            var path = await _dialogService.ShowSaveFileDialogAsync("Save Repository As", "E-Channelizer Files (*.ech)|*.ech|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                await SaveRepositoryInternalAsync(path);
            }
        }

        private async Task CreateNewRepositoryAsync(string path)
        {
            SetBusy(true, "Creating new repository...");
            try
            {
                Repository = new ChannelRepository { FilePath = path, Name = Path.GetFileNameWithoutExtension(path) };
                await _repositoryService.SaveAsync(Repository, path);

                ActiveProfile.RepositoryPath = path;
                await _profileService.SaveProfileAsync(ActiveProfile);

                UpdateCollections();
                SetStatusMessage("New repository created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create repository");
                await _dialogService.ShowErrorAsync("Error", ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task LoadRepositoryAsync(string path)
        {
            SetBusy(true, "Loading repository...");
            try
            {
                Repository = await _repositoryService.LoadAsync(path);
                Repository.FilePath = path;

                ActiveProfile.RepositoryPath = path;
                if (!ActiveProfile.RecentFiles.Contains(path))
                {
                    ActiveProfile.RecentFiles.Insert(0, path);
                    if (ActiveProfile.RecentFiles.Count > 10)
                        ActiveProfile.RecentFiles.RemoveAt(10);
                }
                await _profileService.SaveProfileAsync(ActiveProfile);

                UpdateCollections();
                UpdateStatistics();
                SetStatusMessage($"Loaded: {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load repository");
                await _dialogService.ShowErrorAsync("Error", ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SaveRepositoryInternalAsync(string path)
        {
            SetBusy(true, "Saving repository...");
            try
            {
                await _repositoryService.SaveAsync(Repository, path);
                Repository.FilePath = path;
                Repository.IsModified = false;
                Repository.LastSaved = DateTime.Now;

                ActiveProfile.RepositoryPath = path;
                await _profileService.SaveProfileAsync(ActiveProfile);

                SetStatusMessage("Repository saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save repository");
                await _dialogService.ShowErrorAsync("Error", ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        [RelayCommand]
        private async Task ImportSatellitesAsync()
        {
            var vm = new ImportViewModel(_logger, _dialogService, _importExportService);
            vm.Repository = Repository;
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                UpdateCollections();
                UpdateStatistics();
            }
        }

        [RelayCommand]
        private async Task ExportRepositoryAsync()
        {
            var path = await _dialogService.ShowSaveFileDialogAsync("Export Repository", "ZIP Files (*.zip)|*.zip|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                SetBusy(true, "Exporting repository...");
                try
                {
                    await _importExportService.ExportRepositoryAsync(Repository, path);
                    SetStatusMessage("Repository exported");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to export repository");
                    await _dialogService.ShowErrorAsync("Error", ex.Message);
                }
                finally
                {
                    SetBusy(false);
                }
            }
        }

        [RelayCommand]
        private async Task ExportM3UAsync()
        {
            var path = await _dialogService.ShowSaveFileDialogAsync("Export M3U Playlist", "M3U Files (*.m3u)|*.m3u|M3U8 Files (*.m3u8)|*.m3u8|All Files (*.*)|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                SetBusy(true, "Exporting M3U...");
                try
                {
                    var channels = ShowFavoritesOnly ? Repository.GetFavorites() : Repository.AllChannels.Where(c => c.IsVisible).ToList();
                    await _importExportService.ExportM3UAsync(channels, path);
                    SetStatusMessage("M3U exported");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to export M3U");
                    await _dialogService.ShowErrorAsync("Error", ex.Message);
                }
                finally
                {
                    SetBusy(false);
                }
            }
        }

        [RelayCommand]
        private async Task AssignEpgAsync()
        {
            var vm = new AssignEpgViewModel(_logger, _dialogService, _importExportService);
            vm.Repository = Repository;
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                UpdateCollections();
            }
        }

        [RelayCommand]
        private async Task AssignLogosAsync()
        {
            var vm = new AssignLogosViewModel(_logger, _dialogService, _importExportService);
            vm.Repository = Repository;
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                UpdateCollections();
            }
        }

        [RelayCommand]
        private async Task EditChannelAsync(IChannel? channel = null)
        {
            channel ??= SelectedChannel;
            if (channel == null) return;

            var vm = new EditChannelViewModel(_logger, _dialogService);
            vm.Channel = channel;
            vm.Repository = Repository;
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                Repository.IsModified = true;
                UpdateCollections();
            }
        }

        [RelayCommand]
        private async Task DeleteChannelsAsync()
        {
            var selected = Channels.Where(c => c.IsSelected).ToList();
            if (selected.Count == 0) return;

            var confirm = await _dialogService.ShowConfirmAsync(
                "Delete Channels",
                $"Delete {selected.Count} selected channel(s)?");

            if (confirm)
            {
                foreach (var ch in selected)
                {
                    Repository.RemoveChannel(ch);
                }
                UpdateCollections();
                UpdateStatistics();
                SetStatusMessage($"Deleted {selected.Count} channel(s)");
            }
        }

        [RelayCommand]
        private async Task PlayChannelAsync(IChannel? channel = null)
        {
            channel ??= SelectedChannel;
            if (channel == null) return;

            try
            {
                await _streamService.PlayAsync(channel);
                SetStatusMessage($"Playing: {channel.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to play channel");
                await _dialogService.ShowErrorAsync("Playback Error", ex.Message);
            }
        }

        [RelayCommand]
        private void SortChannels(ChannelSort sort)
        {
            CurrentSort = sort;
            Repository.SortChannels(sort);
            UpdateCollections();
        }

        [RelayCommand]
        private void ToggleFavoritesFilter()
        {
            ShowFavoritesOnly = !ShowFavoritesOnly;
            UpdateCollections();
        }

        [RelayCommand]
        private void ToggleScrambledFilter()
        {
            ShowScrambledOnly = !ShowScrambledOnly;
            UpdateCollections();
        }

        [RelayCommand]
        private void ToggleHDFilter()
        {
            ShowHDOnly = !ShowHDOnly;
            UpdateCollections();
        }

        [RelayCommand]
        private void SearchChannels()
        {
            UpdateCollections();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
            UpdateCollections();
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            var vm = new SettingsViewModel(_logger, _dialogService, _profileService);
            vm.Profile = ActiveProfile;
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                ApplyProfileSettings();
            }
        }

        [RelayCommand]
        private async Task OpenProfilesAsync()
        {
            var vm = new ProfilesViewModel(_logger, _dialogService, _profileService);
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true && vm.SelectedProfile != null)
            {
                ActiveProfile = vm.SelectedProfile;
                await _profileService.SetActiveProfileAsync(ActiveProfile.UniqueKey);
                ApplyProfileSettings();
                if (!string.IsNullOrEmpty(ActiveProfile.RepositoryPath))
                {
                    await LoadRepositoryAsync(ActiveProfile.RepositoryPath);
                }
            }
        }

        [RelayCommand]
        private async Task ManageDevicesAsync()
        {
            var vm = new NetworkDevicesViewModel(_logger, _dialogService);
            vm.Devices = new ObservableCollection<NetworkDevice>(Devices);
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                Devices = vm.Devices;
            }
        }

        [RelayCommand]
        private async Task ManageSatellitesAsync()
        {
            var vm = new SatellitesViewModel(_logger, _dialogService);
            vm.Satellites = new ObservableCollection<Satellite>(Satellites);
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                Satellites = vm.Satellites;
            }
        }

        [RelayCommand]
        private async Task MapSatellitesAsync()
        {
            var vm = new MapSatellitesViewModel(_logger, _dialogService, _importExportService);
            vm.Repository = Repository;
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                UpdateCollections();
            }
        }

        [RelayCommand]
        private async Task CreateBouquetsAsync()
        {
            var vm = new CreateBouquetsViewModel(_logger, _dialogService);
            vm.Repository = Repository;
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                UpdateCollections();
            }
        }

        [RelayCommand]
        private async Task BatchEditEpgAsync()
        {
            var vm = new BatchEditEpgViewModel(_logger, _dialogService);
            vm.Repository = Repository;
            var result = await _dialogService.ShowDialogAsync(vm);
            if (result == true)
            {
                UpdateCollections();
            }
        }

        [RelayCommand]
        private void ExpandAllGroups()
        {
            foreach (var group in Groups)
            {
                group.IsExpanded = true;
            }
        }

        [RelayCommand]
        private void CollapseAllGroups()
        {
            foreach (var group in Groups)
            {
                group.IsExpanded = false;
            }
        }

        [RelayCommand]
        private void ExpandGroup(ChannelGroup group)
        {
            if (group != null)
                group.IsExpanded = true;
        }

        [RelayCommand]
        private void CollapseGroup(ChannelGroup group)
        {
            if (group != null)
                group.IsExpanded = false;
        }

        [RelayCommand]
        private void EditGroup(ChannelGroup group)
        {
            if (group == null) return;
            // Implementation for editing group
        }

        [RelayCommand]
        private void AddSubGroup(ChannelGroup parent)
        {
            if (parent == null) return;
            // Implementation for adding subgroup
        }

        [RelayCommand]
        private void DeleteGroup(ChannelGroup group)
        {
            if (group == null) return;
            // Implementation for deleting group
        }

        [RelayCommand]
        private void SelectAllChannels()
        {
            foreach (var channel in Channels)
                channel.IsSelected = true;
        }

        [RelayCommand]
        private void DeselectAllChannels()
        {
            foreach (var channel in Channels)
                channel.IsSelected = false;
        }

        [RelayCommand]
        private void CopyChannels()
        {
            // Implementation for copying channels
        }

        [RelayCommand]
        private void PasteChannels()
        {
            // Implementation for pasting channels
        }

        [RelayCommand]
        private void AddToFavorites()
        {
            foreach (var channel in Channels.Where(c => c.IsSelected))
                channel.Flags |= ServiceFlags.Favorite;
            Repository.IsModified = true;
            UpdateCollections();
        }

        [RelayCommand]
        private void RemoveFromFavorites()
        {
            foreach (var channel in Channels.Where(c => c.IsSelected))
                channel.Flags &= ~ServiceFlags.Favorite;
            Repository.IsModified = true;
            UpdateCollections();
        }

        [RelayCommand]
        private async Task CleanupAsync()
        {
            var confirm = await _dialogService.ShowConfirmAsync("Cleanup", "Remove unused channels and clean up repository?");
            if (confirm)
            {
                Repository.Cleanup();
                Repository.IsModified = true;
                UpdateCollections();
                UpdateStatistics();
                SetStatusMessage("Repository cleaned up");
            }
        }

        [RelayCommand]
        private async Task CheckUpdatesAsync()
        {
            // Implementation for checking updates
            await _dialogService.ShowInfoAsync("Updates", "Check for updates not yet implemented");
        }

        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _dialogService.ShowInfoAsync("Help", "E-Channelizer Help\n\nDocumentation and support available online.");
        }

        [RelayCommand]
        private async Task ShowAboutAsync()
        {
            await _dialogService.ShowInfoAsync("About", "E-Channelizer v1.0.0\nDVB Channel Editor for Satellite, Cable and Terrestrial");
        }

        [RelayCommand]
        private async Task OpenTicketAsync()
        {
            await _dialogService.ShowInfoAsync("Support", "Support ticket system not yet implemented");
        }

        [RelayCommand]
        private async Task ExitAsync()
        {
            Application.Current.Shutdown();
        }

        private void UpdateCollections()
        {
            Groups.Clear();
            foreach (var group in Repository.Groups)
                Groups.Add(group);

            var filteredChannels = Repository.AllChannels.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredChannels = filteredChannels.Where(c =>
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Provider.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Satellite.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (ShowFavoritesOnly)
                filteredChannels = filteredChannels.Where(c => c.IsFavorite);

            if (ShowScrambledOnly)
                filteredChannels = filteredChannels.Where(c => c.IsScrambled);

            if (ShowHDOnly)
                filteredChannels = filteredChannels.Where(c => c.IsHD || c.IsUHD);

            if (SelectedGroup != null)
            {
                filteredChannels = filteredChannels.Where(c => SelectedGroup.GetAllChannels().Contains(c));
            }

            Channels.Clear();
            foreach (var channel in filteredChannels)
                Channels.Add(channel);

            Transponders.Clear();
            foreach (var transponder in Repository.Transponders)
                Transponders.Add(transponder);

            Satellites.Clear();
            foreach (var satellite in Repository.Satellites)
                Satellites.Add(satellite);

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            TotalChannels = Repository.AllChannels.Count;
            VisibleChannels = Channels.Count;
            FavoriteChannels = Repository.AllChannels.Count(c => c.IsFavorite);
            ScrambledChannels = Repository.AllChannels.Count(c => c.IsScrambled);
        }

        partial void OnSelectedGroupChanged(ChannelGroup? value)
        {
            UpdateCollections();
        }

        partial void OnSearchTextChanged(string value)
        {
            UpdateCollections();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}