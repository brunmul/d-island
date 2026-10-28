// D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
// Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.
// Part of the modified version "Atoll" by Bruno Muller; not an official D-Island release.

using System.Text.Json.Serialization;

namespace DIsland.Services;

/// <summary>
/// Colors of a theme, as "#RRGGBB" or "#AARRGGBB" strings (WPF ColorConverter syntax). They become the
/// shared brushes in App.xaml (Ink, Secondary, Green, …), so every view follows a theme change at once.
/// Defaults are the Classic (D-Island 1.2.1) look: white text on black.
/// </summary>
public sealed class ThemeColors
{
    /// <summary>The island's base fill; the material adds transparency or blur over it.</summary>
    public string Surface { get; set; } = "#000000";

    /// <summary>Text and icons.</summary>
    public string Ink { get; set; } = "#FFFFFF";

    /// <summary>Secondary text: app names, times, labels.</summary>
    public string InkSecondary { get; set; } = "#99FFFFFF";

    /// <summary>Hints only: page dots, scroll thumbs, dashed outlines.</summary>
    public string InkTertiary { get; set; } = "#55FFFFFF";

    /// <summary>Fixed accent, used when the theme's <see cref="IslandTheme.AccentSource"/> is Custom.</summary>
    public string? Accent { get; set; }

    public string Success { get; set; } = "#30D158";
    public string Warning { get; set; } = "#FF9F0A";
    public string Danger { get; set; } = "#FF453A";
    public string Info { get; set; } = "#64D2FF";

    /// <summary>Do Not Disturb.</summary>
    public string Focus { get; set; } = "#BF5AF2";

    /// <summary>Box behind artwork and app icons that haven't loaded.</summary>
    public string Placeholder { get; set; } = "#2C2C2E";

    /// <summary>Resting fill of fields, chips, progress tracks and the selected tab.</summary>
    public string Fill { get; set; } = "#1FFFFFFF";
    public string FillHover { get; set; } = "#26FFFFFF";
    public string FillPressed { get; set; } = "#40FFFFFF";
    public string Separator { get; set; } = "#22FFFFFF";

    /// <summary>Outline color (its strength is the shape's OutlineOpacity); null = the accent, else the ink.</summary>
    public string? Outline { get; set; }

    /// <summary>Soft shadow behind text over see-through materials; transparent disables it visually.</summary>
    public string TextShadow { get; set; } = "#000000";
}

public sealed class ThemeShape
{
    public IslandMode Mode { get; set; } = IslandMode.DynamicIsland;
    public double CornerRoundness { get; set; } = 1.0;
    public double OutlineWidth { get; set; }
    public double OutlineOpacity { get; set; } = 0.25;
    public double ShadowStrength { get; set; }
}

public sealed class ThemeMaterial
{
    public IslandMaterial Material { get; set; } = IslandMaterial.Opaque;
    public double BackgroundOpacity { get; set; } = 0.7;
    public double GlassTint { get; set; } = 0.12;
    public double GlassBlur { get; set; } = 0.35;
    public double GlassShine { get; set; } = 0.3;
    public bool TextShadow { get; set; } = true;

    /// <summary>How much of the accent is mixed into the surface, 0..0.5.</summary>
    public double BackgroundTint { get; set; } = 0.18;

    public bool AdaptiveBackground { get; set; } = true;
    public double DesktopOpacity { get; set; } = 0.15;
    public bool SolidWhenExpanded { get; set; } = true;
}

public sealed class ThemeTypography
{
    public string FontFamily { get; set; } = "Segoe UI Variable Text";
}

/// <summary>
/// One look for the island: colors, shape, material and typography, loadable from a JSON file in
/// <see cref="ThemeService.Folder"/>. Choosing a theme copies its shape, material, typography and accent
/// source into <see cref="AppSettings"/> (the sliders then adjust it); its colors apply as long as it is
/// the chosen theme. Built-in themes live in code below.
/// </summary>
public sealed class IslandTheme
{
    public const string ClassicName = "Classic";

    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Author { get; set; }

    public AccentSource AccentSource { get; set; } = AccentSource.Auto;
    public ThemeColors Colors { get; set; } = new();
    public ThemeShape Shape { get; set; } = new();
    public ThemeMaterial Material { get; set; } = new();
    public ThemeTypography Typography { get; set; } = new();

    [JsonIgnore]
    public bool BuiltIn { get; set; }

    /// <summary>The JSON file it was loaded from; null for built-in themes.</summary>
    [JsonIgnore]
    public string? File { get; set; }

    /// <summary>The look of D-Island 1.2.1: white on black, accent from the wallpaper or media app.</summary>
    public static IslandTheme Classic() => new()
    {
        Name = ClassicName,
        Description = "The original D-Island look: white on black, accent from the wallpaper or the media app.",
        Author = "h7 (ibuildrigs)",
        BuiltIn = true,
    };

    /// <summary>
    /// Arcos (Bruno's design system, light theme): warm opaque paper, ink at 85%, one clay accent, thin
    /// 1 px outline, continuous corners, no glass, no shadow, system font.
    /// </summary>
    public static IslandTheme ArcosLight() => new()
    {
        Name = "Arcos Clair",
        Description = "Arcos: papier chaud opaque, encre, un seul accent argile. Pas de verre, pas d'ombre.",
        Author = "Bruno Muller (design system Arcos)",
        BuiltIn = true,
        AccentSource = AccentSource.Custom,
        Colors = new ThemeColors
        {
            Surface = "#F5F4ED",
            Ink = "#D9000000",
            InkSecondary = "#80000000",
            InkTertiary = "#42000000",
            Accent = "#D97757",
            Success = "#5C7A47",
            Warning = "#A8663D",
            Danger = "#E5484D",
            Info = "#3D7A78",
            Focus = "#785794",
            Placeholder = "#12000000",
            Fill = "#12000000",
            FillHover = "#0F000000",
            FillPressed = "#1F000000",
            Separator = "#29000000",
            Outline = "#000000",
            TextShadow = "#00000000",
        },
        Shape = new ThemeShape { CornerRoundness = 0.55, OutlineWidth = 1, OutlineOpacity = 0.18, ShadowStrength = 0 },
        Material = new ThemeMaterial
        {
            Material = IslandMaterial.Opaque,
            TextShadow = false,
            BackgroundTint = 0,
            AdaptiveBackground = false,
            DesktopOpacity = 1,
            SolidWhenExpanded = true,
        },
        Typography = new ThemeTypography { FontFamily = "Segoe UI Variable Text" },
    };

    /// <summary>Arcos, dark theme: warm charcoal, ink at 85%, the same clay accent and muted tints.</summary>
    public static IslandTheme ArcosDark() => new()
    {
        Name = "Arcos Sombre",
        Description = "Arcos : charbon chaud opaque, encre à 85 %, accent argile, teintes sourdes.",
        Author = "Bruno Muller (design system Arcos)",
        BuiltIn = true,
        AccentSource = AccentSource.Custom,
        Colors = new ThemeColors
        {
            Surface = "#1F1E1D",
            Ink = "#D9FFFFFF",
            InkSecondary = "#8CFFFFFF",
            InkTertiary = "#40FFFFFF",
            Accent = "#D97757",
            Success = "#A0B295",
            Warning = "#CDA68F",
            Danger = "#E5484D",
            Info = "#8FB2B1",
            Focus = "#B19DC1",
            Placeholder = "#12FFFFFF",
            Fill = "#12FFFFFF",
            FillHover = "#0FFFFFFF",
            FillPressed = "#1FFFFFFF",
            Separator = "#29FFFFFF",
            Outline = "#FFFFFF",
            TextShadow = "#00000000",
        },
        Shape = new ThemeShape { CornerRoundness = 0.55, OutlineWidth = 1, OutlineOpacity = 0.18, ShadowStrength = 0 },
        Material = new ThemeMaterial
        {
            Material = IslandMaterial.Opaque,
            TextShadow = false,
            BackgroundTint = 0,
            AdaptiveBackground = false,
            DesktopOpacity = 1,
            SolidWhenExpanded = true,
        },
        Typography = new ThemeTypography { FontFamily = "Segoe UI Variable Text" },
    };
}
