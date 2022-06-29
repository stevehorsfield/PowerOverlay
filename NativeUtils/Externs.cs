namespace overlay_popup;
using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

public partial class NativeUtils
{
    [DllImport("user32.dll")]
    public static extern int ShowWindow(IntPtr hwnd, Win32ShowCmd cmd);

    [DllImport("user32.dll")]
    public static extern int ShowWindowAsync(IntPtr hwnd, Win32ShowCmd cmd);

    [DllImport("user32.dll")]
    public static extern int SetForegroundWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int GetGUIThreadInfo(uint threadId, ref GuiThreadInfo guiInfo);

    [DllImport("user32.dll")]
    private static extern int PostMessageW(IntPtr hwnd, uint messageId, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

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

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hwnd, GetWindowCmd cmd);

    [DllImport("user32.dll")]
    private static extern int GetWindowInfo(IntPtr hwnd, ref tagWINDOWINFO pwi);

    private delegate bool EnumWC(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32.dll")]
    private static extern int EnumWindows(EnumWC lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")]
    public static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    public static extern int SetWindowPos(IntPtr hwnd, IntPtr hwndAfter, int x, int y, int cx, int cy, Win32SetWindowPosFlags flags);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKeyW(uint code, Win32MapVirtualKeyMode mapType);

    [DllImport("user32.dll")]
    public static extern unsafe uint SendInput(uint count, byte* inputs, int cbSize);

    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    public static extern int AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    public static extern unsafe int GetKeyboardState(byte* lpKeyState);
    [DllImport("user32.dll")]
    public static extern unsafe int SetKeyboardState(byte* lpKeyState);

    [DllImport("user32.dll")]
    public static extern short VkKeyScanW(char ch);

}