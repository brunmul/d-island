# D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
# Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.
# Modified for Atoll (Bruno Muller's version of D-Island): own package name, tools from an installed
# Windows SDK when there is one, and a registration fallback. Not an official D-Island release.

# Gives DIsland.exe (Atoll) a package identity (needed for reading notifications and the Windows calendar)
# by registering a *sparse* package: only a manifest, whose "external location" is the folder holding the
# exe. Nothing is copied or installed besides that registration.
#
#   1. Finds makeappx/signtool: an installed Windows SDK (Windows Kits\10\bin\<version>\x64) first, else
#      Microsoft's Microsoft.Windows.SDK.BuildTools NuGet package (downloaded once, cached). On one PC the
#      NuGet makeappx failed with a side-by-side error while the Windows Kits one worked, hence the order.
#      -MakeAppx / -SignTool name the tools explicitly.
#   2. Creates a self-signed code-signing certificate "CN=Atoll Dev" in the current user's store.
#   3. Trusts it machine-wide (Local Machine\Trusted People): the ONLY step that needs admin, so it runs
#      in a separate elevated process and Windows shows one UAC prompt.
#   4. Packs and signs the manifest, and registers it with the exe's folder as external location. If
#      Add-AppxPackage refuses the .msix from %LocalAppData% (seen: 0x80073CF0 / 0x80070003), the same
#      file is registered from the user profile root instead.
#
# Run again after moving the exe to another folder. Undo: Get-AppxPackage Atoll | Remove-AppxPackage
# (and remove the certificate from certlm.msc > Trusted People, and certmgr.msc > Personal).
param(
    [string]$ExeDir = (Split-Path $PSScriptRoot -Parent),
    [switch]$Restart,
    [string]$MakeAppx,
    [string]$SignTool
)
$ErrorActionPreference = 'Stop'
$packageName = 'Atoll'
$publisher = 'CN=Atoll Dev'
$toolsVersion = '10.0.26100.1742'
$work = Join-Path $env:LOCALAPPDATA 'DIsland\Identity'
$log = Join-Path $work 'setup.log'
New-Item -ItemType Directory -Force $work | Out-Null
Start-Transcript -Path $log -Force | Out-Null

function Find-KitTool([string]$name) {
    $kits = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
    if (-not (Test-Path $kits)) { return $null }
    Get-ChildItem $kits -Directory | Where-Object { $_.Name -match '^10\.\d+\.\d+\.\d+$' } |
        Sort-Object { [version]$_.Name } -Descending |
        ForEach-Object { Join-Path $_.FullName "x64\$name" } | Where-Object { Test-Path $_ } | Select-Object -First 1
}

try {
    if (-not (Test-Path (Join-Path $ExeDir 'DIsland.exe'))) { throw "DIsland.exe not found in $ExeDir" }

    # ---- 1. packaging tools
    if (-not $MakeAppx) { $MakeAppx = Find-KitTool 'makeappx.exe' }
    if (-not $SignTool) { $SignTool = Find-KitTool 'signtool.exe' }
    if (-not $MakeAppx -or -not $SignTool) {
        $toolsDir = Join-Path $work "BuildTools.$toolsVersion"
        if (-not (Test-Path $toolsDir)) {
            Write-Host "Downloading Microsoft.Windows.SDK.BuildTools $toolsVersion..."
            $nupkg = Join-Path $work 'buildtools.zip'
            Invoke-WebRequest "https://api.nuget.org/v3-flatcontainer/microsoft.windows.sdk.buildtools/$toolsVersion/microsoft.windows.sdk.buildtools.$toolsVersion.nupkg" -OutFile $nupkg -UseBasicParsing
            Expand-Archive $nupkg $toolsDir -Force
            Remove-Item $nupkg
        }
        if (-not $MakeAppx) { $MakeAppx = Get-ChildItem $toolsDir -Recurse -Filter makeappx.exe | Where-Object FullName -like '*\x64\*' | Select-Object -First 1 -ExpandProperty FullName }
        if (-not $SignTool) { $SignTool = Get-ChildItem $toolsDir -Recurse -Filter signtool.exe | Where-Object FullName -like '*\x64\*' | Select-Object -First 1 -ExpandProperty FullName }
    }
    if (-not $MakeAppx -or -not $SignTool) { throw 'makeappx/signtool not found (Windows SDK or BuildTools package)' }
    Write-Host "Using $MakeAppx"

    # ---- 2. signing certificate (current user)
    $cert = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $publisher -and $_.NotAfter -gt (Get-Date).AddDays(30) } | Select-Object -First 1
    if (-not $cert) {
        Write-Host 'Creating the signing certificate...'
        $cert = New-SelfSignedCertificate -Type Custom -Subject $publisher -KeyUsage DigitalSignature `
            -FriendlyName 'Atoll package signing' -CertStoreLocation Cert:\CurrentUser\My `
            -NotAfter (Get-Date).AddYears(10) -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')
    }

    # ---- 3. trust it (elevated, once)
    $trusted = Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object Thumbprint -eq $cert.Thumbprint
    if (-not $trusted) {
        Write-Host 'Trusting the certificate (Windows will ask for permission)...'
        $cer = Join-Path $work 'Atoll.cer'
        Export-Certificate -Cert $cert -FilePath $cer | Out-Null
        $import = "Import-Certificate -FilePath '$cer' -CertStoreLocation Cert:\LocalMachine\TrustedPeople | Out-Null"
        $p = Start-Process powershell.exe -Verb RunAs -Wait -PassThru -WindowStyle Hidden `
            -ArgumentList '-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', $import
        if ($p.ExitCode -ne 0 -or -not (Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object Thumbprint -eq $cert.Thumbprint)) {
            throw 'The certificate was not trusted (permission declined?)'
        }
    }

    # ---- 4. pack, sign, register
    $stage = Join-Path $work 'package'
    Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force (Join-Path $stage 'Assets') | Out-Null
    Copy-Item (Join-Path $PSScriptRoot 'AppxManifest.xml') $stage

    # Logos: the same violet ring with an amber dot as the tray icon (tools\make-icon.ps1).
    Add-Type -AssemblyName System.Drawing
    foreach ($logo in @(@('StoreLogo.png', 50), @('Square44x44Logo.png', 44), @('Square150x150Logo.png', 150))) {
        $size = $logo[1]
        $bmp = New-Object System.Drawing.Bitmap $size, $size
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.SmoothingMode = 'AntiAlias'
        $thick = [Math]::Max(1.5, $size * 0.19); $outer = $size * 0.86; $x = ($size - $outer) / 2 + $thick / 2; $d = $outer - $thick
        $g.DrawEllipse((New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 124, 92, 255)), $thick), $x, $x, $d, $d)
        $dot = $size * 0.2
        $g.FillEllipse((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 255, 196, 107))), ($size - $dot) / 2, ($size - $dot) / 2, $dot, $dot)
        $bmp.Save((Join-Path $stage "Assets\$($logo[0])"), [System.Drawing.Imaging.ImageFormat]::Png)
        $g.Dispose(); $bmp.Dispose()
    }

    $msix = Join-Path $work "$packageName.msix"
    & $MakeAppx pack /d $stage /p $msix /nv /o | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "makeappx failed ($LASTEXITCODE)" }
    & $SignTool sign /fd SHA256 /sha1 $cert.Thumbprint /s My $msix | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "signtool failed ($LASTEXITCODE)" }

    Get-AppxPackage -Name $packageName | Remove-AppxPackage -ErrorAction SilentlyContinue
    try {
        Add-AppxPackage -Path $msix -ExternalLocation $ExeDir
    }
    catch {
        Write-Host "Add-AppxPackage failed from $work ($($_.Exception.Message)); retrying from the user profile..."
        $alt = Join-Path $env:USERPROFILE "$packageName.msix"
        Copy-Item $msix $alt -Force
        try { Add-AppxPackage -Path $alt -ExternalLocation $ExeDir }
        finally { Remove-Item $alt -ErrorAction SilentlyContinue }
    }
    $pkg = Get-AppxPackage -Name $packageName
    if (-not $pkg) { throw 'The package did not register' }
    Write-Host "Registered $($pkg.PackageFullName) for $ExeDir"

    if ($Restart) {
        # Identity applies to processes started after registration.
        Get-Process DIsland -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Milliseconds 800
        Start-Process (Join-Path $ExeDir 'DIsland.exe')
    }
    Write-Host 'OK'
    $code = 0
}
catch {
    Write-Host "FAILED: $($_.Exception.Message)"
    $code = 1
}
finally {
    Stop-Transcript | Out-Null
}
exit $code
