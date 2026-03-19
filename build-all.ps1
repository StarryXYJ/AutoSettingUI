<#
.SYNOPSIS
    Builds the entire AutoSettingUI solution and optionally packs NuGet packages.

.DESCRIPTION
    This script builds the solution in Debug or Release configuration.
    It can also pack NuGet packages for distribution.

.PARAMETER Configuration
    The build configuration. Options: 'Debug', 'Release'. Default is 'Debug'.

.PARAMETER Pack
    If specified, packs NuGet packages after building.

.PARAMETER OutputDirectory
    The output directory for NuGet packages. Default is 'artifacts'.

.EXAMPLE
    .\build-all.ps1 -Configuration Release -Pack
#>

param (
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [switch]$Pack,

    [string]$OutputDirectory = "artifacts"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Building AutoSettingUI Solution" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Build solution
dotnet build AutoSettingUI.sln -c $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nSolution Build Failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nSolution Build Successful!" -ForegroundColor Green

# Pack NuGet packages if requested
if ($Pack) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  Packing NuGet Packages" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    $Packages = @(
        "src\AutoSettingUI.Core\AutoSettingUI.Core.csproj",
        "src\AutoSettingUI.Generator\AutoSettingUI.Generator.csproj",
        "src\Extensions\AutoSettingUI.Extension.Shared\AutoSettingUI.Extension.Shared.csproj",
        "src\Extensions\AutoSettingUI.Avalonia\AutoSettingUI.Avalonia.csproj",
        "src\Extensions\AutoSettingUI.Ursa\AutoSettingUI.Ursa.csproj",
        "src\Extensions\AutoSettingUI.WPF\AutoSettingUI.WPF.csproj"
    )

    if (-not (Test-Path $OutputDirectory)) {
        New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
    }

    foreach ($Package in $Packages) {
        Write-Host "`nPacking: $Package" -ForegroundColor Yellow
        dotnet pack $Package -c $Configuration -o $OutputDirectory --no-build

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to pack: $Package" -ForegroundColor Red
            exit 1
        }
    }

    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  All Packages Packed Successfully!" -ForegroundColor Green
    Write-Host "  Location: $(Resolve-Path $OutputDirectory)" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
}
