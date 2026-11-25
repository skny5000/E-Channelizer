# E-Channelizer - DVB Channel Editor

**Professional DVB Channel Editor for Satellite, Cable and Terrestrial TV**

![E-Channelizer](https://img.shields.io/badge/.NET-8.0-blue) ![WPF](https://img.shields.io/badge/WPF-Desktop-purple) ![License](https://img.shields.io/badge/License-MIT-green)

---

## ✨ Features

### 📺 Channel Management
- **Multi-standard support**: DVB-S/S2, DVB-C/C2, DVB-T/T2, ATSC, ISDB-T
- **Channel editing**: Name, number, frequency, polarization, symbol rate, FEC, modulation
- **Service flags**: Scrambled, HD, UHD, HEVC, favorites, locked, hidden, skip
- **Multiple audio/subtitle tracks**: AC3, E-AC3, AAC, Teletext, DVB Subtitles
- **Channel markers**: Color-coded markers (5 custom + favorites/locked/hidden/skip)

### 📦 Bouquets & Groups
- **Hierarchical groups**: Nested bouquets with drag & drop
- **Auto-create bouquets**: By satellite, provider, type (TV/Radio), favorites
- **Multiple sources**: User, Provider, Satellite bouquets
- **Import/Export**: M3U playlists, Enigma2/VDR bouquets

### 🛰 Satellite & Transponder Management
- **Satellite database**: 100+ pre-configured satellites with orbital positions
- **Online updates**: Download latest satellite lists (KingOfSat, FlySat, LyngSat)
- **Transponder editing**: Full DVB-S/C/T parameter editing
- **Signal parameters**: Frequency, polarization, symbol rate, FEC, modulation, rolloff, pilot

### 📡 Network Device Integration
- **Enigma2 support**: Dreambox, VU+, Gigablue, Octagon, etc.
- **TVHeadend support**: Via API
- **VDR support**: Via SVDRP
- **Operations**: Send channel lists, bouquets, transponders, reboot, restart

### 📅 EPG (Electronic Program Guide)
- **XMLTV support**: Import/export XMLTV format
- **Auto-mapping**: Smart channel-to-EPG mapping by service ID/name
- **Batch editing**: Bulk EPG assignments
- **Multiple sources**: DVB EIT, XMLTV, MHW, EPGDAT
- **Retention**: Configurable EPG data retention (default 7 days)

### 🎨 Logos & Visuals
- **Auto-download**: Fetch logos from online databases (tv-logo, LyngSat)
- **Local scanning**: Match logos from local folder
- **Format support**: PNG, JPG, BMP, WebP (auto-resize to 200KB)
- **Bulk operations**: Assign, export, cleanup unused logos

### 🌐 Streaming & Playback
- **External players**: VLC, MPV, MPC-HC, PotPlayer
- **Streamlink integration**: For HLS, DASH, RTMP, RTSP streams
- **Buffer configuration**: Low/Medium/High buffering modes
- **Direct URL playback**: HTTP, UDP, RTP, M3U8

### ⚙️ Profiles & Settings
- **Multiple profiles**: Separate configurations per user/device
- **Theme support**: Light/Dark/System with 7 accent colors
- **Language support**: 10+ languages (EN, DE, FR, ES, IT, NL, PL, TR, RU, AR)
- **Auto-save/backup**: Configurable repository management

---

## 📥 Download

### Pre-built Executables (Recommended)

| Version | Architecture | Size | Requirements |
|---------|-------------|------|--------------|
| **Self-contained** | x64 | ~85 MB | **None** - Runs on any Windows 10/11 |
| **Framework-dependent** | x64 | ~20 MB | Requires [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

> **Note**: The self-contained version is recommended for most users as it requires no additional installation.

---

## 🚀 Quick Start

1. **Download** `E-Channelizer.exe` (self-contained version)
2. **Run** - Double-click the executable (no installation needed)
3. **Create Repository** - File → New Repository → Choose location
4. **Import Data** - Tools → Import → Select satellites/channels/EPG
5. **Edit Channels** - Double-click any channel to edit
6. **Create Bouquets** - Tools → Create Bouquets → Auto-generate
7. **Export** - File → Export M3U or Send to Device

---

## 📋 System Requirements

| Requirement | Minimum |
|------------|---------|
| **OS** | Windows 10 (1809+) / Windows 11 (x64) |
| **RAM** | 4 GB (8 GB recommended for large channel lists) |
| **Storage** | 200 MB free space |
| **Display** | 1366×768 (1920×1080 recommended) |
| **.NET** | Included in self-contained version |

---

## 🗂 Data Locations

The application follows Windows standards for data storage:

| Data | Location |
|------|----------|
| **Repositories** | User-chosen location (`.ech` files) |
| **Profiles** | `%LOCALAPPDATA%\E-Channelizer\Profiles\` |
| **Logs** | `%LOCALAPPDATA%\E-Channelizer\Logs\` |
| **Logos** | `%LOCALAPPDATA%\E-Channelizer\Logos\` |
| **Active Profile** | `%APPDATA%\E-Channelizer\active_profile.json` |

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New Repository |
| `Ctrl+O` | Open Repository |
| `Ctrl+S` | Save Repository |
| `Ctrl+Shift+S` | Save Repository As |
| `Ctrl+I` | Import Data |
| `Ctrl+E` | Export M3U |
| `Del` | Delete Selected Channels |
| `Enter` | Edit Selected Channel |
| `Space` | Play Selected Channel |
| `F3` / `Ctrl+F` | Search Channels |
| `F5` | Refresh |

---

## 🔧 Building from Source

### Prerequisites
- Visual Studio 2022 with ".NET desktop development" workload
- .NET 8 SDK

### Build Commands
```bash
# Clone and build
git clone https://github.com/e-channelizer/e-channelizer.git
cd e-channelizer

# Restore packages
dotnet restore

# Build Release
dotnet build -c Release

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64
```

### Using Build Scripts
```cmd
# Windows Batch
build-windows.bat

# PowerShell (more options)
.\build-windows.ps1 -Configuration Release -Runtime win-x64
```

---

## 📁 Supported File Formats

### Import
- **E-Channelizer**: `.ech` (ZIP with JSON + logos)
- **Satellites**: XML (KingOfSat, FlySat), JSON
- **Channels/Transponders**: XML, JSON, CSV
- **EPG**: XMLTV (`.xml`), MHW (`.mhw`), EPGDAT (`.dat`)
- **Playlists**: M3U (`.m3u`, `.m3u8`)
- **Logos**: PNG, JPG, BMP, WebP (individual or ZIP)

### Export
- **E-Channelizer**: `.ech` (complete backup)
- **M3U Playlists**: `.m3u`, `.m3u8`
- **Satellites**: JSON, XML
- **EPG**: XMLTV
- **Logos**: Individual files

---

## 🛠 Troubleshooting

### Application won't start
- **Error 0xc000007b**: Install [Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe)
- **PresentationFramework error**: Use self-contained version or install .NET 8 Desktop Runtime
- **SmartScreen warning**: Click "More info" → "Run anyway" (or code-sign the exe)

### Channels not showing
- Check filters: Favorites Only / Scrambled Only / HD Only (toolbar)
- Verify repository loaded correctly (status bar shows channel count)
- Check group selection in left panel

### EPG not working
- Ensure EPG source configured (Tools → Batch Edit EPG)
- Run Auto-map to match XMLTV channels to your channels
- Check EPG update interval in Settings → EPG

### Streaming not working
- Configure player path in Settings → Streaming
- Install Streamlink for HLS/DASH streams
- Check stream URL accessibility in Edit Channel → Test Stream

### Network device connection failed
- Verify IP address, port, username/password
- Check device API is enabled (Enigma2: Web Interface plugin)
- Test with browser: `http://device-ip/api/about`

---

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **MahApps.Metro** - Modern WPF styling
- **MaterialDesign Icons** - Beautiful iconography
- **Newtonsoft.Json** - JSON serialization
- **CommunityToolkit.Mvvm** - MVVM framework
- **NLog** - Logging framework
- **CsvHelper** - CSV import/export
- **EPPlus** - Excel support

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/e-channelizer/e-channelizer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/e-channelizer/e-channelizer/discussions)
- **Wiki**: [Documentation](https://github.com/e-channelizer/e-channelizer/wiki)

---

**E-Channelizer** - Making DVB channel management simple and powerful! 📺✨