@echo off
chcp 65001 >nul
echo ============================================
echo  E-Channelizer Build Script for Windows
echo ============================================
echo.

REM Check if dotnet is installed
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK is not installed or not in PATH
    echo.
    echo Please install .NET 8 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo [INFO] .NET SDK found:
dotnet --version

echo.
echo [STEP 1] Restoring NuGet packages...
dotnet restore EChannelizer.csproj
if %errorlevel% neq 0 (
    echo [ERROR] Failed to restore packages
    pause
    exit /b 1
)

echo.
echo [STEP 2] Building Release configuration...
dotnet build EChannelizer.csproj -c Release --no-restore
if %errorlevel% neq 0 (
    echo [ERROR] Build failed
    pause
    exit /b 1
)

echo.
echo [STEP 3] Publishing as self-contained executable...
dotnet publish EChannelizer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:DebugType=None -p:DebugSymbols=false -o publish\win-x64
if %errorlevel% neq 0 (
    echo [ERROR] Publish failed
    pause
    exit /b 1
)

echo.
echo [STEP 4] Creating portable version (framework-dependent)...
dotnet publish EChannelizer.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishTrimmed=false -p:DebugType=None -p:DebugSymbols=false -o publish\win-x64-portable
if %errorlevel% neq 0 (
    echo [ERROR] Portable publish failed
    pause
    exit /b 1
)

echo.
echo ============================================
echo  BUILD COMPLETE!
echo ============================================
echo.
echo Self-contained executable (no .NET required on target):
echo   .\publish\win-x64\E-Channelizer.exe
echo.
echo Portable version (requires .NET 8 Runtime on target):
echo   .\publish\win-x64-portable\E-Channelizer.exe
echo.
echo File sizes:
dir publish\win-x64\E-Channelizer.exe
dir publish\win-x64-portable\E-Channelizer.exe
echo.
echo To run: Double-click E-Channelizer.exe in either folder
echo.

pause