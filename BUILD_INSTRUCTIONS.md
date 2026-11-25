# E-Channelizer - Build Instructions for Windows

This document explains how to build the E-Channelizer WPF application into a standalone Windows executable (`.exe`).

## Prerequisites

1. **Windows 10/11** (x64)
2. **.NET 8 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **Visual Studio 2022** (optional, for development) with "Desktop development with .NET" workload

## Quick Build (Command Line)

### Option 1: Batch Script (Easiest)
```cmd
# Navigate to project folder
cd E-Channelizer-Clone

# Run build script
build-windows.bat
```

### Option 2: PowerShell Script (More Features)
```powershell
# Navigate to project folder
cd E-Channelizer-Clone

# Run build script (default: Release, win-x64)
.\build-windows.ps1

# Or with custom options
.\build-windows.ps1 -Configuration Release -Runtime win-x64
```

## Build Outputs

After successful build, you'll find two versions in the `publish` folder:

### 1. Self-Contained Executable (Recommended)
**Location:** `publish\win-x64-selfcontained\E-Channelizer.exe`
- **Size:** ~70-90 MB
- **Requires:** Nothing! Runs on any Windows 10/11 x64 machine without .NET installed
- **Best for:** Distribution to end users

### 2. Framework-Dependent Executable (Smaller)
**Location:** `publish\win-x64-portable\E-Channelizer.exe`
- **Size:** ~15-25 MB
- **Requires:** .NET 8 Desktop Runtime on target machine
- **Best for:** Environments where .NET Runtime is already installed

## Build via Visual Studio

1. Open `EChannelizer.csproj` in Visual Studio 2022
2. Wait for NuGet packages to restore
3. Select **Release** configuration and **x64** platform
4. Right-click project → **Publish**
5. Choose **Folder** as target
6. Set configuration:
   - **Deployment Mode:** Self-contained
   - **Target Runtime:** win-x64
   - **Produce single file:** ✓ Enabled
   - **Enable ReadyToRun:** ✓ Enabled
   - **Trim unused assemblies:** ✗ Disabled (WPF compatibility)
7. Click **Publish**

## Build via Command Line (Manual)

```cmd
# Restore packages
dotnet restore EChannelizer.csproj

# Build
dotnet build EChannelizer.csproj -c Release

# Publish self-contained (single .exe, no .NET needed)
dotnet publish EChannelizer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:DebugType=None -o publish\win-x64

# Publish framework-dependent (smaller, needs .NET Runtime)
dotnet publish EChannelizer.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishTrimmed=false -p:DebugType=None -o publish\win-x64-portable
```

## Distribution Checklist

Before distributing the executable:

- [ ] Test on clean Windows machine (no Visual Studio, no .NET SDK)
- [ ] Verify all features work (import, export, EPG, logos, streaming)
- [ ] Check Windows SmartScreen reputation (may need code signing)
- [ ] Create installer if needed (Inno Setup, WiX, NSIS)
- [ ] Include `NLog.config` and `appsettings.json` if customized

## Code Signing (Recommended for Distribution)

To avoid Windows SmartScreen warnings:

```cmd
# Sign with certificate (requires code signing certificate)
signtool sign /f certificate.pfx /p password /tr http://timestamp.digicert.com /td sha256 /fd sha256 E-Channelizer.exe
```

## Troubleshooting

### "The application could not start correctly (0xc000007b)"
- Missing Visual C++ Redistributable
- Install: [Microsoft Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe)

### "Could not load file or assembly 'PresentationFramework'"
- Target machine needs .NET Desktop Runtime
- Use self-contained build instead

### Application crashes on startup
- Check Windows Event Viewer → Application logs
- Enable logging in `NLog.config` to debug

### Large executable size
- Self-contained includes entire .NET Runtime (~70MB base)
- This is normal for single-file WPF apps
- Framework-dependent is smaller but requires .NET installed

## Folder Structure After Build

```
E-Channelizer-Clone/
├── publish/
│   ├── win-x64-selfcontained/
│   │   ├── E-Channelizer.exe          <-- MAIN EXECUTABLE (distribute this)
│   │   ├── E-Channelizer.pdb          (debug symbols, optional)
│   │   └── appsettings.json           (if copied)
│   └── win-x64-portable/
│       ├── E-Channelizer.exe          <-- SMALLER EXECUTABLE
│       └── ...
├── EChannelizer.csproj
├── build-windows.bat
├── build-windows.ps1
└── BUILD_INSTRUCTIONS.md
```

## Runtime Data Locations

The application stores data in these locations (per Windows standards):

| Data Type | Location |
|-----------|----------|
| User Profiles | `%LOCALAPPDATA%\E-Channelizer\Profiles\` |
| Logs | `%LOCALAPPDATA%\E-Channelizer\Logs\` |
| Channel Logos | `%LOCALAPPDATA%\E-Channelizer\Logos\` |
| Settings | `%APPDATA%\E-Channelizer\active_profile.json` |

These folders are created automatically on first run.

## Support

For build issues:
1. Verify .NET 8 SDK is installed: `dotnet --version` (should show 8.x.x)
2. Check Windows version: Windows 10 1809+ or Windows 11
3. Ensure x64 architecture (ARM64 requires separate build)

---

**E-Channelizer** - DVB Channel Editor for Satellite, Cable and Terrestrial
Built with .NET 8, WPF, and MahApps.Metro