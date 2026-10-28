// D-Island - Copyright 2026 h7 (ibuildrigs) and contributors.
// Licensed under the D-Island License 1.0; see LICENSE.txt and NOTICE.txt.
// Part of the modified version "Atoll" by Bruno Muller; not an official D-Island release.

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using DIsland.Interop;

namespace DIsland.Services;

/// <summary>
/// Brings the app behind a notification (or a media session) to the front, by its AppUserModelID.
/// Tried in order, cheapest first:
///   1. a visible top-level window whose AppUserModelID property matches (Electron apps such as Discord,
///      Slack and Claude set it on their windows; packaged apps' frames carry it too): restore and focus it;
///   2. a packaged app id ("Package_hash!App"): IApplicationActivationManager, which Windows uses for the
///      Start menu, so a running single-instance app just comes forward;
///   3. a running process named after the id's last segment ("com.squirrel.Discord.Discord" → Discord,
///      "Spotify.exe" → Spotify): focus its main window;
///   4. otherwise launch it through shell:AppsFolder, like the quick apps.
/// The call follows a click on the island, which is what lets this process hand over the foreground.
/// </summary>
public static class AppActivator
{
    public static void Activate(string appId)
    {
        if (string.IsNullOrWhiteSpace(appId)) return;
        try
        {
            if (FocusWindowWithAppId(appId)) return;
            if (appId.Contains('!') && ActivatePackaged(appId)) return;
            if (FocusByProcessName(appId)) return;
        }
        catch (Exception ex)
        {
            App.LogError(ex);
        }
        QuickAppsService.Launch(appId);
    }

    // ---- 1. a window carrying the id

    private static bool FocusWindowWithAppId(string appId)
    {
        var self = (uint)Environment.ProcessId;
        var found = IntPtr.Zero;
        NativeMethods.EnumWindows((hwnd, _) =>
        {
            if (!NativeMethods.IsWindowVisible(hwnd)) return true;
            NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
            if (pid == self) return true;
            if (!string.Equals(WindowAppId(hwnd), appId, StringComparison.OrdinalIgnoreCase)) return true;
            found = hwnd;
            return false;
        }, IntPtr.Zero);
        return found != IntPtr.Zero && Focus(found);
    }

    /// <summary>The window's System.AppUserModel.ID property, or null.</summary>
    private static string? WindowAppId(IntPtr hwnd)
    {
        var iid = typeof(IPropertyStore).GUID;
        if (SHGetPropertyStoreForWindow(hwnd, ref iid, out var store) != 0 || store is null) return null;
        try
        {
            var key = new PropKey { fmtid = PKEY_AppUserModel_ID, pid = 5 };
            if (store.GetValue(ref key, out var value) != 0) return null;
            try { return value.vt == VT_LPWSTR ? Marshal.PtrToStringUni(value.p) : null; }
            finally { PropVariantClear(ref value); }
        }
        finally
        {
            Marshal.ReleaseComObject(store);
        }
    }

    // ---- 2. packaged apps

    private static bool ActivatePackaged(string appId)
    {
        var manager = (IApplicationActivationManager)new ApplicationActivationManager();
        try
        {
            return manager.ActivateApplication(appId, null, 0 /* AO_NONE */, out _) == 0;
        }
        finally
        {
            Marshal.ReleaseComObject(manager);
        }
    }

    // ---- 3. a running desktop app named like the id

    private static bool FocusByProcessName(string appId)
    {
        var name = appId.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || appId.Contains('\\')
            ? Path.GetFileNameWithoutExtension(appId)
            : appId[(appId.LastIndexOf('.') + 1)..];
        if (name.Length == 0) return false;
        foreach (var process in Process.GetProcessesByName(name))
        {
            var hwnd = process.MainWindowHandle;
            process.Dispose();
            if (hwnd != IntPtr.Zero && Focus(hwnd)) return true;
        }
        return false;
    }

    private static bool Focus(IntPtr hwnd)
    {
        if (NativeMethods.IsIconic(hwnd)) NativeMethods.ShowWindow(hwnd, 9 /* SW_RESTORE */);
        return NativeMethods.SetForegroundWindow(hwnd);
    }

    // ---- interop

    private const ushort VT_LPWSTR = 31;
    private static readonly Guid PKEY_AppUserModel_ID = new("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3");

    [StructLayout(LayoutKind.Sequential)]
    private struct PropKey
    {
        public Guid fmtid;
        public int pid;
    }

    /// <summary>PROPVARIANT: a 16-bit type, three reserved words, then the value union (two pointers' worth).</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct PropVariant
    {
        public ushort vt;
        public ushort reserved1, reserved2, reserved3;
        public IntPtr p;
        public IntPtr p2;
    }

    [ComImport, Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyStore
    {
        [PreserveSig] int GetCount(out uint count);
        [PreserveSig] int GetAt(uint index, out PropKey key);
        [PreserveSig] int GetValue(ref PropKey key, out PropVariant value);
        [PreserveSig] int SetValue(ref PropKey key, ref PropVariant value);
        [PreserveSig] int Commit();
    }

    [ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IApplicationActivationManager
    {
        [PreserveSig]
        int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
            [MarshalAs(UnmanagedType.LPWStr)] string? arguments, uint options, out uint processId);

        [PreserveSig]
        int ActivateForFile([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId, IntPtr itemArray,
            [MarshalAs(UnmanagedType.LPWStr)] string? verb, out uint processId);

        [PreserveSig]
        int ActivateForProtocol([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId, IntPtr itemArray,
            out uint processId);
    }

    [ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
    private class ApplicationActivationManager
    {
    }

    [DllImport("shell32.dll")]
    private static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out IPropertyStore? store);

    [DllImport("ole32.dll")]
    private static extern int PropVariantClear(ref PropVariant value);
}
