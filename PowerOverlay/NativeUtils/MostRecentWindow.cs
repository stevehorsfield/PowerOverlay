namespace PowerOverlay;

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using System.Windows.Interop;

public partial class NativeUtils {
    
    public static IntPtr GetActiveAppHwnd() {
        IntPtr hwndLast = GetForegroundWindow();
        return hwndLast;
    }

    public static string GetWindowTitle(IntPtr hwnd) {
        IntPtr buffer = Marshal.AllocHGlobal(2048);
        try
        {
            var result = GetWindowTextW(hwnd, buffer, 2048);
            if (result == 0) return String.Empty;
            return Marshal.PtrToStringUni(buffer) ?? String.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
    internal static string GetWindowProcessName(IntPtr hwndApp)
    {
        uint processId = 0;
        uint _ = GetWindowThreadProcessId(hwndApp, ref processId);

        using var p = System.Diagnostics.Process.GetProcessById((int)processId);

        return p.ProcessName;
    }

    internal static string GetWindowProcessMainFilename(IntPtr hwndApp)
    {
        unsafe
        {
            uint processId = 0;
            uint _ = GetWindowThreadProcessId(hwndApp, ref processId);

            try
            {
                using var p = System.Diagnostics.Process.GetProcessById((int)processId);

                return p.MainModule?.FileName ?? String.Empty;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
    }
}