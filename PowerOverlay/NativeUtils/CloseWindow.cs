using System;

namespace PowerOverlay;

public partial class NativeUtils
{
    public static void SendCloseMessage(IntPtr hwnd)
    {
        const uint WM_CLOSE = 0x0010;

        PostMessageW(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    public static void SendQuitMessage(IntPtr hwnd)
    {
        const uint WM_QUIT = 0x0012;

        uint processId = 0;
        uint threadId = GetWindowThreadProcessId(hwnd, ref processId);

        PostThreadMessageW(threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
    }
}