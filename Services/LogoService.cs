using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EChannelizer.Models;
using Microsoft.Extensions.Logging;

namespace EChannelizer.Services
{
    /// <summary>
    /// Logo service for downloading and managing channel logos
    /// </summary>
    public class LogoService : ILogoService
    {
        private readonly ILogger<LogoService> _logger;
        private readonly HttpClient _httpClient;

        public LogoService(ILogger<LogoService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> DownloadLogoAsync(string channelName, string provider, string satellite)
        {
            try
            {
                // Try various logo sources
                var urls = GenerateLogoUrls(channelName, provider, satellite);

                foreach (var url in urls)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            var contentType = response.Content.Headers.ContentType?.MediaType;
                            var extension = GetExtensionFromContentType(contentType) ?? ".png";

                            var fileName = SanitizeFileName($"{channelName}{extension}");
                            var logosFolder = GetLogosFolder();
                            var filePath = Path.Combine(logosFolder, fileName);

                            Directory.CreateDirectory(logosFolder);

                            using var stream = await response.Content.ReadAsStreamAsync();
                            using var fileStream = File.Create(filePath);
                            await stream.CopyToAsync(fileStream);

                            _logger.LogInformation("Downloaded logo for {Channel} from {Url}", channelName, url);
                            return filePath;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to download logo from {Url}", url);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download logo for {Channel}", channelName);
            }

            return string.Empty;
        }

        public async Task<string> FindLocalLogoAsync(string channelName, string logosFolder)
        {
            if (!Directory.Exists(logosFolder))
                return string.Empty;

            var searchNames = new[]
            {
                channelName,
                channelName.Replace(" ", ""),
                channelName.Replace(" ", "_"),
                channelName.ToLowerInvariant(),
                channelName.ToUpperInvariant()
            };

            var extensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".webp" };

            foreach (var name in searchNames)
            {
                foreach (var ext in extensions)
                {
                    var fileName = SanitizeFileName($"{name}{ext}");
                    var filePath = Path.Combine(logosFolder, fileName);

                    if (File.Exists(filePath))
                        return filePath;
                }
            }

            // Try fuzzy matching
            var files = Directory.GetFiles(logosFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => IsImageFile(f))
                .ToList();

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (IsSimilarName(channelName, fileName))
                {
                    return file;
                }
            }

            return string.Empty;
        }

        public async Task AssignLogosAsync(ChannelRepository repository, string logosFolder)
        {
            _logger.LogInformation("Assigning logos for {Count} channels", repository.AllChannels.Count);

            foreach (var channel in repository.AllChannels)
            {
                if (string.IsNullOrEmpty(channel.LogoPath))
                {
                    var logoPath = await FindLocalLogoAsync(channel.Name, logosFolder);
                    if (!string.IsNullOrEmpty(logoPath))
                    {
                        channel.LogoPath = logoPath;
                    }
                }
            }

            repository.IsModified = true;
        }

        public async Task ExportLogosAsync(ChannelRepository repository, string targetFolder)
        {
            Directory.CreateDirectory(targetFolder);

            foreach (var channel in repository.AllChannels)
            {
                if (!string.IsNullOrEmpty(channel.LogoPath) && File.Exists(channel.LogoPath))
                {
                    var fileName = SanitizeFileName($"{channel.Name}{Path.GetExtension(channel.LogoPath)}");
                    var destPath = Path.Combine(targetFolder, fileName);

                    try
                    {
                        File.Copy(channel.LogoPath, destPath, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to copy logo for {Channel}", channel.Name);
                    }
                }
            }
        }

        public async Task CleanupUnusedLogosAsync(string logosFolder, ChannelRepository repository)
        {
            if (!Directory.Exists(logosFolder))
                return;

            var usedLogos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var channel in repository.AllChannels)
            {
                if (!string.IsNullOrEmpty(channel.LogoPath))
                {
                    usedLogos.Add(Path.GetFileName(channel.LogoPath).ToLowerInvariant());
                }
            }

            var files = Directory.GetFiles(logosFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => IsImageFile(f))
                .ToList();

            int deleted = 0;
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file).ToLowerInvariant();
                if (!usedLogos.Contains(fileName))
                {
                    try
                    {
                        File.Delete(file);
                        deleted++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete unused logo {File}", file);
                    }
                }
            }

            _logger.LogInformation("Cleaned up {Count} unused logos", deleted);
        }

        private string[] GenerateLogoUrls(string channelName, string provider, string satellite)
        {
            var urls = new List<string>();
            var encodedName = Uri.EscapeDataString(channelName);
            var encodedProvider = Uri.EscapeDataString(provider);

            // Common logo sources
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/us/{encodedName}.png");
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/de/{encodedName}.png");
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/fr/{encodedName}.png");
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/uk/{encodedName}.png");
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/es/{encodedName}.png");
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/it/{encodedName}.png");
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/nl/{encodedName}.png");
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/pl/{encodedName}.png");
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/tr/{encodedName}.png");
            urls.Add($"https://raw.githubusercontent.com/tv-logo/tv-logos/main/countries/ru/{encodedName}.png");

            // Generic sources
            urls.Add($"https://logos.tv/logo/{encodedName}.png");
            urls.Add($"https://www.lyngsat-logo.com/logo/{encodedProvider}/{encodedName}.png");

            return urls.ToArray();
        }

        private string GetLogosFolder()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "E-Channelizer", "Logos");
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        private bool IsImageFile(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".webp" or ".gif";
        }

        private string? GetExtensionFromContentType(string? contentType)
        {
            return contentType?.ToLowerInvariant() switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/bmp" => ".bmp",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => null
            };
        }

        private bool IsSimilarName(string name1, string name2)
        {
            // Simple similarity check
            var n1 = name1.ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");
            var n2 = name2.ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");

            return n1 == n2 ||
                   n1.Contains(n2) ||
                   n2.Contains(n1) ||
                   LevenshteinDistance(n1, n2) <= 2;
        }

        private int LevenshteinDistance(string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            for (var i = 0; i <= n; i++) d[i, 0] = i;
            for (var j = 0; j <= m; j++) d[0, j] = j;

            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}