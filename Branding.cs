// D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
// Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.
// This file is part of a modified version ("Atoll") by Bruno Muller; it is not an official D-Island release.

namespace DIsland;

/// <summary>
/// Names of this modified version. The D-Island License (section 3) requires a different name and
/// icon, a statement that the software was modified and by whom, and the visible credit below.
/// Keep <see cref="Credit"/> exactly as written. The name is provisional: change it here, in the
/// window titles (MainWindow.xaml, SettingsWindow.xaml), DIsland.csproj (Product, ApplicationIcon),
/// installer\DIsland.iss and tools\make-icon.ps1. See docs\custom-roadmap.md.
/// </summary>
public static class Branding
{
    public const string Name = "Atoll";
    public const string Tagline = "A Dynamic Island for Windows";
    public const string Author = "Bruno Muller (BrunMul)";
    public const string Repository = "https://github.com/brunmul/d-island";

    /// <summary>Required credit (D-Island License, section 3(a)); must stay readable and unchanged.</summary>
    public const string Credit = "Based on D-Island by h7 (ibuildrigs) - https://github.com/H7gamer/d-island";

    public const string UpstreamRepository = "https://github.com/H7gamer/d-island";
}
