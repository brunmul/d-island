# D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
# Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.
# Part of the modified version "Atoll" by Bruno Muller; not an official D-Island release.

# One-shot dev test of notifications: registers the Atoll package identity for the Debug build (one
# UAC prompt for the signing certificate, the first time), stops an official D-Island if it runs, then
# starts Atoll. Windows asks once for notification access; accept, then trigger a notification and click
# the alert in the island.
#
# Usage: .\tools\test-notifications.ps1 [-Release]
param([switch]$Release)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$config = if ($Release) { 'Release' } else { 'Debug' }
$exeDir = Join-Path $root "bin\$config\net8.0-windows10.0.19041.0"
if (-not (Test-Path (Join-Path $exeDir 'DIsland.exe'))) { throw "Build first: dotnet build -c $config" }

$pkg = Get-AppxPackage -Name Atoll -ErrorAction SilentlyContinue
if (-not $pkg -or $pkg.InstallLocation.TrimEnd('\') -ne $exeDir.TrimEnd('\')) {
    Write-Host "Registering the Atoll identity for $exeDir (Windows may ask for permission once)..."
    & (Join-Path $root 'Identity\setup-identity.ps1') -ExeDir $exeDir
    if ($LASTEXITCODE -ne 0) { throw "Identity setup failed; see $env:LOCALAPPDATA\DIsland\Identity\setup.log" }
}
else { Write-Host "Atoll identity already registered for $exeDir" }

Get-Process DIsland -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 800
Start-Process (Join-Path $exeDir 'DIsland.exe')
Write-Host 'Atoll started. Accept the notification access prompt, trigger a notification, click the alert.'
