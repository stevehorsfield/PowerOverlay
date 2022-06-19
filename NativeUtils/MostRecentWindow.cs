namespace overlay_popup;

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using System.Windows.Interop;

public partial class NativeUtils {
    
    public static IntPtr GetActiveAppHwnd() {
        IntPtr hwndThis =
            new WindowInteropHelper(App.Current.MainWindow).Handle;
        
        unsafe {
            IntPtr hwndLast = GetForegroundWindow();
            return hwndLast;
        }

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

    [DllImport("user32.dll")]
    public unsafe static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public unsafe static extern IntPtr GetTopWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    public unsafe static extern IntPtr FindWindowExW(IntPtr hwndParent, IntPtr hwndChildAfter, IntPtr lpszClass, IntPtr lpszWindow);

    [DllImport("user32.dll")]
    public unsafe static extern int GetWindowTextW(IntPtr hwnd, IntPtr buffer, int maxCount);

    [DllImport("user32.dll")]
    public unsafe static extern uint GetWindowThreadProcessId(IntPtr hwnd, ref uint processId);

    internal static string GetWindowProcessName(IntPtr hwndApp)
    {
        unsafe
        {
            uint processId = 0;
            uint _ = GetWindowThreadProcessId(hwndApp, ref processId);

            using var p = System.Diagnostics.Process.GetProcessById((int)processId);

            return p.ProcessName;
            
        }
    }
}