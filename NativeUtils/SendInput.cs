namespace PowerOverlay;

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using System.Windows.Interop;
using System.Windows;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public partial class NativeUtils
{
    public class InputWrapper
    {
        private readonly List<tagINPUT> data = new List<tagINPUT>();
        private readonly List<int> delays = new();
        private readonly List<bool> hasData = new();

        private bool awaitingKeyReset;

        public InputWrapper(bool sendKeyReset = true)
        {
            awaitingKeyReset = sendKeyReset;
        }

        private void AddDataInternal(tagINPUT input, int delayBefore)
        {
            if (delayBefore < 0) throw new ArgumentException("Argument cannot be negative", nameof(delayBefore));

            data.Add(input);
            delays.Add(delayBefore);
            hasData.Add(true);
        }

        public void AddSleep(int delay)
        {
            if (delay == 0) return;
            if (delay < 0) throw new ArgumentException("Argument cannot be negative", nameof(delay));
            data.Add(new tagINPUT());
            delays.Add(delay);
            hasData.Add(false);
        }

        private void AddKeyModifierClear(int delayBefore)
        {
            awaitingKeyReset = false;
            AddKeyInternal(true, false, Win32VirtualKey.VK_LSHIFT, delayBefore);
            AddKeyInternal(true, false, Win32VirtualKey.VK_LCONTROL, 0);
            AddKeyInternal(true, false, Win32VirtualKey.VK_LMENU, 0);
            AddKeyInternal(true, false, Win32VirtualKey.VK_LWIN, 0);
            AddKeyInternal(true, false, Win32VirtualKey.VK_RSHIFT, 0);
            AddKeyInternal(true, false, Win32VirtualKey.VK_RCONTROL, 0);
            AddKeyInternal(true, false, Win32VirtualKey.VK_RMENU, 0);
            AddKeyInternal(true, false, Win32VirtualKey.VK_RWIN, 0);
        }

        private void AddKeyInternal(bool up, bool extended, Win32VirtualKey vk, int delayBefore)
        {
            tagINPUT i = new tagINPUT();

            i.type = InputType.Keyboard;
            i.keyInput.wVk = (ushort)vk;
            i.keyInput.time = (uint)(DateTimeOffset.Now.ToUnixTimeMilliseconds() + data.Count);
            i.keyInput.wScan = (ushort)MapVirtualKeyW((ushort)vk, Win32MapVirtualKeyMode.VkToScan);
            i.keyInput.dwFlags = 0;
            //if (i.keyInput.wScan != 0)
            //{
            //    i.keyInput.wVk = 0;
            //    i.keyInput.dwFlags |= Win32KeyboardInputFlags.ScanCode;
            //}
            if (((i.keyInput.wScan & 0xFF00) == 0xe000) || ((i.keyInput.wScan & 0xFF00) == 0xe100))
                extended = true;
            if (up) i.keyInput.dwFlags |= Win32KeyboardInputFlags.KeyUp;
            if (extended) i.keyInput.dwFlags |= Win32KeyboardInputFlags.ExtendedKey;

            i.keyInput.dwExtraInfo = UIntPtr.Zero;

            AddDataInternal(i, delayBefore);
        }

        private void AddKey(bool up, bool extended, Win32VirtualKey vk, int delayBefore)
        {
            if (awaitingKeyReset)
            {
                AddKeyModifierClear(delayBefore);
                delayBefore = 0;
            }
            AddKeyInternal(up, extended, vk, delayBefore);
        }

        public void AddKeyUp(Win32VirtualKey vk, bool extended, int delayBefore) => AddKey(true, extended, vk, delayBefore);
        public void AddKeyDown(Win32VirtualKey vk, bool extended, int delayBefore) => AddKey(false, extended, vk, delayBefore);
        public void AddKeyPress(Win32VirtualKey vk, bool extended, int delayBefore) {
            AddKey(false, extended, vk, delayBefore);
            AddKey(true, extended, vk, 0);
        }

        public bool AddKeyDown(char c, int delayBefore) => AddKey(false, c, delayBefore);
        public bool AddKeyUp(char c, int delayBefore) => AddKey(true, c, delayBefore);

        public bool AddKey(bool up, char c, int delayBefore)
        {
            var vk = (byte)(VkKeyScanW(c) & 0xFF);
            if (vk == 0xFF) return false;
            AddKey(up, false, (Win32VirtualKey)vk, delayBefore);
            return true;
        }
        public bool AddKeyPress(char c, int delayBefore)
        {
            return AddKey(false, c, delayBefore) && AddKey(true, c, 0);
        }

        public static bool IsValidChar(char c)
        {
            var vk = (byte)(VkKeyScanW(c) & 0xFF);
            return vk != 0xFF;
        }

        public Span<tagINPUT> UnsafeGetData()
        {
            return CollectionsMarshal.AsSpan(data);
        }

        public Span<int> UnsafeGetDelays()
        {
            return CollectionsMarshal.AsSpan(delays);
        }

        public Span<bool> UnsafeGetHasData()
        {
            return CollectionsMarshal.AsSpan(hasData);
        }
    }

    // Initial implementation. Some keys do not work if, for instance, NUM LOCK is on.
    //public static unsafe bool SendKeys(InputWrapper input)
    //{
    //    var data = input.UnsafeGetData();

    //    fixed (byte* pbData = &MemoryMarshal.GetReference(MemoryMarshal.Cast<tagINPUT, byte>(data)))
    //    {
    //        var result = SendInput((uint) data.Length, pbData, Marshal.SizeOf<tagINPUT>());
    //        return result == data.Length;
    //    }
    //}

    public static unsafe bool SendKeys(IntPtr hwndTarget, InputWrapper input)
    {
        if (hwndTarget == IntPtr.Zero) return false;
        SetForegroundWindow(hwndTarget); // must be activated to ensure we get correct focus target
        System.Threading.Thread.Sleep(50); // Allow window to get activation

        uint processId = 0;
        uint threadId = GetWindowThreadProcessId(hwndTarget, ref processId);
        uint thisThreadId = GetCurrentThreadId();

        if (AttachThreadInput(thisThreadId, threadId, true) == 0) return false;
        try
        {
            var initialState = new byte[256];
            var updateState = new byte[256];

            fixed (byte* pInitial = &MemoryMarshal.GetReference(initialState.AsSpan()))
            {
                if (GetKeyboardState(pInitial) == 0) return false;
            }
            Array.Copy(initialState, updateState, initialState.Length);

            var resetWrapper = new InputWrapper(false);
            var isVKPressed = (Win32VirtualKey vk) => (initialState[(byte)vk] & 0x80) == 0x80;
            var isVKToggled = (Win32VirtualKey vk) => (initialState[(byte)vk] & 0x01) == 0x01;

            if (isVKToggled(Win32VirtualKey.VK_CAPITAL)) resetWrapper.AddKeyPress(Win32VirtualKey.VK_CAPITAL, false, 0);
            if (isVKToggled(Win32VirtualKey.VK_NUMLOCK)) resetWrapper.AddKeyPress(Win32VirtualKey.VK_NUMLOCK, true, 0);
            if (isVKToggled(Win32VirtualKey.VK_SCROLL)) resetWrapper.AddKeyPress(Win32VirtualKey.VK_SCROLL, true, 0);

            bool restoreToggles = false;

            try
            {
                if (!SendInputInternal(resetWrapper)) return false;
                restoreToggles = true;

                return SendInputInternal(input);
            }
            finally
            {
                fixed (byte* pInitial = &MemoryMarshal.GetReference(initialState.AsSpan()))
                {
                    SetKeyboardState(pInitial); // ignore failures for resetting state
                }
                if (restoreToggles) SendInputInternal(resetWrapper);
            }
        }
        finally
        {
            AttachThreadInput(thisThreadId, threadId, false);
        }
    }

    private static unsafe bool SendInputInternal(InputWrapper input)
    {
        var data = input.UnsafeGetData();
        var delays = input.UnsafeGetDelays();
        var hasDataFlags = input.UnsafeGetHasData();

        int index = 0;

        while (index < data.Length)
        {
            if (delays[index] != 0)
            {
                Thread.Sleep(delays[index]);
            }
            int nextStop = index + 1;

            if (hasDataFlags[index])
            {
                while (nextStop < data.Length)
                {
                    if (delays[nextStop] != 0) break;
                    ++nextStop;
                }

                Span<tagINPUT> group = data.Slice(index, nextStop - index);


                fixed (byte* pbData = &MemoryMarshal.GetReference(MemoryMarshal.Cast<tagINPUT, byte>(group)))
                {
                    var result = SendInput((uint)group.Length, pbData, Marshal.SizeOf<tagINPUT>());
                    if (result != group.Length) return false;
                    System.Diagnostics.Debug.WriteLine($"SendInput completed with {group.Length} entries");
                }
            }

            index = nextStop;
        }
        return true;
    }

    // INVALID APPROACH.
    // Sending via WM_KEYUP/DOWN etc. Doesn't manage thread modifier state and isn't interpreted correctly by apps.
    //public static bool SendKeys(IntPtr hwndTarget, InputWrapper input)
    //{
    //    if (hwndTarget == IntPtr.Zero) return false;

    //    SetForegroundWindow(hwndTarget); // must be activated to ensure we get correct focus target
    //    System.Threading.Thread.Sleep(50); // Allow window to get activation

    //    uint processId = 0;
    //    uint threadId = GetWindowThreadProcessId(hwndTarget, ref processId);
    //    GuiThreadInfo guiInfo = new GuiThreadInfo();
    //    guiInfo.cbSize = (uint)Marshal.SizeOf<GuiThreadInfo>();
    //    if (GetGUIThreadInfo(threadId, ref guiInfo) == 0) return false; // error

    //    bool sendAsSysKeys = guiInfo.hwndFocus == IntPtr.Zero;
    //    IntPtr sendTarget = guiInfo.hwndFocus != IntPtr.Zero ? guiInfo.hwndFocus : hwndTarget;

    //    bool leftAltPressed = false;
    //    bool rightAltPressed = false;

    //    var data = input.UnsafeGetData();
    //    foreach (var ki in data)
    //    {
    //        if (ki.type != InputType.Keyboard) throw new NotSupportedException("Non-keyboard input in SendKeys");

    //        bool isKeyUp = ki.keyInput.dwFlags.HasFlag(Win32KeyboardInputFlags.KeyUp);
    //        bool isLeftAlt = ki.keyInput.wVk == (ushort)Win32VirtualKey.VK_LMENU;
    //        bool isRightAlt = ki.keyInput.wVk == (ushort)Win32VirtualKey.VK_RMENU;
    //        bool isF10 = ki.keyInput.wVk == (ushort)Win32VirtualKey.VK_F10;

    //        bool isSysKey = isLeftAlt || isRightAlt || isF10 || sendAsSysKeys;

    //        const uint WM_KEYDOWN = 0x0100;
    //        const uint WM_KEYUP = 0x0101;
    //        const uint WM_SYSKEYDOWN = 0x0104;
    //        const uint WM_SYSKEYUP = 0x0105;

    //        if (isLeftAlt) leftAltPressed = !isKeyUp;
    //        if (isRightAlt) rightAltPressed = !isKeyUp;

    //        bool isExtended = (Win32VirtualKey) ki.keyInput.wVk switch
    //        {
    //            Win32VirtualKey.VK_RCONTROL => true,
    //            Win32VirtualKey.VK_RMENU => true,
    //            Win32VirtualKey.VK_INSERT => true,
    //            Win32VirtualKey.VK_DELETE => true,
    //            Win32VirtualKey.VK_HOME => true,
    //            Win32VirtualKey.VK_END => true,
    //            Win32VirtualKey.VK_NEXT => true,
    //            Win32VirtualKey.VK_PRIOR => true,
    //            Win32VirtualKey.VK_LEFT => true,
    //            Win32VirtualKey.VK_RIGHT => true,
    //            Win32VirtualKey.VK_UP => true,
    //            Win32VirtualKey.VK_DOWN => true,
    //            Win32VirtualKey.VK_NUMLOCK => true,
    //            Win32VirtualKey.VK_PRINT => true,
    //            Win32VirtualKey.VK_PAUSE => true,
    //            Win32VirtualKey.VK_DIVIDE => true,
    //            _ => false
    //        };
                
    //        bool altDown = leftAltPressed || rightAltPressed;

    //        var msgType = (isSysKey) ? 
    //            ((isKeyUp) ? WM_SYSKEYUP : WM_SYSKEYDOWN) 
    //            : (isKeyUp ? WM_KEYUP : WM_KEYDOWN);

    //        IntPtrToInt64Union wParam = new IntPtrToInt64Union();
    //        IntPtrToInt64Union lParam = new IntPtrToInt64Union();

    //        var vkActual = ki.keyInput.wVk;
    //        vkActual = ((Win32VirtualKey)vkActual) switch
    //        {
    //            Win32VirtualKey.VK_LSHIFT => (ushort) Win32VirtualKey.VK_SHIFT,
    //            Win32VirtualKey.VK_RSHIFT => (ushort) Win32VirtualKey.VK_SHIFT,
    //            Win32VirtualKey.VK_LCONTROL => (ushort) Win32VirtualKey.VK_CONTROL,
    //            Win32VirtualKey.VK_RCONTROL => (ushort) Win32VirtualKey.VK_CONTROL,
    //            Win32VirtualKey.VK_LMENU => (ushort) Win32VirtualKey.VK_MENU,
    //            Win32VirtualKey.VK_RMENU => (ushort) Win32VirtualKey.VK_MENU,
    //            _ => vkActual
    //        };

    //        var scanActual = ki.keyInput.wScan;
    //        if (vkActual != ki.keyInput.wVk)
    //        {
    //            scanActual = (ushort) MapVirtualKeyW(vkActual, isExtended ? Win32MapVirtualKeyMode.VkToScanEx : Win32MapVirtualKeyMode.VkToScan);
    //        }

    //        // https://docs.microsoft.com/en-us/windows/win32/inputdev/about-keyboard-input#keystroke-message-flags
    //        wParam.value = vkActual;
    //        lParam.value = 0;
    //        lParam.value += 1 << 32; // repeat count = 1
    //        lParam.value += (isKeyUp) ? 0x80000000 : 0; // is up
    //        lParam.value += (leftAltPressed || rightAltPressed) ? 0x20000000 : 0;
    //        lParam.value += (isExtended) ? 0x01000000 : 0;
    //        lParam.value += (isKeyUp) ? 0x40000000 : 0; // was down
    //        lParam.value += (ki.keyInput.wScan != 0) ? ((uint)(byte)scanActual) << 48 : 0;

    //        if (PostMessageW(sendTarget, msgType, wParam.ptr, lParam.ptr) == 0) return false;
    //        Thread.Sleep(10);
    //    }
    //    return true;
    //}
    
}