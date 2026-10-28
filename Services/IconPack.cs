// D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
// Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.
// Part of the modified version "Atoll" by Bruno Muller; not an official D-Island release.

using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace DIsland.Services;

/// <summary>
/// Optional local icon pack: one-path SVGs on a 20-unit grid (Notion's page icons), read from
/// <c>icons\notion\</c> next to the exe. The pack is Notion's property, so it's never committed
/// (the folder is git-ignored); a missing file returns null and the caller keeps its Segoe glyph.
/// </summary>
public static partial class IconPack
{
    public const double Grid = 20;

    private static readonly string Folder = Path.Combine(AppContext.BaseDirectory, "icons", "notion");
    private static readonly ConcurrentDictionary<string, Geometry?> Cache = new();

    [GeneratedRegex("""<path\b[^>]*>""", RegexOptions.IgnoreCase)]
    private static partial Regex PathTag();

    [GeneratedRegex(@"\sd=""([^""]+)""")]
    private static partial Regex PathData();

    /// <summary>The icon's frozen geometry in 20 × 20 units, or null when the pack doesn't have it.</summary>
    public static Geometry? Get(string name) => Cache.GetOrAdd(name, Load);

    private static Geometry? Load(string name)
    {
        try
        {
            var file = Path.Combine(Folder, name + ".svg");
            if (!File.Exists(file)) return null;
            var group = new GeometryGroup { FillRule = FillRule.Nonzero };
            foreach (Match tag in PathTag().Matches(File.ReadAllText(file)))
            {
                if (PathData().Match(tag.Value) is not { Success: true } data) continue;
                // SVG fills nonzero by default; WPF's path syntax defaults to even-odd unless "F1".
                var rule = tag.Value.Contains("evenodd", StringComparison.OrdinalIgnoreCase) ? "F0 " : "F1 ";
                group.Children.Add(Geometry.Parse(rule + data.Groups[1].Value));
            }
            if (group.Children.Count == 0) return null;
            Geometry geometry = group.Children.Count == 1 ? group.Children[0] : group;
            geometry.Freeze();
            return geometry;
        }
        catch (Exception)
        {
            return null; // a broken file falls back to the glyph
        }
    }
}
