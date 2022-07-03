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

    public static void SendText(IntPtr targetApp, Span<Int16> textUTF16)
    {
        SetForegroundWindow(targetApp); // must be activated to ensure we get correct focus target
        System.Threading.Thread.Sleep(50); // Allow window to get activation
        uint processId = 0;
        uint threadId = GetWindowThreadProcessId(targetApp, ref processId);
        GuiThreadInfo guiInfo = new GuiThreadInfo();
        guiInfo.cbSize = (uint)Marshal.SizeOf<GuiThreadInfo>();
        if (GetGUIThreadInfo(threadId, ref guiInfo) == 0) return; // error

        if (guiInfo.hwndFocus == IntPtr.Zero) return;
        uint WM_UNICHAR = 0x0109; // Limited support
        uint WM_CHAR = 0x0102;


        IntPtrToInt64Union wParam = new();
        IntPtrToInt64Union lParam = new();
        lParam.value = 0;
        var lParamData = new KeystrokeFlagsLparam();
        lParamData.repeatCount = 1;
        lParamData.scanCode = 0;
        lParamData.altPressed = false;
        lParamData.wasPressed = false; // match with KEYDOWN
        lParamData.transitionToRelease = false; // match with KEYDOWN
        lParamData.isExtended = false;
        // https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-unichar
        lParam.value += lParamData.repeatCount;
        lParam.value += ((Int64) lParamData.scanCode) << 16;
        lParam.value += ((Int64)(lParamData.isExtended ? 1 : 0)) << 24;
        lParam.value += ((Int64)(lParamData.altPressed ? 1 : 0)) << 29;
        lParam.value += ((Int64)(lParamData.wasPressed ? 1 : 0)) << 30;
        lParam.value += ((Int64)(lParamData.transitionToRelease ? 1 : 0)) << 31;
        for (int i = 0; i < textUTF16.Length; i++)
        {
            wParam.value = textUTF16[i];

            var result = PostMessageW(guiInfo.hwndFocus, WM_CHAR, wParam.ptr, lParam.ptr);
        }
    }

    
}