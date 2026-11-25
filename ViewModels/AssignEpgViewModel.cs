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
    /// Assign EPG to Channels ViewModel
    /// </summary>
    public partial class AssignEpgViewModel : ScreenViewModel
    {
        private readonly ILogger<AssignEpgViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly IImportExportService _importExportService;

        public AssignEpgViewModel(ILogger<AssignEpgViewModel> logger, IDialogService dialogService, IImportExportService importExportService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _importExportService = importExportService;

            Title = "Assign EPG";
        }

        [ObservableProperty]
        private ChannelRepository _repository = new();

        [ObservableProperty]
        private ObservableCollection<XmlTvChannel> _xmlTvChannels = new();

        [ObservableProperty]
        private ObservableCollection<IChannel> _channels = new();

        [ObservableProperty]
        private XmlTvChannel _selectedXmlChannel = null!;

        [ObservableProperty]
        private IChannel _selectedChannel = null!;

        [ObservableProperty]
        private string _xmlTvPath = string.Empty;

        [ObservableProperty]
        private int _mappedCount;

        [ObservableProperty]
        private int _unmappedXmlCount;

        [ObservableProperty]
        private int _unmappedChannelCount;

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
                    Channels.Add(ch);
                }
            }
        }

        [RelayCommand]
        private async Task LoadXmlTvAsync()
        {
            if (string.IsNullOrEmpty(XmlTvPath))
            {
                var path = await _dialogService.ShowOpenFileDialogAsync("Select XMLTV File", "XML Files (*.xml)|*.xml|All Files (*.*)|*.*");
                if (!string.IsNullOrEmpty(path))
                {
                    XmlTvPath = path;
                }
            }

            if (!string.IsNullOrEmpty(XmlTvPath))
            {
                SetBusy(true, "Loading XMLTV...");
                try
                {
                    // Parse XMLTV
                    var doc = System.Xml.Linq.XDocument.Load(XmlTvPath);
                    XmlTvChannels.Clear();

                    foreach (var channel in doc.Descendants("channel"))
                    {
                        var xmlChannel = new XmlTvChannel
                        {
                            Id = channel.Attribute("id")?.Value ?? "",
                            Name = channel.Element("display-name")?.Value ?? ""
                        };
                        XmlTvChannels.Add(xmlChannel);
                    }

                    UnmappedXmlCount = XmlTvChannels.Count;
                    UnmappedChannelCount = Channels.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load XMLTV");
                    await _dialogService.ShowErrorAsync("Error", ex.Message);
                }
                finally
                {
                    SetBusy(false);
                }
            }
        }

        [RelayCommand]
        private async Task AutoMapAsync()
        {
            if (Repository == null || XmlTvChannels.Count == 0) return;

            SetBusy(true, "Auto-mapping...");
            try
            {
                var xmlChannels = XmlTvChannels.ToList();
                var channels = Repository.AllChannels.ToList();

                var result = await _importExportService.MapEpgToChannelsAsync(xmlChannels, channels);

                MappedCount = result.Mapped;
                UnmappedXmlCount = result.Unmapped;
                UnmappedChannelCount = result.UnmappedChannels.Count;

                SetStatusMessage($"Mapped {result.Mapped} channels");
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
        private void MapSelected()
        {
            if (SelectedXmlChannel != null && SelectedChannel != null)
            {
                SelectedXmlChannel.MappedChannel = SelectedChannel;
                SelectedXmlChannel.IsMapped = true;
                MappedCount++;
                UnmappedXmlCount--;
                UnmappedChannelCount--;
            }
        }

        [RelayCommand]
        private void UnmapSelected()
        {
            if (SelectedXmlChannel != null && SelectedXmlChannel.IsMapped)
            {
                SelectedXmlChannel.MappedChannel = null;
                SelectedXmlChannel.IsMapped = false;
                MappedCount--;
                UnmappedXmlCount++;
                UnmappedChannelCount++;
            }
        }

        [RelayCommand]
        private void ClearAllMappings()
        {
            foreach (var xmlChannel in XmlTvChannels)
            {
                xmlChannel.MappedChannel = null;
                xmlChannel.IsMapped = false;
            }
            MappedCount = 0;
            UnmappedXmlCount = XmlTvChannels.Count;
            UnmappedChannelCount = Channels.Count;
        }

        [RelayCommand]
        private void OK()
        {
            // Save mappings to repository
            if (Repository != null)
            {
                foreach (var xmlChannel in XmlTvChannels.Where(x => x.IsMapped && x.MappedChannel != null))
                {
                    xmlChannel.MappedChannel.EpgSources.Add(xmlChannel.Id);
                }
                Repository.IsModified = true;
            }
            Close(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }
    }
}