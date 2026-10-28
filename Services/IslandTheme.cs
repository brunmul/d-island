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

    /// <summary>When set, the fill is a top-to-bottom gradient from <see cref="Surface"/> to this color (the Slack-style presets).</summary>
    public string? SurfaceEnd { get; set; }

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

    // ---- the 34 Slack presets of Arcos / DockyWin, as gradients (SlackTheme.cs in Files-Finder, value for value)

    private static readonly (string Id, string Name, int Top, int Bottom)[] SlackPresetTable =
    {
        ("kind-of-blue", "Kind of Blue", 0x96C4EC, 0x4676A1),
        ("funky-fresh", "Funky Fresh", 0xC4BEF7, 0x82C8AC),
        ("jazz-club", "Jazz Club", 0x701B1A, 0x331134),
        ("electric-fusion", "Electric Fusion", 0xE0F1A3, 0xB6F9B2),
        ("brassy", "Brassy", 0xEBD47B, 0xC2A94F),
        ("sunglasses-inside", "Sunglasses Inside", 0x2F1236, 0x220926),
        ("aubergine", "Aubergine", 0x391742, 0x29092E),
        ("clementine", "Clementine", 0x833B17, 0x7F3612),
        ("banana", "Banana", 0xFDF5C5, 0xF8D854),
        ("jade", "Jade", 0x2C5B42, 0x26513E),
        ("lagoon", "Lagoon", 0x204868, 0x1E344B),
        ("barbra", "Barbra", 0xF6CEDE, 0xEE88AB),
        ("gray", "Gray", 0xECECEC, 0xCBCCCF),
        ("mood-indigo", "Mood Indigo", 0x17215A, 0x0A154C),
        ("tritanopia", "Tritanopia", 0x2F2F2F, 0x000000),
        ("protanopia", "Protanopia", 0x423247, 0x210429),
        ("raspberry-beret", "Raspberry Beret", 0xF1DEFB, 0xEE87AA),
        ("big-business", "Big Business", 0x3C3C3F, 0x1D2A6E),
        ("pog", "POG", 0xF2D9E4, 0xE2B447),
        ("mint-chip", "Mint Chip", 0xD1EDBB, 0x8499E6),
        ("pbandj", "PB&J", 0xE4D5C2, 0xD69BB8),
        ("chill-vibes", "Chill Vibes", 0x2D5C43, 0x214752),
        ("forest-floor", "Forest Floor", 0x445719, 0x4D3618),
        ("slackr", "Slackr", 0xF6CFE0, 0x6AC3ED),
        ("sea-glass", "Sea Glass", 0xC4EEF1, 0xD4A5F0),
        ("lemon-lime", "Lemon Lime", 0x7C621B, 0x26500F),
        ("falling-leaves", "Falling Leaves", 0x553C20, 0x6A5416),
        ("sunrise", "Sunrise", 0xFBE8DD, 0xF1A9C3),
        ("choco-mint", "Choco Mint", 0x493219, 0x204233),
        ("cmyk", "CMYK", 0xBCEBFC, 0xEB8BAE),
        ("haberdashery", "Haberdashery", 0x19354F, 0x3A2E0B),
        ("hoth", "Hoth", 0xF9F9F9, 0xB3E8FB),
        ("ochin", "Ochin", 0x1F4167, 0x16305B),
        ("sweet-treat", "Sweet Treat", 0xF9DDDC, 0xF0A298),
    };

    /// <summary>The Slack-style gradient presets, built from the table above.</summary>
    public static IEnumerable<IslandTheme> SlackPresets() =>
        SlackPresetTable.Select(p => FromGradient(p.Name, p.Top, p.Bottom, $"Arcos / Slack preset \"{p.Id}\"."));

    /// <summary>
    /// A theme from a top-to-bottom gradient, with DockyWin's rules: ink by the contrast on the gradient's
    /// middle (paper on dark, ink on light), every fill a transparency of that ink, and the accent the
    /// hue of the more colorful stop (clay when the theme is neutral). Shape and material follow Arcos.
    /// </summary>
    public static IslandTheme FromGradient(string name, int top, int bottom, string? description = null)
    {
        var topColor = Rgb(top);
        var bottomColor = Rgb(bottom);
        var dark = IsDarkSurface(Mix(topColor, bottomColor, 0.5));
        var ink = dark ? LightInk : DarkInk;
        var (hue, saturation, _) = ToHsl(Chroma(topColor) >= Chroma(bottomColor) ? topColor : bottomColor);
        var neutral = saturation < 0.08 || Math.Max(Chroma(topColor), Chroma(bottomColor)) < 12;
        var accent = neutral ? "#D97757" : Hex(FromHsl(hue, 0.6, dark ? 0.68 : 0.45));
        var arcos = dark ? ArcosDark() : ArcosLight();
        return new IslandTheme
        {
            Name = name,
            Description = description,
            Author = "Bruno Muller (Arcos / DockyWin)",
            BuiltIn = true,
            AccentSource = AccentSource.Custom,
            Colors = new ThemeColors
            {
                Surface = Hex(topColor),
                SurfaceEnd = Hex(bottomColor),
                Ink = Hex(ink),
                InkSecondary = Hex(ink, 0.58),
                InkTertiary = Hex(ink, 0.35),
                Accent = accent,
                Success = arcos.Colors.Success,
                Warning = arcos.Colors.Warning,
                Danger = arcos.Colors.Danger,
                Info = arcos.Colors.Info,
                Focus = arcos.Colors.Focus,
                Placeholder = Hex(ink, 0.10),
                Fill = Hex(ink, 0.07),
                FillHover = Hex(ink, 0.06),
                FillPressed = Hex(ink, 0.16),
                Separator = Hex(ink, 0.16),
                Outline = Hex(ink),
                TextShadow = "#00000000",
            },
            Shape = arcos.Shape,
            Material = arcos.Material,
            Typography = arcos.Typography,
        };
    }

    // DockyWin's palette: ink on light surfaces, paper on dark ones.
    private static readonly System.Windows.Media.Color DarkInk = Rgb(0x1F1E1D);
    private static readonly System.Windows.Media.Color LightInk = Rgb(0xF2F0EA);

    private static System.Windows.Media.Color Rgb(int rgb) =>
        System.Windows.Media.Color.FromRgb((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF));

    private static string Hex(System.Windows.Media.Color c, double opacity = 1) =>
        $"#{(byte)Math.Round(opacity * 255):X2}{c.R:X2}{c.G:X2}{c.B:X2}";

    private static System.Windows.Media.Color Mix(System.Windows.Media.Color from, System.Windows.Media.Color to, double amount) =>
        System.Windows.Media.Color.FromRgb(
            (byte)Math.Round(from.R + (to.R - from.R) * amount),
            (byte)Math.Round(from.G + (to.G - from.G) * amount),
            (byte)Math.Round(from.B + (to.B - from.B) * amount));

    private static int Chroma(System.Windows.Media.Color c) =>
        Math.Max(c.R, Math.Max(c.G, c.B)) - Math.Min(c.R, Math.Min(c.G, c.B));

    /// <summary>Whether light ink has the better WCAG contrast on the surface.</summary>
    private static bool IsDarkSurface(System.Windows.Media.Color surface)
    {
        var luminance = Luminance(surface);
        return (Luminance(LightInk) + 0.05) / (luminance + 0.05) > (luminance + 0.05) / (Luminance(DarkInk) + 0.05);
    }

    private static double Luminance(System.Windows.Media.Color c)
    {
        static double Channel(byte value)
        {
            var v = value / 255.0;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
    }

    private static (double Hue, double Saturation, double Lightness) ToHsl(System.Windows.Media.Color c)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
        var lightness = (max + min) / 2;
        if (max == min) return (0, 0, lightness);
        var delta = max - min;
        var saturation = lightness > 0.5 ? delta / (2 - max - min) : delta / (max + min);
        var hue = max == r ? (g - b) / delta + (g < b ? 6 : 0) : max == g ? (b - r) / delta + 2 : (r - g) / delta + 4;
        return (hue / 6, saturation, lightness);
    }

    private static System.Windows.Media.Color FromHsl(double hue, double saturation, double lightness)
    {
        static double Channel(double p, double q, double t)
        {
            t = t < 0 ? t + 1 : t > 1 ? t - 1 : t;
            return t < 1.0 / 6 ? p + (q - p) * 6 * t : t < 0.5 ? q : t < 2.0 / 3 ? p + (q - p) * (2.0 / 3 - t) * 6 : p;
        }
        var q = lightness < 0.5 ? lightness * (1 + saturation) : lightness + saturation - lightness * saturation;
        var p = 2 * lightness - q;
        return System.Windows.Media.Color.FromRgb(
            (byte)Math.Round(Channel(p, q, hue + 1.0 / 3) * 255),
            (byte)Math.Round(Channel(p, q, hue) * 255),
            (byte)Math.Round(Channel(p, q, hue - 1.0 / 3) * 255));
    }

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
