namespace overlay_popup;

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Input;

public partial class NativeUtils
{
    
    public const int WM_HOTKEY = 0x0312;
    
    private const uint MOD_NONE = 0x0000; //[NONE]
    private const uint MOD_ALT = 0x0001; //ALT
    private const uint MOD_CONTROL = 0x0002; //CTRL
    private const uint MOD_SHIFT = 0x0004; //SHIFT
    private const uint MOD_WIN = 0x0008; //WINDOWS

    // Hotkey registration from: https://blog.magnusmontin.net/2015/03/31/implementing-global-hot-keys-in-wpf/

    public static bool RegisterHotKey(Window window, int hotkeyId, Key key, ModifierKeys modifierKeys)
    {
        IntPtr handle = new WindowInteropHelper(window).Handle;

        uint modifiers = 0;
        if (modifierKeys.HasFlag(ModifierKeys.Windows)) modifiers |= MOD_WIN;
        if (modifierKeys.HasFlag(ModifierKeys.Control)) modifiers |= MOD_CONTROL;
        if (modifierKeys.HasFlag(ModifierKeys.Shift)) modifiers |= MOD_SHIFT;
        if (modifierKeys.HasFlag(ModifierKeys.Alt)) modifiers |= MOD_ALT;

        return RegisterHotKey(handle, hotkeyId, modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));
    }
}