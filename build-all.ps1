<#
.SYNOPSIS
    Builds the entire AutoSettingUI solution.
#>

$ErrorActionPreference = "Continue"

Write-Host "--- Building AutoSettingUI Solution ---" -ForegroundColor Cyan

dotnet build AutoSettingUI.sln -c Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nSolution Build Successful!" -ForegroundColor Green
}
else {
    Write-Host "`nSolution Build Failed!" -ForegroundColor Red
    exit 1
}
