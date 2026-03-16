<#
.SYNOPSIS
    Publishes the AutoSettingUI Demo projects for various frameworks.
    
.DESCRIPTION
    This script builds and publishes the Demo projects. 
    Supported frameworks: Avalonia, Ursa, WPF.
    Options: Normal (Standard), AOT (Native AOT), Trimmed.

.PARAMETER Framework
    The UI framework to publish. Options: 'Avalonia', 'Ursa', 'WPF'. Default is 'Avalonia'.

.PARAMETER Mode
    The publish mode. Options: 'Normal', 'AOT', 'Trim'. Default is 'Normal'.

.EXAMPLE
    .\publish-demo.ps1 -Framework Avalonia -Mode AOT
#>

param (
    [ValidateSet("Avalonia", "Ursa", "WPF")]
    [string]$Framework = "Avalonia",

    [ValidateSet("Normal", "AOT", "Trim")]
    [string]$Mode = "Normal",

    [string]$Runtime = "win-x64",
    
    [string]$OutputDirectory = "publish"
)

$ErrorActionPreference = "Stop"

$ProjectMap = @{
    "Avalonia" = "Demo\AutoSettingUI.Avalonia.Demo\AutoSettingUI.Avalonia.Demo.csproj"
    "Ursa"     = "Demo\AutoSettingUI.Ursa.Demo\AutoSettingUI.Ursa.Demo.csproj"
    "WPF"      = "Demo\AutoSettingUI.Wpf.Demo\AutoSettingUI.Wpf.Demo.csproj"
}

$ProjectFile = $ProjectMap[$Framework]
if (-not (Test-Path $ProjectFile)) {
    Write-Error "Project file not found: $ProjectFile"
}

$PublishDir = "$OutputDirectory\$Framework`_$Mode"
Write-Host "--- Publishing $Framework Demo in $Mode mode ---" -ForegroundColor Cyan
Write-Host "Target: $ProjectFile"
Write-Host "Output: $PublishDir"

$Args = @("publish", $ProjectFile, "-c", "Release", "-r", $Runtime, "-o", $PublishDir, "--self-contained", "true")

# Mode specific arguments
if ($Mode -eq "AOT") {
    if ($Framework -eq "WPF") {
        Write-Host "`nError: WPF does not support Native AOT. Please use 'Normal' or 'Trim' mode for WPF." -ForegroundColor Red
        exit 1
    }
    Write-Host "AOT Mode Enabled. Requirements:" -ForegroundColor Yellow
    Write-Host "1. Visual Studio Build Tools with 'Desktop development with C++' workload installed."
    Write-Host "2. Target project must be net8.0/net9.0 (not -windows suffix for AOT support)."
    
    $Args += "/p:PublishAot=true"
}
elseif ($Mode -eq "Trim") {
    $Args += "/p:PublishTrimmed=true"
}

# Run the command
Write-Host "Executing: dotnet $($Args -join ' ')" -ForegroundColor Gray
dotnet $Args

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nPublish Successful!" -ForegroundColor Green
    Write-Host "Location: $(Resolve-Path $PublishDir)"
}
else {
    Write-Host "`nPublish Failed!" -ForegroundColor Red
    exit 1
}
