<#
.SYNOPSIS
    AutoSettingUI Build and Pack Script
.DESCRIPTION
    Build and pack AutoSettingUI NuGet packages with user-selectable options.
.EXAMPLE
    .\build.ps1
    .\build.ps1 -Configuration Release -Version 1.0.1
#>

param(
    [string]$Configuration = "Release",
    [string]$Version = "",
    [switch]$SkipBuild,
    [switch]$Help
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$OutputDir = Join-Path $ScriptDir "artifacts"

$Packages = @(
    @{ Name = "AutoSettingUI.Core"; Project = "src\AutoSettingUI.Core\AutoSettingUI.Core.csproj" },
    @{ Name = "AutoSettingUI.Generator"; Project = "src\AutoSettingUI.Generator\AutoSettingUI.Generator.csproj" },
    @{ Name = "AutoSettingUI.Extension.Shared"; Project = "AutoSettingUI.Extension.Shared\AutoSettingUI.Extension.Shared.csproj" },
    @{ Name = "AutoSettingUI.Avalonia"; Project = "src\Extensions\AutoSettingUI.Avalonia\AutoSettingUI.Avalonia.csproj" },
    @{ Name = "AutoSettingUI.Ursa"; Project = "src\Extensions\AutoSettingUI.Ursa\AutoSettingUI.Ursa.csproj" },
    @{ Name = "AutoSettingUI.WPF"; Project = "src\Extensions\AutoSettingUI.WPF\AutoSettingUI.WPF.csproj" }
)

function Show-Help {
    Write-Host @"
AutoSettingUI Build Script

Usage: .\build.ps1 [options]

Options:
    -Configuration <config>  Build configuration (Debug/Release). Default: Release
    -Version <version>       Override package version (e.g., 1.0.1)
    -SkipBuild               Skip build step, only pack
    -Help                    Show this help message

Examples:
    .\build.ps1
    .\build.ps1 -Configuration Debug
    .\build.ps1 -Version 1.2.0
    .\build.ps1 -SkipBuild -Version 1.0.1-beta
"@
}

function Show-Menu {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  AutoSettingUI Build & Pack Script" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Available packages:" -ForegroundColor Yellow
    Write-Host ""
    
    for ($i = 0; $i -lt $Packages.Count; $i++) {
        Write-Host "  [$($i + 1)] $($Packages[$i].Name)" -ForegroundColor White
    }
    
    Write-Host ""
    Write-Host "  [A] Build ALL packages" -ForegroundColor Green
    Write-Host "  [Q] Quit" -ForegroundColor Red
    Write-Host ""
}

function Get-UserSelection {
    Show-Menu
    
    $selection = Read-Host "Enter your choice (1-$($Packages.Count), A, or Q)"
    
    switch -Regex ($selection.ToUpper()) {
        "^[1-$($Packages.Count)]$" {
            $index = [int]$selection - 1
            return @($Packages[$index])
        }
        "^A$" {
            return $Packages
        }
        "^Q$" {
            Write-Host "Exiting..." -ForegroundColor Yellow
            exit 0
        }
        default {
            Write-Host "Invalid selection. Please try again." -ForegroundColor Red
            return Get-UserSelection
        }
    }
}

function Invoke-Build {
    param([array]$SelectedPackages)
    
    Write-Host ""
    Write-Host "Building projects..." -ForegroundColor Cyan
    Write-Host "Configuration: $Configuration" -ForegroundColor Gray
    
    foreach ($pkg in $SelectedPackages) {
        $projectPath = Join-Path $ScriptDir $pkg.Project
        
        if (-not (Test-Path $projectPath)) {
            Write-Host "Project not found: $projectPath" -ForegroundColor Red
            continue
        }
        
        Write-Host "  Building $($pkg.Name)..." -ForegroundColor Yellow
        
        $buildArgs = @("build", $projectPath, "-c", $Configuration)
        
        if ($Version) {
            $buildArgs += "/p:Version=$Version"
        }
        
        & dotnet $buildArgs
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to build $($pkg.Name)" -ForegroundColor Red
            throw "Build failed for $($pkg.Name)"
        }
    }
    
    Write-Host "Build completed successfully!" -ForegroundColor Green
}

function Invoke-Pack {
    param([array]$SelectedPackages)
    
    if (Test-Path $OutputDir) {
        Remove-Item -Path "$OutputDir\*.nupkg" -Force -ErrorAction SilentlyContinue
    } else {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }
    
    Write-Host ""
    Write-Host "Packing NuGet packages..." -ForegroundColor Cyan
    Write-Host "Output directory: $OutputDir" -ForegroundColor Gray
    
    foreach ($pkg in $SelectedPackages) {
        $projectPath = Join-Path $ScriptDir $pkg.Project
        
        if (-not (Test-Path $projectPath)) {
            continue
        }
        
        Write-Host "  Packing $($pkg.Name)..." -ForegroundColor Yellow
        
        $packArgs = @("pack", $projectPath, "-c", $Configuration, "-o", $OutputDir)
        
        if ($Version) {
            $packArgs += "/p:Version=$Version"
        }
        
        & dotnet $packArgs
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to pack $($pkg.Name)" -ForegroundColor Red
            throw "Pack failed for $($pkg.Name)"
        }
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  Pack completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Packages created in: $OutputDir" -ForegroundColor Cyan
    
    $nupkgFiles = Get-ChildItem -Path $OutputDir -Filter "*.nupkg"
    foreach ($file in $nupkgFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor White
    }
}

if ($Help) {
    Show-Help
    exit 0
}

try {
    Write-Host "AutoSettingUI Build Script" -ForegroundColor Cyan
    Write-Host "Configuration: $Configuration" -ForegroundColor Gray
    if ($Version) {
        Write-Host "Version override: $Version" -ForegroundColor Gray
    }
    
    $selectedPackages = Get-UserSelection
    
    if (-not $SkipBuild) {
        Invoke-Build -SelectedPackages $selectedPackages
    }
    
    Invoke-Pack -SelectedPackages $selectedPackages
    
} catch {
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}
