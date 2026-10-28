// D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
// Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DIsland.Interop;
using DIsland.Services;

namespace DIsland;

public sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Icon _icon;

    public TrayIcon(AppSettings settings, Action onOpenSettings, Func<bool> isHidden, Action<bool> setHidden, Action onExit)
    {
        _icon = CreateIcon();

        var menu = new ContextMenuStrip();

        var open = new ToolStripMenuItem("Settings…") { Font = new Font(menu.Font, FontStyle.Bold) };
        open.Click += (_, _) => onOpenSettings();

        var hide = new ToolStripMenuItem("Hide island") { CheckOnClick = true };
        hide.Click += (_, _) => setHidden(hide.Checked);

        var hideFlyout = new ToolStripMenuItem("Hide Windows volume flyout") { CheckOnClick = true };
        hideFlyout.Click += (_, _) =>
        {
            settings.HideVolumeFlyout = hideFlyout.Checked;
            settings.NotifyChanged();
            settings.Save();
        };

        var startup = new ToolStripMenuItem("Start with Windows") { CheckOnClick = true };
        startup.Click += (_, _) =>
        {
            try { StartupManager.SetEnabled(startup.Checked); }
            catch (Exception) { /* registry write blocked by policy */ }
            startup.Checked = StartupManager.IsEnabled;
        };

        var exit = new ToolStripMenuItem("Exit");
        exit.Click += (_, _) => onExit();

        menu.Items.AddRange(new ToolStripItem[]
        {
            open, hide, new ToolStripSeparator(), hideFlyout, startup, new ToolStripSeparator(), exit,
        });

        // Settings can also change from the settings window; sync checkmarks each time the menu opens.
        menu.Opening += (_, _) =>
        {
            hide.Checked = isHidden();
            hide.ShortcutKeyDisplayString = settings.ToggleHotkey;
            hideFlyout.Checked = settings.HideVolumeFlyout;
            startup.Checked = StartupManager.IsEnabled;
        };

        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
            Text = Branding.Name,
            ContextMenuStrip = menu,
            Visible = true,
        };
        _notifyIcon.MouseDoubleClick += (_, _) => onOpenSettings();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
        _icon.Dispose();
    }

    /// <summary>
    /// Draws a small violet ring (an atoll) with an amber dot, so no .ico asset is needed. Same design as
    /// Atoll.ico (tools\make-icon.ps1): the license requires an icon clearly different from D-Island's pill.
    /// </summary>
    private static Icon CreateIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using var ring = new Pen(Color.FromArgb(255, 124, 92, 255), 6f);
            using var lagoon = new SolidBrush(Color.FromArgb(255, 255, 196, 107));
            g.DrawEllipse(ring, 5, 5, 22, 22);
            g.FillEllipse(lagoon, 13, 13, 6, 6);
        }

        var handle = bmp.GetHicon();
        using var temp = Icon.FromHandle(handle);
        var icon = (Icon)temp.Clone();
        NativeMethods.DestroyIcon(handle);
        return icon;
    }
}
