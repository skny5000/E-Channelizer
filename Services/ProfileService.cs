using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EChannelizer.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EChannelizer.Services
{
    /// <summary>
    /// Profile service implementation
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly ILogger<ProfileService> _logger;
        private readonly string _profilesPath;
        private readonly string _activeProfileFile;

        public ProfileService(ILogger<ProfileService> logger)
        {
            _logger = logger;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "E-Channelizer");
            _profilesPath = Path.Combine(appFolder, "Profiles");
            _activeProfileFile = Path.Combine(appFolder, "active_profile.json");

            Directory.CreateDirectory(_profilesPath);
        }

        public async Task<Profile> LoadActiveProfileAsync()
        {
            try
            {
                // Try to load active profile key
                string activeKey = string.Empty;
                if (File.Exists(_activeProfileFile))
                {
                    var json = await File.ReadAllTextAsync(_activeProfileFile);
                    var activeData = JsonConvert.DeserializeObject<ActiveProfileData>(json);
                    activeKey = activeData?.ActiveProfileKey ?? string.Empty;
                }

                // Load all profiles
                var profiles = await GetProfilesAsync();

                // Find active profile
                var activeProfile = profiles.FirstOrDefault(p => p.UniqueKey == activeKey);

                // If no active profile found, use first or create default
                if (activeProfile == null)
                {
                    activeProfile = profiles.FirstOrDefault();

                    if (activeProfile == null)
                    {
                        activeProfile = await CreateProfileAsync("Default");
                    }

                    await SetActiveProfileAsync(activeProfile.UniqueKey);
                }

                activeProfile.LastUsed = DateTime.Now;
                await SaveProfileAsync(activeProfile);

                _logger.LogInformation("Loaded active profile: {Name}", activeProfile.Name);
                return activeProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load active profile");
                return new Profile { Name = "Default" };
            }
        }

        public async Task SaveProfileAsync(Profile profile)
        {
            try
            {
                var filePath = Path.Combine(_profilesPath, $"{profile.UniqueKey}.json");
                var json = JsonConvert.SerializeObject(profile, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);

                _logger.LogDebug("Saved profile: {Name}", profile.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save profile");
                throw;
            }
        }

        public async Task SetActiveProfileAsync(string profileKey)
        {
            try
            {
                var data = new ActiveProfileData { ActiveProfileKey = profileKey };
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                await File.WriteAllTextAsync(_activeProfileFile, json);

                _logger.LogInformation("Set active profile: {Key}", profileKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set active profile");
                throw;
            }
        }

        public async Task<List<Profile>> GetProfilesAsync()
        {
            var profiles = new List<Profile>();

            try
            {
                var files = Directory.GetFiles(_profilesPath, "*.json");

                foreach (var file in files)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var profile = JsonConvert.DeserializeObject<Profile>(json);
                        if (profile != null)
                            profiles.Add(profile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load profile from {File}", file);
                    }
                }

                // Sort by last used
                profiles = profiles.OrderByDescending(p => p.LastUsed).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get profiles");
            }

            return profiles;
        }

        public async Task<Profile> CreateProfileAsync(string name)
        {
            var profile = new Profile
            {
                Name = name,
                UniqueKey = Guid.NewGuid().ToString(),
                Created = DateTime.Now,
                LastUsed = DateTime.Now,
                Settings = new ProfileSettings()
            };

            await SaveProfileAsync(profile);
            _logger.LogInformation("Created profile: {Name}", name);

            return profile;
        }

        public async Task DeleteProfileAsync(string profileKey)
        {
            try
            {
                var filePath = Path.Combine(_profilesPath, $"{profileKey}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted profile: {Key}", profileKey);
                }

                // Check if this was the active profile
                var activeData = new ActiveProfileData();
                if (File.Exists(_activeProfileFile))
                {
                    var json = await File.ReadAllTextAsync(_activeProfileFile);
                    activeData = JsonConvert.DeserializeObject<ActiveProfileData>(json) ?? new ActiveProfileData();
                }

                if (activeData.ActiveProfileKey == profileKey)
                {
                    // Set first available profile as active
                    var profiles = await GetProfilesAsync();
                    if (profiles.Count > 0)
                    {
                        await SetActiveProfileAsync(profiles[0].UniqueKey);
                    }
                    else
                    {
                        // Create default profile
                        var defaultProfile = await CreateProfileAsync("Default");
                        await SetActiveProfileAsync(defaultProfile.UniqueKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete profile");
                throw;
            }
        }

        public async Task<Profile> CloneProfileAsync(string profileKey, string newName)
        {
            var profiles = await GetProfilesAsync();
            var sourceProfile = profiles.FirstOrDefault(p => p.UniqueKey == profileKey);

            if (sourceProfile == null)
                throw new ArgumentException($"Profile not found: {profileKey}");

            var newProfile = sourceProfile.Clone() as Profile ?? new Profile();
            newProfile.Name = newName;
            newProfile.UniqueKey = Guid.NewGuid().ToString();
            newProfile.Created = DateTime.Now;
            newProfile.LastUsed = DateTime.Now;

            await SaveProfileAsync(newProfile);
            _logger.LogInformation("Cloned profile: {Source} -> {Target}", profileKey, newProfile.UniqueKey);

            return newProfile;
        }

        private class ActiveProfileData
        {
            public string ActiveProfileKey { get; set; } = string.Empty;
        }
    }
}