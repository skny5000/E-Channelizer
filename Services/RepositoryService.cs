using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EChannelizer.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EChannelizer.Services
{
    /// <summary>
    /// Repository service implementation
    /// </summary>
    public class RepositoryService : IRepositoryService
    {
        private readonly ILogger<RepositoryService> _logger;

        public RepositoryService(ILogger<RepositoryService> logger)
        {
            _logger = logger;
        }

        public async Task<ChannelRepository> LoadAsync(string path)
        {
            _logger.LogInformation("Loading repository from {Path}", path);

            if (!File.Exists(path))
                throw new FileNotFoundException("Repository file not found", path);

            var extension = Path.GetExtension(path).ToLowerInvariant();

            return extension switch
            {
                ".ech" => await LoadEchAsync(path),
                ".zip" => await LoadZipAsync(path),
                ".json" => await LoadJsonAsync(path),
                _ => throw new NotSupportedException($"File format {extension} not supported")
            };
        }

        private async Task<ChannelRepository> LoadEchAsync(string path)
        {
            using var stream = File.OpenRead(path);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);

            var entry = zip.GetEntry("repository.json");
            if (entry == null)
                throw new InvalidDataException("Invalid .ech file: repository.json not found");

            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            var json = await reader.ReadToEndAsync();

            var repository = JsonConvert.DeserializeObject<ChannelRepository>(json)
                ?? throw new InvalidDataException("Failed to deserialize repository");

            // Load logos if present
            foreach (var logoEntry in zip.Entries.Where(e => e.FullName.StartsWith("logos/")))
            {
                var logoName = Path.GetFileName(logoEntry.FullName);
                var logoPath = Path.Combine(Path.GetDirectoryName(path)!, "logos", logoName);

                Directory.CreateDirectory(Path.GetDirectoryName(logoPath)!);
                using var logoStream = logoEntry.Open();
                using var fileStream = File.Create(logoPath);
                await logoStream.CopyToAsync(fileStream);

                repository.Logos[logoName] = logoPath;
            }

            repository.FilePath = path;
            repository.IsModified = false;

            _logger.LogInformation("Repository loaded: {Channels} channels, {Groups} groups",
                repository.AllChannels.Count, repository.Groups.Count);

            return repository;
        }

        private async Task<ChannelRepository> LoadZipAsync(string path)
        {
            // Same as .ech but without logo extraction to temp
            return await LoadEchAsync(path);
        }

        private async Task<ChannelRepository> LoadJsonAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            var repository = JsonConvert.DeserializeObject<ChannelRepository>(json)
                ?? throw new InvalidDataException("Failed to deserialize repository");

            repository.FilePath = path;
            repository.IsModified = false;

            return repository;
        }

        public async Task SaveAsync(ChannelRepository repository, string path)
        {
            _logger.LogInformation("Saving repository to {Path}", path);

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var extension = Path.GetExtension(path).ToLowerInvariant();

            switch (extension)
            {
                case ".ech":
                    await SaveEchAsync(repository, path);
                    break;
                case ".json":
                    await SaveJsonAsync(repository, path);
                    break;
                default:
                    throw new NotSupportedException($"File format {extension} not supported");
            }

            repository.FilePath = path;
            repository.IsModified = false;
            repository.LastSaved = DateTime.Now;

            _logger.LogInformation("Repository saved: {Channels} channels, {Groups} groups",
                repository.AllChannels.Count, repository.Groups.Count);
        }

        private async Task SaveEchAsync(ChannelRepository repository, string path)
        {
            using var stream = File.Create(path);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create);

            // Serialize repository to JSON
            var json = JsonConvert.SerializeObject(repository, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            var entry = zip.CreateEntry("repository.json");
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream);
            await writer.WriteAsync(json);

            // Add logos
            var logosFolder = Path.Combine(Path.GetDirectoryName(path)!, "logos");
            if (Directory.Exists(logosFolder))
            {
                foreach (var logoFile in Directory.GetFiles(logosFolder))
                {
                    var logoName = Path.GetFileName(logoFile);
                    var logoEntry = zip.CreateEntry($"logos/{logoName}");
                    using var logoEntryStream = logoEntry.Open();
                    using var fileStream = File.OpenRead(logoFile);
                    await fileStream.CopyToAsync(logoEntryStream);
                }
            }
        }

        private async Task SaveJsonAsync(ChannelRepository repository, string path)
        {
            var json = JsonConvert.SerializeObject(repository, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

            await File.WriteAllTextAsync(path, json);
        }

        public async Task<ChannelRepository> CreateNewAsync(string name)
        {
            await Task.CompletedTask;

            return new ChannelRepository
            {
                Name = name,
                FilePath = string.Empty,
                IsModified = true
            };
        }

        public bool ValidateRepository(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return false;

                var extension = Path.GetExtension(path).ToLowerInvariant();

                if (extension == ".ech" || extension == ".zip")
                {
                    using var stream = File.OpenRead(path);
                    using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
                    return zip.GetEntry("repository.json") != null;
                }
                else if (extension == ".json")
                {
                    var json = File.ReadAllText(path);
                    var repo = JsonConvert.DeserializeObject<ChannelRepository>(json);
                    return repo != null;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ImportResult> ImportAsync(string path, ImportOptions options)
        {
            var result = new ImportResult();

            try
            {
                var extension = Path.GetExtension(path).ToLowerInvariant();

                switch (extension)
                {
                    case ".ech":
                    case ".zip":
                        var repo = await LoadAsync(path);
                        MergeRepositories(result, repo, options);
                        break;
                    case ".json":
                        var jsonRepo = await LoadJsonAsync(path);
                        MergeRepositories(result, jsonRepo, options);
                        break;
                    case ".m3u":
                    case ".m3u8":
                        // Import M3U playlist
                        result = await ImportM3UAsync(path);
                        break;
                    default:
                        result.Success = false;
                        result.Errors.Add($"Unsupported file format: {extension}");
                        break;
                }

                result.Success = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import failed");
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private void MergeRepositories(ImportResult result, ChannelRepository source, ImportOptions options)
        {
            // This would merge the source repository into the target
            // Implementation depends on specific merge strategy
            result.ChannelsImported = source.AllChannels.Count;
            result.GroupsCreated = source.Groups.Count;
            result.TranspondersImported = source.Transponders.Count;
            result.SatellitesImported = source.Satellites.Count;
        }

        public async Task ExportAsync(ChannelRepository repository, string path, ExportOptions options)
        {
            await SaveAsync(repository, path);
        }

        private async Task<ImportResult> ImportM3UAsync(string path)
        {
            var result = new ImportResult();
            var lines = await File.ReadAllLinesAsync(path);

            IChannel? currentChannel = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("#EXTINF:"))
                {
                    // Parse EXTINF line
                    var info = trimmed.Substring(8);
                    var commaIndex = info.LastIndexOf(',');
                    if (commaIndex > 0)
                    {
                        var attributes = info.Substring(0, commaIndex);
                        var name = info.Substring(commaIndex + 1).Trim();

                        currentChannel = new IptvChannel
                        {
                            Name = name,
                            ServiceType = ServiceType.TV
                        };

                        // Parse attributes
                        ParseM3UAttributes(attributes, currentChannel as IptvChannel);
                    }
                }
                else if (!trimmed.StartsWith("#") && currentChannel != null)
                {
                    // URL line
                    if (currentChannel is IptvChannel iptv)
                    {
                        iptv.Url = trimmed;
                        result.ChannelsImported++;
                        currentChannel = null;
                    }
                }
            }

            result.Success = true;
            return result;
        }

        private void ParseM3UAttributes(string attributes, IptvChannel? channel)
        {
            if (channel == null) return;

            // Parse tvg-id, tvg-name, tvg-logo, group-title, etc.
            var parts = attributes.Split(' ');
            foreach (var part in parts)
            {
                if (part.StartsWith("tvg-id="))
                    channel.ServiceId = int.TryParse(part.Substring(7).Trim('"'), out var id) ? id : 0;
                else if (part.StartsWith("tvg-logo="))
                    channel.LogoPath = part.Substring(9).Trim('"');
                else if (part.StartsWith("group-title="))
                    channel.Satellite = part.Substring(12).Trim('"'); // Using Satellite field for group
            }
        }
    }
}