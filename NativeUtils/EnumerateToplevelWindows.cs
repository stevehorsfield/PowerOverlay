namespace PowerOverlay;

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using System.Windows.Interop;
using System.Windows;
using System.Collections.Generic;

public partial class NativeUtils
{
    private static IntPtr GetAnyTopLevelWindow()
    {
        IntPtr result = IntPtr.Zero;

        EnumWC handler = (IntPtr hwnd, IntPtr _) =>
        {
            if (hwnd == IntPtr.Zero) return false;

            bool isMinimised, isTopMost;
            var isTopLevel = IsTopLevelWindow(hwnd, out isMinimised, out isTopMost);

            if (!isTopLevel.HasValue)
            {
                return false; // Failed to enumerate
            }
            
            if (! isTopLevel.Value) return true; // continue
            if (isTopMost) return true; // continue

            result = hwnd;

            return false; // match found
        };
        EnumWindows(handler, IntPtr.Zero); // result cannot distinguish between error and early exit

        return result;
    }

    private static bool? IsTopLevelWindow(IntPtr hwnd, out bool isMinimised, out bool isTopMost)
    {
        isMinimised = false;
        isTopMost = false;

        tagWINDOWINFO info = new();
        info.dwSize = (uint)Marshal.SizeOf<tagWINDOWINFO>();

        if (GetWindowInfo(hwnd, ref info) == 0)
        {
            return null;
        };

        if (info.dwStyle.HasFlag(Win32WindowStyles.WS_ICONIC)) isMinimised = true;
        if (info.dwExStyle.HasFlag(Win32ExtendedWindowStyles.WS_EX_TOPMOST)) isTopMost = true;
        
        if (info.dwStyle.HasFlag(Win32WindowStyles.WS_POPUP)) return false;
        if (info.dwExStyle.HasFlag(Win32ExtendedWindowStyles.WS_EX_TOOLWINDOW)) return false;
        if (!info.dwStyle.HasFlag(Win32WindowStyles.WS_VISIBLE)) return false;

        return true;
    }

    public static IEnumerable<IntPtr> EnumerateTopLevelWindows(bool includeTopMost, bool includeMinimised)
    {
        IntPtr startingPoint = GetAnyTopLevelWindow();
        if (startingPoint == IntPtr.Zero) yield break;

        IntPtr hwndCurrent = GetWindow(startingPoint, GetWindowCmd.First);

        List<IntPtr> hwnds = new List<IntPtr>();

        while (hwndCurrent != IntPtr.Zero)
        {
            bool isMinimised, isTopMost;
            var isTopLevel = IsTopLevelWindow(hwndCurrent, out isMinimised, out isTopMost);

            if (isTopLevel.HasValue && isTopLevel.Value)
            {
                if (((!isMinimised) || includeMinimised)
                     &&
                     ((!isTopMost) || includeTopMost)
                    )
                {
                    hwnds.Add(hwndCurrent);
                }
            }

            hwndCurrent = GetWindow(hwndCurrent, GetWindowCmd.Next);
        }
        if (startingPoint == IntPtr.Zero)
        {
            MessageBox.Show("Failed to find a suitable top-level window");
        }
        foreach (var x in hwnds) yield return x;
    }
 
}