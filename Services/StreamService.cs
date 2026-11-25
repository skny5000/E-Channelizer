using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using EChannelizer.Models;
using Microsoft.Extensions.Logging;

namespace EChannelizer.Services
{
    /// <summary>
    /// Stream service for playing channels
    /// </summary>
    public class StreamService : IStreamService
    {
        private readonly ILogger<StreamService> _logger;
        private Process? _playerProcess;
        private IChannel? _currentChannel;

        public StreamService(ILogger<StreamService> logger)
        {
            _logger = logger;
        }

        public bool IsPlaying => _playerProcess != null && !_playerProcess.HasExited;
        public PlayerStatus Status { get; private set; } = PlayerStatus.Stopped;

        public event EventHandler<StreamEventArgs>? StreamStatusChanged;

        public async Task PlayAsync(IChannel channel)
        {
            await StopAsync();

            _currentChannel = channel;
            Status = PlayerStatus.Connecting;
            OnStreamStatusChanged($"Connecting to {channel.Name}...");

            try
            {
                var streamInfo = await GetStreamInfoAsync(channel);
                if (string.IsNullOrEmpty(streamInfo.Url))
                {
                    throw new InvalidOperationException("No stream URL available");
                }

                Status = PlayerStatus.Buffering;
                OnStreamStatusChanged("Buffering...");

                await LaunchPlayerAsync(streamInfo);

                Status = PlayerStatus.Playing;
                OnStreamStatusChanged($"Playing: {channel.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to play channel {Channel}", channel.Name);
                Status = PlayerStatus.Error;
                OnStreamStatusChanged($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (_playerProcess != null && !_playerProcess.HasExited)
            {
                try
                {
                    _playerProcess.Kill(true);
                    await _playerProcess.WaitForExitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error stopping player");
                }
            }

            _playerProcess = null;
            _currentChannel = null;

            if (Status != PlayerStatus.Stopped)
            {
                Status = PlayerStatus.Stopped;
                OnStreamStatusChanged("Stopped");
            }
        }

        public async Task PauseAsync()
        {
            // Most command-line players don't support pause via API
            // Would need player-specific implementation
            await Task.CompletedTask;
        }

        public async Task ResumeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task<StreamInfo> GetStreamInfoAsync(IChannel channel)
        {
            var info = new StreamInfo();

            switch (channel)
            {
                case IptvChannel iptv:
                    info.Url = iptv.Url;
                    info.Type = iptv.StreamType;
                    break;

                case DvbSChannel dvbs:
                    // For DVB channels, would need to construct streaming URL
                    // This depends on the streaming server (TVHeadend, VDR, Enigma2, etc.)
                    info.Url = BuildDvbStreamUrl(channel);
                    info.Type = StreamTsType.TS;
                    break;

                case DvbCChannel dvbc:
                    info.Url = BuildDvbStreamUrl(channel);
                    info.Type = StreamTsType.TS;
                    break;

                case DvbTChannel dvbt:
                    info.Url = BuildDvbStreamUrl(channel);
                    info.Type = StreamTsType.TS;
                    break;

                default:
                    throw new NotSupportedException($"Channel type {channel.GetType().Name} not supported for streaming");
            }

            return info;
        }

        private string BuildDvbStreamUrl(IChannel channel)
        {
            // This would construct a streaming URL based on the device/profile settings
            // For example: http://device:port/stream/channelnumber
            // or http://tvheadend:9981/stream/serviceid
            return string.Empty;
        }

        private async Task LaunchPlayerAsync(StreamInfo streamInfo)
        {
            var profile = ProfileService.GetCurrentProfile(); // Would need to inject this
            var playerPath = profile?.Settings.PlayerPath ?? GetDefaultPlayerPath();
            var playerArgs = profile?.Settings.PlayerArguments ?? GetDefaultPlayerArgs(streamInfo);

            if (string.IsNullOrEmpty(playerPath) || !File.Exists(playerPath))
            {
                // Try to find a suitable player
                playerPath = FindPlayer();
                if (string.IsNullOrEmpty(playerPath))
                {
                    throw new InvalidOperationException("No suitable media player found. Please configure one in settings.");
                }
            }

            var finalArgs = playerArgs
                .Replace("{url}", streamInfo.Url)
                .Replace("{title}", _currentChannel?.Name ?? "Stream");

            _logger.LogInformation("Launching player: {Player} {Args}", playerPath, finalArgs);

            var startInfo = new ProcessStartInfo
            {
                FileName = playerPath,
                Arguments = finalArgs,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _playerProcess = Process.Start(startInfo);
            if (_playerProcess == null)
            {
                throw new InvalidOperationException("Failed to start media player");
            }

            // Monitor process exit
            _ = Task.Run(async () =>
            {
                await _playerProcess!.WaitForExitAsync();
                if (Status == PlayerStatus.Playing || Status == PlayerStatus.Buffering)
                {
                    Status = PlayerStatus.Stopped;
                    OnStreamStatusChanged("Playback ended");
                }
            });
        }

        private string GetDefaultPlayerPath()
        {
            // Check common player locations
            var paths = new[]
            {
                @"C:\Program Files\VideoLAN\VLC\vlc.exe",
                @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe",
                @"C:\Program Files\MPV\mpv.exe",
                @"C:\Program Files (x86)\MPV\mpv.exe",
                @"C:\Program Files\MPC-HC\mpc-hc64.exe",
                @"C:\Program Files (x86)\MPC-HC\mpc-hc64.exe"
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return path;
            }

            return string.Empty;
        }

        private string GetDefaultPlayerArgs(StreamInfo streamInfo)
        {
            var playerPath = GetDefaultPlayerPath();
            var fileName = Path.GetFileName(playerPath).ToLowerInvariant();

            return fileName switch
            {
                "vlc.exe" => "--play-and-exit \"{url}\"",
                "mpv.exe" => "\"{url}\"",
                "mpc-hc64.exe" => "\"{url}\" /play /close",
                _ => "\"{url}\""
            };
        }

        private string FindPlayer()
        {
            // Check PATH
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                var paths = pathEnv.Split(Path.PathSeparator);
                var players = new[] { "vlc", "mpv", "mpc-hc64", "mpc-hc" };

                foreach (var dir in paths)
                {
                    foreach (var player in players)
                    {
                        var fullPath = Path.Combine(dir, player + ".exe");
                        if (File.Exists(fullPath))
                            return fullPath;
                    }
                }
            }

            return GetDefaultPlayerPath();
        }

        private void OnStreamStatusChanged(string message)
        {
            StreamStatusChanged?.Invoke(this, new StreamEventArgs
            {
                Status = Status,
                Message = message,
                Channel = _currentChannel
            });
        }
    }

    /// <summary>
    /// Helper to access current profile (would be properly injected in real implementation)
    /// </summary>
    internal static class ProfileService
    {
        public static Profile? GetCurrentProfile()
        {
            // This would be implemented with proper DI
            return null;
        }
    }
}