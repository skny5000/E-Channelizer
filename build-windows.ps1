<#
.SYNOPSIS
    Build script for E-Channelizer - Creates Windows executable (.exe)

.DESCRIPTION
    This script builds the E-Channelizer WPF application and publishes it as
    a single-file executable for Windows x64. It creates two versions:
    1. Self-contained (no .NET runtime required on target machine)
    2. Framework-dependent (requires .NET 8 Runtime on target machine)

.PARAMETER Configuration
    Build configuration: Debug or Release (default: Release)

.PARAMETER Runtime
    Target runtime: win-x64, win-x86, win-arm64 (default: win-x64)

.PARAMETER OutputPath
    Custom output directory (default: ./publish)

.EXAMPLE
    .\build-windows.ps1
    # Builds Release for win-x64 to ./publish

.EXAMPLE
    .\build-windows.ps1 -Configuration Debug -Runtime win-x86
    # Builds Debug for win-x86

.NOTES
    Requires .NET 8 SDK installed
    Run from the project directory (where EChannelizer.csproj is located)
#>

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [ValidateSet('win-x64', 'win-x86', 'win-arm64')]
    [string]$Runtime = 'win-x64',

    [string]$OutputPath = 'publish'
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  E-Channelizer Build Script for Windows" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "[INFO] .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] .NET SDK is not installed or not in PATH" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install .NET 8 SDK from:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

$projectFile = "EChannelizer.csproj"
if (-not (Test-Path $projectFile)) {
    Write-Host "[ERROR] Project file not found: $projectFile" -ForegroundColor Red
    Write-Host "Run this script from the project directory" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "[STEP 1] Restoring NuGet packages..." -ForegroundColor Yellow
$result = dotnet restore $projectFile
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to restore packages" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "[OK] Packages restored" -ForegroundColor Green

Write-Host ""
Write-Host "[STEP 2] Building $Configuration configuration..." -ForegroundColor Yellow
$result = dotnet build $projectFile -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "[OK] Build successful" -ForegroundColor Green

Write-Host ""
Write-Host "[STEP 3] Publishing self-contained executable..." -ForegroundColor Yellow
$selfContainedPath = Join-Path $OutputPath "$Runtime-selfcontained"
$result = dotnet publish $projectFile -c $Configuration -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $selfContainedPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Self-contained publish failed" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "[OK] Self-contained executable created" -ForegroundColor Green

Write-Host ""
Write-Host "[STEP 4] Publishing framework-dependent executable..." -ForegroundColor Yellow
$frameworkDependentPath = Join-Path $OutputPath "$Runtime-portable"
$result = dotnet publish $projectFile -c $Configuration -r $Runtime `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $frameworkDependentPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Framework-dependent publish failed" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "[OK] Framework-dependent executable created" -ForegroundColor Green

# Show results
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  BUILD COMPLETE!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$selfContainedExe = Join-Path $selfContainedPath "E-Channelizer.exe"
$portableExe = Join-Path $frameworkDependentPath "E-Channelizer.exe"

if (Test-Path $selfContainedExe) {
    $size = (Get-Item $selfContainedExe).Length
    $sizeMB = [math]::Round($size / 1MB, 1)
    Write-Host "Self-contained executable (no .NET required on target):" -ForegroundColor Green
    Write-Host "  $selfContainedExe" -ForegroundColor White
    Write-Host "  Size: $sizeMB MB" -ForegroundColor Gray
}

if (Test-Path $portableExe) {
    $size = (Get-Item $portableExe).Length
    $sizeMB = [math]::Round($size / 1MB, 1)
    Write-Host ""
    Write-Host "Framework-dependent executable (requires .NET 8 Runtime):" -ForegroundColor Green
    Write-Host "  $portableExe" -ForegroundColor White
    Write-Host "  Size: $sizeMB MB" -ForegroundColor Gray
}

Write-Host ""
Write-Host "To run: Double-click E-Channelizer.exe in either folder" -ForegroundColor Cyan
Write-Host ""