using System;

namespace PowerOverlay;

public partial class NativeUtils {

    public static bool SendWmAppCommand(IntPtr hwnd, int command, WmAppCommandSource source, WmAppCommandModifiers modifiers)
    {
        IntPtrToInt64Union wParam;
        IntPtrToInt64Union lParam;

        // Address code warnings (storage is overlapped, so no effect here)
        wParam.ptr = IntPtr.Zero;
        lParam.ptr = IntPtr.Zero;

        // Note IntPtr is 64-bit, but only lower 32-bits are used.
        // Top 16-bits are command and source mask
        // Lower 16-bits are modifier keys

        wParam.ptr = hwnd;
        lParam.value =
            ( ((uint)(command << 16)) + ((((uint)source) << 16) << 16) ) // upper 16-bits
            + ((uint)modifiers); // lower 16-bits

        if (PostMessageW(hwnd, WM_APPCOMMAND, wParam.ptr, lParam.ptr) == 0) return false;
        return true;

        /*
        From WinUser.h

            #define FAPPCOMMAND_MOUSE 0x8000
            #define FAPPCOMMAND_KEY   0
            #define FAPPCOMMAND_OEM   0x1000
            #define FAPPCOMMAND_MASK  0xF000

            #define GET_APPCOMMAND_LPARAM(lParam) ((short)(HIWORD(lParam) & ~FAPPCOMMAND_MASK))
            #define GET_DEVICE_LPARAM(lParam)     ((WORD)(HIWORD(lParam) & FAPPCOMMAND_MASK))
            #define GET_MOUSEORKEY_LPARAM         GET_DEVICE_LPARAM
            #define GET_FLAGS_LPARAM(lParam)      (LOWORD(lParam))
            #define GET_KEYSTATE_LPARAM(lParam)   GET_FLAGS_LPARAM(lParam)
         */
    }
}