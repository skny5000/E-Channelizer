using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EChannelizer.Models;
using Microsoft.Extensions.Logging;

namespace EChannelizer.Services
{
    /// <summary>
    /// Network Device service for Enigma2, Dreambox, etc.
    /// </summary>
    public class NetworkDeviceService : INetworkDeviceService
    {
        private readonly ILogger<NetworkDeviceService> _logger;
        private readonly HttpClient _httpClient;

        public NetworkDeviceService(ILogger<NetworkDeviceService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<DeviceInfo> GetDeviceInfoAsync(NetworkDevice device)
        {
            var info = new DeviceInfo();

            try
            {
                var url = $"{device.ConnectionUrl}/api/about";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    // Parse Enigma2 about response
                    var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("model", out var model))
                        info.Model = model.GetString() ?? "";
                    if (root.TryGetProperty("version", out var version))
                        info.FirmwareVersion = version.GetString() ?? "";
                    if (root.TryGetProperty("kernel", out var kernel))
                        info.KernelVersion = kernel.GetString() ?? "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get device info for {Device}", device.Name);
            }

            return info;
        }

        public async Task<List<ChannelGroup>> GetBouquetsAsync(NetworkDevice device)
        {
            var bouquets = new List<ChannelGroup>();

            try
            {
                var url = $"{device.ConnectionUrl}/api/bouquets";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    // Parse bouquets
                    var doc = JsonDocument.Parse(json);

                    // This would parse the actual Enigma2 bouquet format
                    // Implementation depends on the specific API response format
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get bouquets for {Device}", device.Name);
            }

            return bouquets;
        }

        public async Task<List<IChannel>> GetChannelsAsync(NetworkDevice device, string bouquetRef)
        {
            var channels = new List<IChannel>();

            try
            {
                var url = $"{device.ConnectionUrl}/api/services?bouquet={Uri.EscapeDataString(bouquetRef)}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    // Parse services
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get channels for {Device}", device.Name);
            }

            return channels;
        }

        public async Task<List<TransponderBase>> GetTranspondersAsync(NetworkDevice device)
        {
            var transponders = new List<TransponderBase>();

            try
            {
                var url = $"{device.ConnectionUrl}/api/transponders";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    // Parse transponders
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get transponders for {Device}", device.Name);
            }

            return transponders;
        }

        public async Task<List<Satellite>> GetSatellitesAsync(NetworkDevice device)
        {
            var satellites = new List<Satellite>();

            try
            {
                var url = $"{device.ConnectionUrl}/api/satellites";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    // Parse satellites
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get satellites for {Device}", device.Name);
            }

            return satellites;
        }

        public async Task<bool> TestConnectionAsync(NetworkDevice device)
        {
            try
            {
                var url = $"{device.ConnectionUrl}/api/about";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendChannelListAsync(NetworkDevice device, ChannelRepository repository)
        {
            try
            {
                // Convert repository to Enigma2 format and send
                var url = $"{device.ConnectionUrl}/api/services";
                var content = new StringContent(JsonSerializer.Serialize(repository), System.Text.Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send channel list to {Device}", device.Name);
                throw;
            }
        }

        public async Task SendBouquetsAsync(NetworkDevice device, List<ChannelGroup> bouquets)
        {
            try
            {
                var url = $"{device.ConnectionUrl}/api/bouquets";
                var content = new StringContent(JsonSerializer.Serialize(bouquets), System.Text.Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send bouquets to {Device}", device.Name);
                throw;
            }
        }

        public async Task RebootAsync(NetworkDevice device)
        {
            try
            {
                var url = $"{device.ConnectionUrl}/api/powerstate?newstate=3"; // Reboot
                await _httpClient.GetAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reboot {Device}", device.Name);
                throw;
            }
        }

        public async Task RestartEnigmaAsync(NetworkDevice device)
        {
            try
            {
                var url = $"{device.ConnectionUrl}/api/powerstate?newstate=4"; // Restart Enigma2
                await _httpClient.GetAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart Enigma2 on {Device}", device.Name);
                throw;
            }
        }
    }
}