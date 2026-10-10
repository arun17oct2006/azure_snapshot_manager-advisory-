# Builds a self-contained, single-file Windows executable of the offline
# desktop tool and stages it (with the launcher .bat) into ./desktop/dist.
#
# Usage (from the repo root):
#   .\desktop\publish-exe.ps1
#
# Requires: .NET 8 SDK, Windows (targets win-x64; change -r for other OSes,
# e.g. linux-x64 or osx-arm64, and swap the .bat for a .sh launcher).

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $PSScriptRoot "dist"

Write-Host "Publishing self-contained single-file exe (win-x64)..." -ForegroundColor Cyan

dotnet publish "$root\src\AzureSnapshotManager.Api\AzureSnapshotManager.Api.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $outDir

Copy-Item "$PSScriptRoot\Run Azure Snapshot Manager.bat" -Destination $outDir -Force

Write-Host ""
Write-Host "Done. Distributable folder: $outDir" -ForegroundColor Green
Write-Host "Zip that folder and share it, or attach it to a GitHub Release." -ForegroundColor Green
Write-Host "End users: unzip, then double-click 'Run Azure Snapshot Manager.bat'." -ForegroundColor Green
