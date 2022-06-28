namespace overlay_popup;
using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

public partial class NativeUtils
{
    public enum Win32ShowCmd : int
    {
        Hide = 0,
        Normal = 1,
        ActivateAndMinimize = 2,
        ActivateAndMaximize = 3,
        NoActivate = 4,
        ActivateAndShowAsIs = 5,
        MinimizeAndDeactivate = 6,
        MinimizeOnly = 7,
        ShowAsIs = 8,
        ActivateAndRestore = 9,
        ShowAsStartup = 10,
        ForceMinimize = 11,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GuiThreadInfo
    {
        public uint cbSize;
        public uint flags;
        public IntPtr hwndActive;
        public IntPtr hwndFocus;
        public IntPtr hwndCapture;
        public IntPtr hwndMenuOwner;
        public IntPtr hwndMoveSize;
        public IntPtr hwndCaret;
        public int caretLeft, caretTop, caretRight, caretBottom;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct IntPtrToInt64Union
    {
        [FieldOffset(0)]
        public IntPtr ptr;
        [FieldOffset(0)]
        public Int64 value;
    }
    private struct KeystrokeFlagsLparam
    {
        public UInt16 repeatCount;
        public byte scanCode;
        public bool isExtended;
        public bool altPressed;
        public bool wasPressed;
        public bool transitionToRelease;
    }

    private enum GetWindowCmd : uint
    {
        First = 0,
        Last = 1,
        Next = 2,
        Previous = 3,
        Owner = 4,
        Child = 5,
        EnabledPopup = 6,
    }

    [Flags]
    public enum Win32WindowStyles : uint
    {
        WS_VISIBLE = 0x10000000,
        WS_ICONIC = 0x20000000,
        WS_POPUP = 0x80000000,
    }

    [Flags]
    public enum Win32ExtendedWindowStyles : uint
    {
        WS_EX_TOOLWINDOW = 0x00000080,
        WS_EX_TOPMOST = 0x00000008,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct tagWINDOWINFO
    {
        public uint dwSize;
        public int windowLeft, windowTop, windowRight, windowBottom;
        public int clientLeft, clientTop, clientRight, clientBottom;
        public Win32WindowStyles dwStyle;
        public Win32ExtendedWindowStyles dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders, cyWindowBorders;
        public UInt16 atomWindowType, creatorVersion;
    }
}