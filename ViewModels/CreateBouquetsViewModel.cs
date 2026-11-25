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
    /// Create Bouquets ViewModel
    /// </summary>
    public partial class CreateBouquetsViewModel : ScreenViewModel
    {
        private readonly ILogger<CreateBouquetsViewModel> _logger;
        private readonly IDialogService _dialogService;

        public CreateBouquetsViewModel(ILogger<CreateBouquetsViewModel> logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;

            Title = "Create Bouquets";
        }

        [ObservableProperty]
        private ChannelRepository _repository = new();

        [ObservableProperty]
        private ObservableCollection<BouquetTemplate> _templates = new();

        [ObservableProperty]
        private BouquetTemplate _selectedTemplate = null!;

        [ObservableProperty]
        private string _bouquetName = string.Empty;

        [ObservableProperty]
        private bool _createPerSatellite = true;

        [ObservableProperty]
        private bool _createPerProvider = true;

        [ObservableProperty]
        private bool _createPerType = true;

        [ObservableProperty]
        private bool _createFavorites = true;

        [ObservableProperty]
        private bool _includeScrambled = true;

        [ObservableProperty]
        private bool _includeHidden = false;

        public override void Initialize()
        {
            base.Initialize();
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            Templates.Add(new BouquetTemplate { Name = "By Satellite", Description = "Create one bouquet per satellite" });
            Templates.Add(new BouquetTemplate { Name = "By Provider", Description = "Create one bouquet per provider" });
            Templates.Add(new BouquetTemplate { Name = "By Type (TV/Radio)", Description = "Separate TV and Radio channels" });
            Templates.Add(new BouquetTemplate { Name = "Favorites Only", Description = "Create bouquet with only favorite channels" });
            Templates.Add(new BouquetTemplate { Name = "Custom", Description = "Custom bouquet creation" });
        }

        [RelayCommand]
        private void CreateBouquets()
        {
            if (Repository == null) return;

            var created = 0;

            if (CreatePerSatellite)
            {
                var satellites = Repository.AllChannels
                    .Where(c => !string.IsNullOrEmpty(c.Satellite))
                    .Select(c => c.Satellite)
                    .Distinct()
                    .ToList();

                foreach (var sat in satellites)
                {
                    var channels = Repository.AllChannels
                        .Where(c => c.Satellite == sat &&
                                   (IncludeScrambled || !c.IsScrambled) &&
                                   (IncludeHidden || !c.IsHidden))
                        .ToList();

                    if (channels.Count > 0)
                    {
                        var group = new ChannelGroup
                        {
                            Name = sat,
                            Source = BouquetSource.Satellite,
                            Satellite = sat
                        };
                        group.Channels.AddRange(channels);
                        Repository.Groups.Add(group);
                        created++;
                    }
                }
            }

            if (CreatePerProvider)
            {
                var providers = Repository.AllChannels
                    .Where(c => !string.IsNullOrEmpty(c.Provider))
                    .Select(c => c.Provider)
                    .Distinct()
                    .ToList();

                foreach (var provider in providers)
                {
                    var channels = Repository.AllChannels
                        .Where(c => c.Provider == provider &&
                                   (IncludeScrambled || !c.IsScrambled) &&
                                   (IncludeHidden || !c.IsHidden))
                        .ToList();

                    if (channels.Count > 0)
                    {
                        var group = new ChannelGroup
                        {
                            Name = provider,
                            Source = BouquetSource.Provider,
                            Satellite = string.Empty
                        };
                        group.Channels.AddRange(channels);
                        Repository.Groups.Add(group);
                        created++;
                    }
                }
            }

            if (CreatePerType)
            {
                var tvChannels = Repository.AllChannels
                    .Where(c => c.ServiceType == ServiceType.TV &&
                               (IncludeScrambled || !c.IsScrambled) &&
                               (IncludeHidden || !c.IsHidden))
                    .ToList();

                if (tvChannels.Count > 0)
                {
                    var group = new ChannelGroup { Name = "TV Channels", Source = BouquetSource.User };
                    group.Channels.AddRange(tvChannels);
                    Repository.Groups.Add(group);
                    created++;
                }

                var radioChannels = Repository.AllChannels
                    .Where(c => c.ServiceType == ServiceType.Radio &&
                               (IncludeScrambled || !c.IsScrambled) &&
                               (IncludeHidden || !c.IsHidden))
                    .ToList();

                if (radioChannels.Count > 0)
                {
                    var group = new ChannelGroup { Name = "Radio Channels", Source = BouquetSource.User };
                    group.Channels.AddRange(radioChannels);
                    Repository.Groups.Add(group);
                    created++;
                }
            }

            if (CreateFavorites)
            {
                var favorites = Repository.GetFavorites();
                if (favorites.Count > 0)
                {
                    var group = new ChannelGroup { Name = "Favorites", Source = BouquetSource.User };
                    group.Channels.AddRange(favorites);
                    Repository.Groups.Add(group);
                    created++;
                }
            }

            Repository.IsModified = true;
            SetStatusMessage($"Created {created} bouquets");
            Close(true);
        }

        [RelayCommand]
        private void CreateCustomBouquet()
        {
            if (Repository == null || string.IsNullOrWhiteSpace(BouquetName)) return;

            var group = new ChannelGroup
            {
                Name = BouquetName,
                Source = BouquetSource.User
            };
            Repository.Groups.Add(group);
            Repository.IsModified = true;
            BouquetName = string.Empty;
            SetStatusMessage($"Created custom bouquet: {group.Name}");
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }

        public class BouquetTemplate
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}