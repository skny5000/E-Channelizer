using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EChannelizer.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EChannelizer.Services
{
    /// <summary>
    /// Settings service for application settings
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly Dictionary<string, object> _settings = new();
        private readonly string _settingsFile;

        public SettingsService(ILogger<SettingsService> logger)
        {
            _logger = logger;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "E-Channelizer");
            _settingsFile = Path.Combine(appFolder, "settings.json");

            Directory.CreateDirectory(appFolder);
        }

        public T GetSetting<T>(string key, T defaultValue = default!)
        {
            if (_settings.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public void SetSetting<T>(string key, T value)
        {
            _settings[key] = value!;
        }

        public async Task SaveAsync()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                await File.WriteAllTextAsync(_settingsFile, json);
                _logger.LogDebug("Settings saved to {File}", _settingsFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save settings");
            }
        }

        public async Task LoadAsync()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    var json = await File.ReadAllTextAsync(_settingsFile);
                    var loaded = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (loaded != null)
                    {
                        foreach (var kvp in loaded)
                        {
                            _settings[kvp.Key] = kvp.Value;
                        }
                    }
                    _logger.LogDebug("Settings loaded from {File}", _settingsFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load settings");
            }
        }

        public void ResetToDefaults()
        {
            _settings.Clear();
            _logger.LogInformation("Settings reset to defaults");
        }
    }
}