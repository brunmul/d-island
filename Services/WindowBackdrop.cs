// D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
// Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.
// Part of the modified version "Atoll" by Bruno Muller; not an official D-Island release.

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DIsland.Services;

/// <summary>
/// Gives an ordinary (framed) window the native Windows 11 look: the Mica backdrop behind its content
/// and a title bar in dark or light mode. Mica needs Windows 11 22H2 (build 22621); earlier builds only
/// get the dark-mode title bar, and the caller keeps a solid background.
/// </summary>
public static class WindowBackdrop
{
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMSBT_MAINWINDOW = 2; // Mica

    public static bool IsMicaAvailable => Environment.OSVersion.Version.Build >= 22621;

    /// <summary>Applies dark or light chrome, then Mica when available. Returns whether Mica is on (then the window's background must stay transparent).</summary>
    public static bool Apply(Window window, bool dark)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return false;
        var darkValue = dark ? 1 : 0;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkValue, sizeof(int));
        if (!IsMicaAvailable) return false;

        // The backdrop shows through wherever nothing is painted: WPF's own surface must be transparent
        // and the frame extended over the whole client area.
        if (HwndSource.FromHwnd(hwnd) is { } source) source.CompositionTarget.BackgroundColor = Colors.Transparent;
        var margins = new MARGINS { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);
        var type = DWMSBT_MAINWINDOW;
        return DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref type, sizeof(int)) == 0;
    }

    /// <summary>Perceived lightness 0..1 of a color (Rec. 601 weights), to pick dark or light chrome for the text.</summary>
    public static double Lightness(Color c) => (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int Left, Right, Top, Bottom;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
}
