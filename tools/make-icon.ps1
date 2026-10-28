# D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
# Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.
# Part of the modified version "Atoll" by Bruno Muller; not an official D-Island release.

# Generates Atoll.ico, this modified version's own icon: a violet ring (an atoll) with an amber dot in
# the lagoon. The D-Island License (section 3(c)) requires an icon clearly different from D-Island's
# black pill. Multi-size, PNG-compressed entries (Windows Vista and later read these). The tray icon
# is drawn the same way in TrayIcon.CreateIcon; keep the two in step.
#
# Usage: .\tools\make-icon.ps1 [-Force]
param([switch]$Force)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$ico = Join-Path $root 'Atoll.ico'
if ((Test-Path $ico) -and -not $Force) { Write-Host "Atoll.ico exists; use -Force to regenerate."; return }

Add-Type -AssemblyName System.Drawing
$ring = [System.Drawing.Color]::FromArgb(255, 124, 92, 255)
$lagoon = [System.Drawing.Color]::FromArgb(255, 255, 196, 107)
$sizes = 16, 24, 32, 48, 64, 128, 256
$frames = foreach ($size in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.Clear([System.Drawing.Color]::Transparent)
    $thick = [Math]::Max(1.5, $size * 0.19)
    $outer = $size * 0.86
    $x = ($size - $outer) / 2 + $thick / 2
    $d = $outer - $thick
    $pen = New-Object System.Drawing.Pen $ring, $thick
    $g.DrawEllipse($pen, $x, $x, $d, $d)
    $dot = [Math]::Max(1, $size * 0.2)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $lagoon), ($size - $dot) / 2, ($size - $dot) / 2, $dot, $dot)
    $g.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    , @($size, $ms.ToArray())
}
$out = New-Object System.IO.MemoryStream
$wr = New-Object System.IO.BinaryWriter $out
$wr.Write([uint16]0); $wr.Write([uint16]1); $wr.Write([uint16]$frames.Count)   # ICONDIR
$offset = 6 + 16 * $frames.Count
foreach ($f in $frames) {                                                         # ICONDIRENTRY
    $s = if ($f[0] -ge 256) { 0 } else { $f[0] }
    $wr.Write([byte]$s); $wr.Write([byte]$s); $wr.Write([byte]0); $wr.Write([byte]0)
    $wr.Write([uint16]1); $wr.Write([uint16]32); $wr.Write([uint32]$f[1].Length); $wr.Write([uint32]$offset)
    $offset += $f[1].Length
}
foreach ($f in $frames) { $wr.Write([byte[]]$f[1]) }
[System.IO.File]::WriteAllBytes($ico, $out.ToArray())
Write-Host "Wrote $ico"
