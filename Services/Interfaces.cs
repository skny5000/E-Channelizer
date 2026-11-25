using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EChannelizer.Models;

namespace EChannelizer.Services
{
    /// <summary>
    /// Repository service for loading/saving channel repositories
    /// </summary>
    public interface IRepositoryService
    {
        Task<ChannelRepository> LoadAsync(string path);
        Task SaveAsync(ChannelRepository repository, string path);
        Task<ChannelRepository> CreateNewAsync(string name);
        bool ValidateRepository(string path);
        Task<ImportResult> ImportAsync(string path, ImportOptions options);
        Task ExportAsync(ChannelRepository repository, string path, ExportOptions options);
    }

    /// <summary>
    /// Profile service for managing user profiles
    /// </summary>
    public interface IProfileService
    {
        Task<Profile> LoadActiveProfileAsync();
        Task SaveProfileAsync(Profile profile);
        Task SetActiveProfileAsync(string profileKey);
        Task<List<Profile>> GetProfilesAsync();
        Task<Profile> CreateProfileAsync(string name);
        Task DeleteProfileAsync(string profileKey);
        Task<Profile> CloneProfileAsync(string profileKey, string newName);
    }

    /// <summary>
    /// Dialog service for showing dialogs
    /// </summary>
    public interface IDialogService
    {
        Task<bool?> ShowDialogAsync(ViewModelBase viewModel);
        Task<string> ShowOpenFileDialogAsync(string title, string filter);
        Task<string> ShowSaveFileDialogAsync(string title, string filter);
        Task<bool> ShowConfirmAsync(string title, string message);
        Task ShowErrorAsync(string title, string message);
        Task ShowInfoAsync(string title, string message);
        Task ShowWarningAsync(string title, string message);
        Task<T?> ShowInputDialogAsync<T>(string title, string message, T defaultValue = default!);
    }

    /// <summary>
    /// Stream service for playing channels
    /// </summary>
    public interface IStreamService
    {
        Task PlayAsync(IChannel channel);
        Task StopAsync();
        Task PauseAsync();
        Task ResumeAsync();
        Task<StreamInfo> GetStreamInfoAsync(IChannel channel);
        bool IsPlaying { get; }
        PlayerStatus Status { get; }
        event EventHandler<StreamEventArgs> StreamStatusChanged;
    }

    /// <summary>
    /// Import/Export service
    /// </summary>
    public interface IImportExportService
    {
        Task<ImportResult> ImportSatellitesAsync(string sourcePath, ImportSatellitesOptions options);
        Task<ImportResult> ImportDvbAsync(string sourcePath, ImportDvbOptions options);
        Task<ImportResult> ImportEpgAsync(string sourcePath, EpgSourceType type);
        Task<ImportResult> ImportLogosAsync(string sourcePath);
        Task<ImportResult> ImportM3UAsync(string sourcePath);
        Task<ImportResult> ImportRepositoryAsync(string sourcePath);
        Task ExportRepositoryAsync(ChannelRepository repository, string targetPath);
        Task ExportM3UAsync(List<IChannel> channels, string targetPath);
        Task ExportSatellitesAsync(List<Satellite> satellites, string targetPath);
        Task ExportLogosAsync(Dictionary<string, string> logos, string targetPath);
        Task ExportEpgAsync(List<EpgProgram> programs, string targetPath);
    }

    /// <summary>
    /// Settings service
    /// </summary>
    public interface ISettingsService
    {
        T GetSetting<T>(string key, T defaultValue = default!);
        void SetSetting<T>(string key, T value);
        Task SaveAsync();
        Task LoadAsync();
        void ResetToDefaults();
    }

    /// <summary>
    /// Update service
    /// </summary>
    public interface IUpdateService
    {
        Task<UpdateInfo> CheckForUpdatesAsync();
        Task DownloadUpdateAsync(UpdateInfo update, IProgress<double> progress);
        Task InstallUpdateAsync(string updatePath);
        UpdateStatus CurrentStatus { get; }
    }

    /// <summary>
    /// Logging service
    /// </summary>
    public interface ILoggingService
    {
        void LogDebug(string message, params object[] args);
        void LogInfo(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, Exception? ex = null, params object[] args);
        void LogCritical(string message, Exception? ex = null, params object[] args);
    }

    /// <summary>
    /// Network device service
    /// </summary>
    public interface INetworkDeviceService
    {
        Task<DeviceInfo> GetDeviceInfoAsync(NetworkDevice device);
        Task<List<ChannelGroup>> GetBouquetsAsync(NetworkDevice device);
        Task<List<IChannel>> GetChannelsAsync(NetworkDevice device, string bouquetRef);
        Task<List<TransponderBase>> GetTranspondersAsync(NetworkDevice device);
        Task<List<Satellite>> GetSatellitesAsync(NetworkDevice device);
        Task<bool> TestConnectionAsync(NetworkDevice device);
        Task SendChannelListAsync(NetworkDevice device, ChannelRepository repository);
        Task SendBouquetsAsync(NetworkDevice device, List<ChannelGroup> bouquets);
        Task RebootAsync(NetworkDevice device);
        Task RestartEnigmaAsync(NetworkDevice device);
    }

    /// <summary>
    /// EPG service
    /// </summary>
    public interface IEpgService
    {
        Task<List<EpgProgram>> LoadEpgAsync(EpgSource source);
        Task UpdateEpgAsync(EpgSource source);
        Task<MapResult> MapEpgToChannelsAsync(List<XmlTvChannel> xmlChannels, List<IChannel> channels);
        Task<List<EpgProgram>> GetEpgForChannelAsync(IChannel channel, DateTime start, DateTime end);
        EpgManager Manager { get; }
    }

    /// <summary>
    /// Logo service
    /// </summary>
    public interface ILogoService
    {
        Task<string> DownloadLogoAsync(string channelName, string provider, string satellite);
        Task<string> FindLocalLogoAsync(string channelName, string logosFolder);
        Task AssignLogosAsync(ChannelRepository repository, string logosFolder);
        Task ExportLogosAsync(ChannelRepository repository, string targetFolder);
        Task CleanupUnusedLogosAsync(string logosFolder, ChannelRepository repository);
    }

    // Data transfer objects

    public class ImportOptions
    {
        public ImportSatellitesOptions SatelliteOptions { get; set; } = ImportSatellitesOptions.All;
        public ImportDvbOptions DvbOptions { get; set; } = ImportDvbOptions.All;
        public bool CreateBackup { get; set; } = true;
        public bool CreateGroups { get; set; } = true;
        public string TargetSatellite { get; set; } = string.Empty;
    }

    public class ExportOptions
    {
        public bool IncludeChannels { get; set; } = true;
        public bool IncludeGroups { get; set; } = true;
        public bool IncludeTransponders { get; set; } = true;
        public bool IncludeSatellites { get; set; } = true;
        public bool IncludeDevices { get; set; } = true;
        public bool IncludeEpg { get; set; } = false;
        public bool IncludeLogos { get; set; } = false;
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public int ChannelsImported { get; set; }
        public int GroupsCreated { get; set; }
        int TranspondersImported { get; set; }
        public int SatellitesImported { get; set; }
        public int LogosImported { get; set; }
        public int EpgSourcesImported { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public TimeSpan Duration { get; set; }
    }

    public class MapResult
    {
        public int Mapped { get; set; }
        public int Unmapped { get; set; }
        public List<string> UnmappedChannels { get; set; } = new();
        public List<string> UnmappedXmlChannels { get; set; } = new();
    }

    public class StreamInfo
    {
        public string Url { get; set; } = string.Empty;
        public StreamTsType Type { get; set; }
        public long Bitrate { get; set; }
        public string Codec { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public double FrameRate { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class StreamEventArgs : EventArgs
    {
        public PlayerStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public IChannel? Channel { get; set; }
    }

    public class UpdateInfo
    {
        public string Version { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public bool IsMandatory { get; set; }
        public long Size { get; set; }
        public string Checksum { get; set; } = string.Empty;
    }
}