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

    public static void RepositionWindow(IntPtr hwnd, bool setPosition, bool setSize, int left, int top, int width, int height)
    {
        Win32SetWindowPosFlags flags = 
            Win32SetWindowPosFlags.SWP_NOZORDER 
            | Win32SetWindowPosFlags.SWP_NOACTIVATE
            | Win32SetWindowPosFlags.SWP_NOOWNERZORDER;
        if (!setPosition) flags |= Win32SetWindowPosFlags.SWP_NOMOVE;
        if (!setSize) flags |= Win32SetWindowPosFlags.SWP_NOSIZE;

        SetWindowPos(hwnd, IntPtr.Zero, left, top, width, height, flags);
    }
}