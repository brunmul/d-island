// D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
// Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.
// Part of the modified version "Atoll" by Bruno Muller; not an official D-Island release.

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace DIsland.Services;

/// <summary>
/// The themes the island can wear: the built-in ones plus every *.json in <see cref="Folder"/>. Applying
/// a theme has two parts: its shape, material, typography and accent source are copied into
/// <see cref="AppSettings"/> once, when it is chosen (<see cref="ApplyToSettings"/>); its colors are
/// pushed into the shared brushes of App.xaml by <see cref="ApplyPalette"/> whenever the island's
/// ApplyTheme runs. The brushes change color in place, so nothing has to be rebuilt.
/// </summary>
public static class ThemeService
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static List<IslandTheme>? _all;

    public static string Folder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DIsland", "themes");

    /// <summary>Bumped by <see cref="Reload"/>, so ApplyTheme's de-duplication key sees a reload.</summary>
    public static int Stamp { get; private set; }

    public static IReadOnlyList<IslandTheme> All => _all ??= Load();

    /// <summary>The theme of that name, or Classic when it is unknown (a deleted file, an old settings file).</summary>
    public static IslandTheme Find(string? name) =>
        All.FirstOrDefault(t => t.Name.Equals(name ?? "", StringComparison.OrdinalIgnoreCase)) ?? All[0];

    public static void Reload()
    {
        _all = null;
        Stamp++;
    }

    private static List<IslandTheme> Load()
    {
        var list = new List<IslandTheme> { IslandTheme.Classic(), IslandTheme.ArcosLight(), IslandTheme.ArcosDark() };
        list.AddRange(IslandTheme.SlackPresets());
        try
        {
            if (!Directory.Exists(Folder)) return list;
            foreach (var file in Directory.GetFiles(Folder, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var theme = JsonSerializer.Deserialize<IslandTheme>(File.ReadAllText(file), Json);
                    if (theme is null) continue;
                    if (string.IsNullOrWhiteSpace(theme.Name)) theme.Name = Path.GetFileNameWithoutExtension(file);
                    theme.File = file;
                    list.RemoveAll(t => t.Name.Equals(theme.Name, StringComparison.OrdinalIgnoreCase)); // a file may override a built-in
                    list.Add(theme);
                }
                catch (Exception ex)
                {
                    App.LogError(ex); // one bad file doesn't hide the others
                }
            }
        }
        catch (Exception)
        {
            // Unreadable folder: built-in themes only.
        }
        return list;
    }

    /// <summary>Copies the theme's shape, material, typography and accent into the settings and names it as chosen.</summary>
    public static void ApplyToSettings(IslandTheme t, AppSettings s)
    {
        s.Theme = t.Name;
        s.AccentSource = t.AccentSource;
        if (t.Colors.Accent is { Length: > 0 } accent) s.CustomAccent = accent;

        s.Mode = t.Shape.Mode;
        s.CornerRoundness = t.Shape.CornerRoundness;
        s.OutlineWidth = t.Shape.OutlineWidth;
        s.OutlineOpacity = t.Shape.OutlineOpacity;
        s.ShadowStrength = t.Shape.ShadowStrength;

        s.Material = t.Material.Material;
        s.BackgroundOpacity = t.Material.BackgroundOpacity;
        s.GlassTint = t.Material.GlassTint;
        s.GlassBlur = t.Material.GlassBlur;
        s.GlassShine = t.Material.GlassShine;
        s.TextShadow = t.Material.TextShadow;
        s.BackgroundTint = t.Material.BackgroundTint;
        s.AdaptiveBackground = t.Material.AdaptiveBackground;
        s.DesktopOpacity = t.Material.DesktopOpacity;
        s.SolidWhenExpanded = t.Material.SolidWhenExpanded;

        s.FontFamily = t.Typography.FontFamily;
    }

    /// <summary>The current look as a theme: the settings' shape, material and font plus the chosen theme's colors.</summary>
    public static IslandTheme FromSettings(AppSettings s, string name)
    {
        var current = Find(s.Theme);
        var colors = JsonSerializer.Deserialize<ThemeColors>(JsonSerializer.Serialize(current.Colors, Json), Json) ?? new ThemeColors();
        if (s.AccentSource == AccentSource.Custom) colors.Accent = s.CustomAccent;
        return new IslandTheme
        {
            Name = name,
            Description = $"Saved from Settings on {DateTime.Now:yyyy-MM-dd}, from \"{current.Name}\".",
            Author = Environment.UserName,
            AccentSource = s.AccentSource,
            Colors = colors,
            Shape = new ThemeShape
            {
                Mode = s.Mode,
                CornerRoundness = s.CornerRoundness,
                OutlineWidth = s.OutlineWidth,
                OutlineOpacity = s.OutlineOpacity,
                ShadowStrength = s.ShadowStrength,
            },
            Material = new ThemeMaterial
            {
                Material = s.Material,
                BackgroundOpacity = s.BackgroundOpacity,
                GlassTint = s.GlassTint,
                GlassBlur = s.GlassBlur,
                GlassShine = s.GlassShine,
                TextShadow = s.TextShadow,
                BackgroundTint = s.BackgroundTint,
                AdaptiveBackground = s.AdaptiveBackground,
                DesktopOpacity = s.DesktopOpacity,
                SolidWhenExpanded = s.SolidWhenExpanded,
            },
            Typography = new ThemeTypography { FontFamily = s.FontFamily },
        };
    }

    /// <summary>Writes the theme to <see cref="Folder"/> as &lt;name&gt;.json and reloads the list. Returns the path.</summary>
    public static string Export(IslandTheme theme)
    {
        Directory.CreateDirectory(Folder);
        var name = string.Join("-", theme.Name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        if (name.Length == 0) name = "theme";
        var path = Path.Combine(Folder, name + ".json");
        File.WriteAllText(path, JsonSerializer.Serialize(theme, Json));
        Reload();
        return path;
    }

    /// <summary>
    /// Pushes the palette into the application's shared brushes. They are deliberately not frozen and
    /// referenced through DynamicResource in styles, so changing their color here restyles every view.
    /// </summary>
    public static void ApplyPalette(ThemeColors c)
    {
        var r = Application.Current.Resources;
        Set(r, "Surface", c.Surface, Colors.Black);
        Set(r, "Ink", c.Ink, Colors.White);
        Set(r, "Secondary", c.InkSecondary, Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
        Set(r, "Tertiary", c.InkTertiary, Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF));
        Set(r, "Green", c.Success, Color.FromRgb(0x30, 0xD1, 0x58));
        Set(r, "Orange", c.Warning, Color.FromRgb(0xFF, 0x9F, 0x0A));
        Set(r, "Red", c.Danger, Color.FromRgb(0xFF, 0x45, 0x3A));
        Set(r, "Cyan", c.Info, Color.FromRgb(0x64, 0xD2, 0xFF));
        Set(r, "Purple", c.Focus, Color.FromRgb(0xBF, 0x5A, 0xF2));
        Set(r, "ArtPlaceholder", c.Placeholder, Color.FromRgb(0x2C, 0x2C, 0x2E));
        Set(r, "Fill", c.Fill, Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF));
        Set(r, "FillHover", c.FillHover, Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF));
        Set(r, "FillPressed", c.FillPressed, Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
        Set(r, "Separator", c.Separator, Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF));
    }

    private static void Set(ResourceDictionary resources, string key, string? text, Color fallback)
    {
        var color = ParseColor(text, fallback);
        if (resources[key] is SolidColorBrush brush && !brush.IsFrozen)
        {
            if (brush.Color != color) brush.Color = color;
        }
        else
        {
            resources[key] = new SolidColorBrush(color);
        }
    }

    public static Color ParseColor(string? text, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(text)) return fallback;
        try { return ColorConverter.ConvertFromString(text.Trim()) is Color c ? c : fallback; }
        catch (FormatException) { return fallback; }
    }
}
