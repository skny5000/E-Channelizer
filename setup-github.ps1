<#
.SYNOPSIS
    Automates GitHub repository setup for E-Channelizer
    Run this ONCE after creating a GitHub repository
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName = "E-Channelizer",

    [Parameter(Mandatory=$true)]
    [string]$GitHubUsername,

    [string]$Description = "DVB Channel Editor for Satellite, Cable and Terrestrial",

    [switch]$Private
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  E-Channelizer GitHub Setup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# التحقق من وجود Git
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] Git is not installed. Please install Git first." -ForegroundColor Red
    Write-Host "Download: https://git-scm.com/download/win" -ForegroundColor Yellow
    exit 1
}

# التحقق من وجود GitHub CLI (اختياري لكن مفيد)
$hasGh = Get-Command gh -ErrorAction SilentlyContinue

$projectDir = "E-Channelizer-Clone"
if (-not (Test-Path $projectDir)) {
    Write-Host "[ERROR] Project directory not found: $projectDir" -ForegroundColor Red
    exit 1
}

Set-Location $projectDir

Write-Host "[1/6] Initializing Git repository..." -ForegroundColor Yellow
if (-not (Test-Path ".git")) {
    git init
    Write-Host "  Git initialized" -ForegroundColor Green
} else {
    Write-Host "  Git already initialized" -ForegroundColor Gray
}

Write-Host "[2/6] Creating .gitignore..." -ForegroundColor Yellow
$gitignore = @"
# Build outputs
bin/
obj/
publish/
installer-output/

# IDE
.vs/
.idea/
*.user
*.suo

# OS
Thumbs.db
Desktop.ini

# Logs
*.log
Logs/

# Local data
/localappdata/
/userappdata/

# NuGet
packages/
*.nupkg

# Temporary files
*.tmp
*.temp
"@
$gitignore | Out-File -FilePath ".gitignore" -Encoding utf8
Write-Host "  .gitignore created" -ForegroundColor Green

Write-Host "[3/6] Adding files..." -ForegroundColor Yellow
git add .
Write-Host "  Files staged" -ForegroundColor Green

Write-Host "[4/6] Creating initial commit..." -ForegroundColor Yellow
git commit -m "Initial commit: E-Channelizer v1.0.0

- Complete WPF application with MahApps.Metro
- DVB-S/S2, DVB-C/C2, DVB-T/T2, ATSC support
- Channel editing, bouquets, EPG, logos
- Network device integration (Enigma2, TVHeadend)
- M3U import/export, XMLTV EPG
- Self-contained executable support
- Inno Setup installer
- GitHub Actions CI/CD"
Write-Host "  Initial commit created" -ForegroundColor Green

Write-Host "[5/6] Setting up GitHub remote..." -ForegroundColor Yellow
$repoUrl = "https://github.com/$GitHubUsername/$RepoName.git"
try {
    git remote add origin $repoUrl
    Write-Host "  Remote added: $repoUrl" -ForegroundColor Green
} catch {
    Write-Host "  Remote may already exist" -ForegroundColor Yellow
}

Write-Host "[6/6] Pushing to GitHub..." -ForegroundColor Yellow
try {
    git branch -M main
    git push -u origin main
    Write-Host "  Pushed to GitHub successfully!" -ForegroundColor Green
} catch {
    Write-Host "  Push failed. You may need to create the repo first on GitHub." -ForegroundColor Red
    Write-Host ""
    Write-Host "  Create repository at: https://github.com/new" -ForegroundColor Cyan
    Write-Host "  Repository name: $RepoName" -ForegroundColor Cyan
    Write-Host "  Description: $Description" -ForegroundColor Cyan
    Write-Host "  Private: $($Private.ToString())" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Then run this script again." -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  GITHUB SETUP COMPLETE!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Repository: https://github.com/$GitHubUsername/$RepoName" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Go to: https://github.com/$GitHubUsername/$RepoName/actions" -ForegroundColor White
Write-Host "  2. Enable Actions if prompted" -ForegroundColor White
Write-Host "  3. Create a release tag to trigger build:" -ForegroundColor White
Write-Host "     git tag v1.0.0 && git push origin v1.0.0" -ForegroundColor Yellow
Write-Host "  4. Download built files from Releases page" -ForegroundColor White
Write-Host ""
Write-Host "The workflow will automatically build on every push!" -ForegroundColor Green