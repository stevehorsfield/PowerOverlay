﻿namespace PowerOverlay;
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

    [Flags]
    public enum Win32SetWindowPosFlags : uint
    {
        SWP_ASYNCWINDOWPOS = 0x4000,
        SWP_DEFERERASE = 0x2000,
        SWP_DRAWFRAME = 0x0020,
        SWP_FRAMECHANGED = 0x0020, // Same value from https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOACTIVATE = 0x0010,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOMOVE = 0x0002,
        SWP_NOOWNERZORDER = 0x0200,
        SWP_NOREDRAW = 0x0008,
        SWP_NOREPOSITION = 0x0200,
        SWP_NOSENDCHANGING = 0x0400,
        SWP_NOSIZE = 0x0001,
        SWP_NOZORDER = 0x0004,
        SWP_SHOWWINDOW = 0x0040,
    }

    public enum Win32MapVirtualKeyMode : uint
    {
        VkToScan = 0,
        ScanToVk = 1,
        VkToChar = 2,
        ScanToVkEx = 3,
        VkToScanEx = 4,
    }

    public enum InputType : uint
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2,
    }

    [Flags]
    public enum Win32KeyboardInputFlags : uint
    {
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        ScanCode = 0x0008,
        Unicode = 0x0004,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tagKEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public Win32KeyboardInputFlags dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tagMOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tagHARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct tagINPUT
    {
        [FieldOffset(0)]
        public InputType type;
        [FieldOffset(8)]
        public tagKEYBDINPUT keyInput;
        [FieldOffset(8)]
        public tagMOUSEINPUT mouseInput;
        [FieldOffset(8)]
        public tagHARDWAREINPUT hwInput;
    }

    public enum Win32VirtualKey : ushort // from winuser.h
    {
        VK_LBUTTON = 0x01,
        VK_RBUTTON = 0x02,
        VK_CANCEL = 0x03,
        VK_MBUTTON = 0x04,
        VK_XBUTTON1 = 0x05,
        VK_XBUTTON2 = 0x06,
        VK_BACK = 0x08,
        VK_TAB = 0x09,
        VK_CLEAR = 0x0C,
        VK_RETURN = 0x0D,
        VK_SHIFT = 0x10,
        VK_CONTROL = 0x11,
        VK_MENU = 0x12, // = ALT
        VK_PAUSE = 0x13,
        VK_CAPITAL = 0x14,
        VK_KANA = 0x15,
        VK_HANGEUL = 0x15,
        VK_HANGUL = 0x15,
        VK_IME_ON = 0x16,
        VK_JUNJA = 0x17,
        VK_FINAL = 0x18,
        VK_HANJA = 0x19,
        VK_KANJI = 0x19,
        VK_IME_OFF = 0x1A,
        VK_ESCAPE = 0x1B,
        VK_CONVERT = 0x1C,
        VK_NONCONVERT = 0x1D,
        VK_ACCEPT = 0x1E,
        VK_MODECHANGE = 0x1F,
        VK_SPACE = 0x20,
        VK_PRIOR = 0x21,
        VK_NEXT = 0x22,
        VK_END = 0x23,
        VK_HOME = 0x24,
        VK_LEFT = 0x25,
        VK_UP = 0x26,
        VK_RIGHT = 0x27,
        VK_DOWN = 0x28,
        VK_SELECT = 0x29,
        VK_PRINT = 0x2A,
        VK_EXECUTE = 0x2B,
        VK_SNAPSHOT = 0x2C,
        VK_INSERT = 0x2D,
        VK_DELETE = 0x2E,
        VK_HELP = 0x2F,
        VK_LWIN = 0x5B,
        VK_RWIN = 0x5C,
        VK_APPS = 0x5D,
        VK_SLEEP = 0x5F,
        VK_NUMPAD0 = 0x60,
        VK_NUMPAD1 = 0x61,
        VK_NUMPAD2 = 0x62,
        VK_NUMPAD3 = 0x63,
        VK_NUMPAD4 = 0x64,
        VK_NUMPAD5 = 0x65,
        VK_NUMPAD6 = 0x66,
        VK_NUMPAD7 = 0x67,
        VK_NUMPAD8 = 0x68,
        VK_NUMPAD9 = 0x69,
        VK_MULTIPLY = 0x6A,
        VK_ADD = 0x6B,
        VK_SEPARATOR = 0x6C,
        VK_SUBTRACT = 0x6D,
        VK_DECIMAL = 0x6E,
        VK_DIVIDE = 0x6F,
        VK_F1 = 0x70,
        VK_F2 = 0x71,
        VK_F3 = 0x72,
        VK_F4 = 0x73,
        VK_F5 = 0x74,
        VK_F6 = 0x75,
        VK_F7 = 0x76,
        VK_F8 = 0x77,
        VK_F9 = 0x78,
        VK_F10 = 0x79,
        VK_F11 = 0x7A,
        VK_F12 = 0x7B,
        VK_F13 = 0x7C,
        VK_F14 = 0x7D,
        VK_F15 = 0x7E,
        VK_F16 = 0x7F,
        VK_F17 = 0x80,
        VK_F18 = 0x81,
        VK_F19 = 0x82,
        VK_F20 = 0x83,
        VK_F21 = 0x84,
        VK_F22 = 0x85,
        VK_F23 = 0x86,
        VK_F24 = 0x87,
        VK_NAVIGATION_VIEW = 0x88,
        VK_NAVIGATION_MENU = 0x89,
        VK_NAVIGATION_UP = 0x8A,
        VK_NAVIGATION_DOWN = 0x8B,
        VK_NAVIGATION_LEFT = 0x8C,
        VK_NAVIGATION_RIGHT = 0x8D,
        VK_NAVIGATION_ACCEPT = 0x8E,
        VK_NAVIGATION_CANCEL = 0x8F,
        VK_NUMLOCK = 0x90,
        VK_SCROLL = 0x91,
        VK_OEM_NEC_EQUAL = 0x92,
        VK_OEM_FJ_JISHO = 0x92,
        VK_OEM_FJ_MASSHOU = 0x93,
        VK_OEM_FJ_TOUROKU = 0x94,
        VK_OEM_FJ_LOYA = 0x95,
        VK_OEM_FJ_ROYA = 0x96,
        VK_LSHIFT = 0xA0,
        VK_RSHIFT = 0xA1,
        VK_LCONTROL = 0xA2,
        VK_RCONTROL = 0xA3,
        VK_LMENU = 0xA4,
        VK_RMENU = 0xA5,
        VK_BROWSER_BACK = 0xA6,
        VK_BROWSER_FORWARD = 0xA7,
        VK_BROWSER_REFRESH = 0xA8,
        VK_BROWSER_STOP = 0xA9,
        VK_BROWSER_SEARCH = 0xAA,
        VK_BROWSER_FAVORITES = 0xAB,
        VK_BROWSER_HOME = 0xAC,
        VK_VOLUME_MUTE = 0xAD,
        VK_VOLUME_DOWN = 0xAE,
        VK_VOLUME_UP = 0xAF,
        VK_MEDIA_NEXT_TRACK = 0xB0,
        VK_MEDIA_PREV_TRACK = 0xB1,
        VK_MEDIA_STOP = 0xB2,
        VK_MEDIA_PLAY_PAUSE = 0xB3,
        VK_LAUNCH_MAIL = 0xB4,
        VK_LAUNCH_MEDIA_SELECT = 0xB5,
        VK_LAUNCH_APP1 = 0xB6,
        VK_LAUNCH_APP2 = 0xB7,
        VK_OEM_1 = 0xBA,   // ';:' for US
        VK_OEM_PLUS = 0xBB,   // '+' any country
        VK_OEM_COMMA = 0xBC,   // ',' any country
        VK_OEM_MINUS = 0xBD,   // '-' any country
        VK_OEM_PERIOD = 0xBE,   // '.' any country
        VK_OEM_2 = 0xBF,   // '/?' for US
        VK_OEM_3 = 0xC0,   // '`~' for US
        VK_GAMEPAD_A = 0xC3,
        VK_GAMEPAD_B = 0xC4,
        VK_GAMEPAD_X = 0xC5,
        VK_GAMEPAD_Y = 0xC6,
        VK_GAMEPAD_RIGHT_SHOULDER = 0xC7,
        VK_GAMEPAD_LEFT_SHOULDER = 0xC8,
        VK_GAMEPAD_LEFT_TRIGGER = 0xC9,
        VK_GAMEPAD_RIGHT_TRIGGER = 0xCA,
        VK_GAMEPAD_DPAD_UP = 0xCB,
        VK_GAMEPAD_DPAD_DOWN = 0xCC,
        VK_GAMEPAD_DPAD_LEFT = 0xCD,
        VK_GAMEPAD_DPAD_RIGHT = 0xCE,
        VK_GAMEPAD_MENU = 0xCF,
        VK_GAMEPAD_VIEW = 0xD0,
        VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON = 0xD1,
        VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON = 0xD2,
        VK_GAMEPAD_LEFT_THUMBSTICK_UP = 0xD3,
        VK_GAMEPAD_LEFT_THUMBSTICK_DOWN = 0xD4,
        VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT = 0xD5,
        VK_GAMEPAD_LEFT_THUMBSTICK_LEFT = 0xD6,
        VK_GAMEPAD_RIGHT_THUMBSTICK_UP = 0xD7,
        VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN = 0xD8,
        VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT = 0xD9,
        VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT = 0xDA,
        VK_OEM_4 = 0xDB,  //  '[{' for US
        VK_OEM_5 = 0xDC,  //  '\|' for US
        VK_OEM_6 = 0xDD,  //  ']}' for US
        VK_OEM_7 = 0xDE,  //  ''"' for US
        VK_OEM_8 = 0xDF,
        VK_OEM_AX = 0xE1,  //  'AX' key on Japanese AX kbd
        VK_OEM_102 = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.
        VK_ICO_HELP = 0xE3,  //  Help key on ICO
        VK_ICO_00 = 0xE4,  //  00 key on ICO
        VK_PROCESSKEY = 0xE5,
        VK_ICO_CLEAR = 0xE6,
        VK_PACKET = 0xE7,
        VK_OEM_RESET = 0xE9,
        VK_OEM_JUMP = 0xEA,
        VK_OEM_PA1 = 0xEB,
        VK_OEM_PA2 = 0xEC,
        VK_OEM_PA3 = 0xED,
        VK_OEM_WSCTRL = 0xEE,
        VK_OEM_CUSEL = 0xEF,
        VK_OEM_ATTN = 0xF0,
        VK_OEM_FINISH = 0xF1,
        VK_OEM_COPY = 0xF2,
        VK_OEM_AUTO = 0xF3,
        VK_OEM_ENLW = 0xF4,
        VK_OEM_BACKTAB = 0xF5,
        VK_ATTN = 0xF6,
        VK_CRSEL = 0xF7,
        VK_EXSEL = 0xF8,
        VK_EREOF = 0xF9,
        VK_PLAY = 0xFA,
        VK_ZOOM = 0xFB,
        VK_NONAME = 0xFC,
        VK_PA1 = 0xFD,
        VK_OEM_CLEAR = 0xFE,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct tagMONITORINFOEX
    {
        public uint cbSize;
        public int rcMonitorLeft, rcMonitorTop, rcMonitorRight, rcMonitorBottom;
        public int rcWorkLeft, rcWorkTop, rcWorkRight, rcWorkBottom;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string deviceName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct tagRECT
    {
        public int left, top, right, bottom;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0, CharSet = CharSet.Unicode)]
    public struct DISPLAY_DEVICEW
    {
        public uint cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    public const int WM_SYSCOMMAND = 0x0112;
    public const int SC_SIZE = 0xF000;
    public const int SC_MOVE = 0xF010;
    public const int SC_MINIMIZE = 0xF020;
    public const int SC_MAXIMIZE = 0xF030;

    public const uint ASFW_ANY = 0xFFFFFFFF; // -1

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct tagPOINT
    {
        public int x, y;
    }

    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput
    public const uint MOUSEEVENTF_MOVE = 0x0001; // Movement occurred.
    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002; // The left button was pressed.
    public const uint MOUSEEVENTF_LEFTUP = 0x0004; // The left button was released.
    public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008; // The right button was pressed.
    public const uint MOUSEEVENTF_RIGHTUP = 0x0010; // The right button was released.
    public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020; // The middle button was pressed.
    public const uint MOUSEEVENTF_MIDDLEUP = 0x0040; // The middle button was released.
    public const uint MOUSEEVENTF_XDOWN = 0x0080; // An X button was pressed.
    public const uint MOUSEEVENTF_XUP = 0x0100; // An X button was released.
    public const uint MOUSEEVENTF_WHEEL = 0x0800; // The wheel was moved, if the mouse has a wheel.The amount of movement is specified in mouseData.
    public const uint MOUSEEVENTF_HWHEEL = 0x1000; // The wheel was moved horizontally, if the mouse has a wheel.The amount of movement is specified in mouseData. Windows XP/2000: This value is not supported.
    public const uint MOUSEEVENTF_MOVE_NOCOALESCE = 0x2000; // The WM_MOUSEMOVE messages will not be coalesced.The default behavior is to coalesce WM_MOUSEMOVE messages. Windows XP/2000: This value is not supported.
    public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000; // Maps coordinates to the entire desktop. Must be used with MOUSEEVENTF_ABSOLUTE.
    public const uint MOUSEEVENTF_ABSOLUTE = 0x8000; // The dx and dy members contain normalized absolute coordinates. If the flag is not set, dxand dy contain relative data (the change in position since the last reported position). This flag can be set, or not set, regardless of what kind of mouse or other pointing device, if any, is connected to the system. For further information about relative mouse motion, see the following Remarks section.

    public const int WHEEL_DELTA = 120;

    const uint MONITOR_DEFAULTTONULL = 0x00000000;
    const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
    const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    public enum WmAppCommandSource
    {
        Key = 0,
        Mouse = 0x8000,
        Oem = 0x1000,
    }

    [Flags]
    public enum WmAppCommandModifiers
    {
        MK_CONTROL = 0x0008, // The CTRL key is down.
        MK_LBUTTON = 0x0001, // The left mouse button is down.
        MK_MBUTTON = 0x0010, // The middle mouse button is down.
        MK_RBUTTON = 0x0002, // The right mouse button is down.
        MK_SHIFT = 0x0004, // The SHIFT key is down.
        MK_XBUTTON1 = 0x0020, // The first X button is down.
        MK_XBUTTON2 = 0x0040, // The second X button is down.
    }

    public const uint WM_APPCOMMAND = 0x0319;
}