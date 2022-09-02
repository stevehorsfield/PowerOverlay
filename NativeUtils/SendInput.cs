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
using System.Windows.Threading;
using System.Threading.Tasks;

public partial class NativeUtils
{
    public class InputWrapper
    {
        // These are instance time to handle changes in screen configuration during app operation
        private readonly int virtualScreenLeft, virtualScreenTop, virtualScreenWidth, virtualScreenHeight;
        private readonly int doubleClickTimeMilliseconds;

        private readonly List<tagINPUT> data = new List<tagINPUT>();
        private readonly List<int> delays = new();
        private readonly List<bool> hasData = new();

        private bool awaitingKeyReset;

        public InputWrapper(bool sendKeyReset = true)
        {
            awaitingKeyReset = sendKeyReset;
            const int SM_XVIRTUALSCREEN = 76;
            const int SM_YVIRTUALSCREEN = 77;
            const int SM_CXVIRTUALSCREEN = 78;
            const int SM_CYVIRTUALSCREEN = 79;

            virtualScreenLeft = NativeUtils.GetSystemMetrics(SM_XVIRTUALSCREEN);
            virtualScreenTop = NativeUtils.GetSystemMetrics(SM_YVIRTUALSCREEN);
            virtualScreenWidth = NativeUtils.GetSystemMetrics(SM_CXVIRTUALSCREEN);
            virtualScreenHeight = NativeUtils.GetSystemMetrics(SM_CYVIRTUALSCREEN);

            doubleClickTimeMilliseconds = (int)GetDoubleClickTime();
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

        private void AddMouse(int dx, int dy, uint mouseData, uint flags, int delayBefore)
        {
            tagINPUT i = new tagINPUT();
            i.type = InputType.Mouse;
            i.mouseInput.dx = dx;
            i.mouseInput.dy = dy;
            i.mouseInput.time = 0;
            i.mouseInput.dwFlags = flags;
            i.mouseInput.dwExtraInfo = UIntPtr.Zero;
            i.mouseInput.mouseData = mouseData;

            AddDataInternal(i, delayBefore);
        }

        public void AddMouseLDown(int delayBefore) => AddMouse(0, 0, 0, MOUSEEVENTF_LEFTDOWN, delayBefore);
        public void AddMouseLUp(int delayBefore) => AddMouse(0, 0, 0, MOUSEEVENTF_LEFTUP, delayBefore);
        public void AddMouseLClick(int delayBefore) { AddMouseLDown(delayBefore); AddMouseLUp(0); }
        public void AddMouseLDoubleClick(int delayBefore) { AddMouseLClick(delayBefore); AddMouseLClick(0); }

        public void AddMouseRDown(int delayBefore) => AddMouse(0, 0, 0, MOUSEEVENTF_RIGHTDOWN, delayBefore);
        public void AddMouseRUp(int delayBefore) => AddMouse(0, 0, 0, MOUSEEVENTF_RIGHTUP, delayBefore);
        public void AddMouseRClick(int delayBefore) { AddMouseRDown(delayBefore); AddMouseRUp(0); }
        public void AddMouseRDoubleClick(int delayBefore) { AddMouseRClick(delayBefore); AddMouseRClick(0); }

        public void AddMouseMDown(int delayBefore) => AddMouse(0, 0, 0, MOUSEEVENTF_MIDDLEDOWN, delayBefore);
        public void AddMouseMUp(int delayBefore) => AddMouse(0, 0, 0, MOUSEEVENTF_MIDDLEUP, delayBefore);
        public void AddMouseMClick(int delayBefore) { AddMouseMDown(delayBefore); AddMouseMUp(0); }
        public void AddMouseMDoubleClick(int delayBefore) { AddMouseMClick(delayBefore); AddMouseMClick(0); }

        public void AddMouseHorizontalScroll(int clicks, int delayBefore)
        {
            AddMouse(0, 0, unchecked((uint) (WHEEL_DELTA * clicks)), MOUSEEVENTF_HWHEEL, delayBefore);
        }

        public void AddMouseVerticalScroll(int clicks, int delayBefore)
        {
            AddMouse(0, 0, unchecked((uint)(WHEEL_DELTA * clicks)), MOUSEEVENTF_WHEEL, delayBefore);
        }

        public void AddMouseMove(int absolutePixelLeft, int absolutePixelTop, int delayBefore)
        {
            // absolute mode is used because relative movement is subject to speed-based adjustments
            // in absolute, (0,0) is top left and (65535,65535) is lower right
            // virtualdesk means entire desktop area

            // apply bounds
            if (absolutePixelLeft < virtualScreenLeft) absolutePixelLeft = virtualScreenLeft;
            if (absolutePixelLeft >= (virtualScreenLeft + virtualScreenWidth))
                absolutePixelLeft = virtualScreenLeft + virtualScreenWidth - 1;
            if (absolutePixelTop < virtualScreenTop) absolutePixelTop = virtualScreenTop;
            if (absolutePixelTop >= (virtualScreenTop + virtualScreenHeight))
                absolutePixelTop = virtualScreenTop + virtualScreenHeight - 1;

            // determine offset within virtual screen
            var offsetX = absolutePixelLeft - virtualScreenLeft;
            var offsetY = absolutePixelTop - virtualScreenTop;

            // calculate position in API coordinates
            var pctX = ((double)offsetX) / ((double)virtualScreenWidth);
            var pctY = ((double)offsetY) / ((double)virtualScreenHeight);
            var effectiveX = ((int)Math.Ceiling(pctX * 65535.0));
            var effectiveY = ((int)Math.Ceiling(pctY * 65535.0));

            //System.Diagnostics.Debug.WriteLine(
                //$"Move to {absolutePixelLeft},{absolutePixelTop} as {effectiveX},{effectiveY}");

            AddMouse(effectiveX, effectiveY, 0, MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE | MOUSEEVENTF_VIRTUALDESK | MOUSEEVENTF_MOVE_NOCOALESCE, delayBefore);
        }

        public Point ScreenFromMouseInput(tagINPUT input)
        {
            double dx = input.mouseInput.dx;
            double dy = input.mouseInput.dy;
            double pctX = dx / 65535.0;
            double pctY = dy / 65535.0;
            double offsetX = virtualScreenWidth * pctX;
            double offsetY = virtualScreenHeight * pctY;

            int x = (int)Math.Floor(offsetX + virtualScreenLeft);
            int y = (int)Math.Floor(offsetY + virtualScreenTop);

            //System.Diagnostics.Debug.WriteLine(
            //    $"Translate from {dx},{dy} to {x},{y}");

            return new Point(x, y);
        }

        public void AddMouseDoubleClickBlockWait()
        {
            AddSleep(doubleClickTimeMilliseconds + 20);
        }

        public ReadOnlyMemory<tagINPUT> GetData()
        {
            return data.ToArray().AsMemory();
        }

        public ReadOnlyMemory<int> GetDelays()
        {
            return delays.ToArray().AsMemory();
        }

        public ReadOnlyMemory<bool> GetHasData()
        {
            return hasData.ToArray().AsMemory();
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

    public static async Task<bool> SendKeys(IntPtr hwndTarget, InputWrapper input)
    {
        if (hwndTarget == IntPtr.Zero) return false;
        SetForegroundWindow(hwndTarget); // must be activated to ensure we get correct focus target
        System.Threading.Thread.Sleep(50); // Allow window to get activation

        uint processId = 0;
        uint threadId = GetWindowThreadProcessId(hwndTarget, ref processId);
        uint thisThreadId = GetCurrentThreadId();

        bool[] attached = { false }; // cannot use ref with async, so using a one-element array
        if (thisThreadId != threadId)
        {
            DebugLog.Log($"Attaching input hook");
            attached[0] = true;
            if (AttachThreadInput(thisThreadId, threadId, true) == 0)
            {
                DebugLog.Log($"Failed to attach input hook");
                return false;
            }
        }
        try
        {
            var initialState = new byte[256];
            var updateState = new byte[256];

            unsafe
            {
                fixed (byte* pInitial = &MemoryMarshal.GetReference(initialState.AsSpan()))
                {
                    if (GetKeyboardState(pInitial) == 0) return false;
                }
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
                if (! await SendInputInternal(resetWrapper, threadId, thisThreadId, attached)) return false;
                restoreToggles = true;

                return await SendInputInternal(input, threadId, thisThreadId, attached);
            }
            finally
            {
                unsafe
                {
                    fixed (byte* pInitial = &MemoryMarshal.GetReference(initialState.AsSpan()))
                    {
                        SetKeyboardState(pInitial); // ignore failures for resetting state
                    }
                }
                if (restoreToggles) await SendInputInternal(resetWrapper, threadId, thisThreadId, attached);
            }
        }
        finally
        {
            if (attached[0])
            {
                DebugLog.Log($"Detaching input hook");
                AttachThreadInput(thisThreadId, threadId, false);
            }
        }
    }

    private static async Task<bool> SendInputInternal(InputWrapper input, uint threadId, uint thisThreadId, bool[] attached /* array as cannot use ref with async */)
    {
        var data = input.GetData();
        var delays = input.GetDelays();
        var hasDataFlags = input.GetHasData();

        int index = 0;

        while (index < data.Length)
        {
            if (delays.Span[index] != 0)
            {
                if (attached[0])
                {
                    DebugLog.Log($"Detaching input hook");
                    AttachThreadInput(thisThreadId, threadId, false);
                    attached[0] = false;

                    DebugLog.Log($"Executing delay of {delays.Span[index]}ms");
                    await Sleeper.Sleep(delays.Span[index]);

                    DebugLog.Log($"Attaching input hook");
                    if (AttachThreadInput(thisThreadId, threadId, true) == 0)
                    {
                        DebugLog.Log($"Failed to attach input hook");
                        return false;
                    }
                    attached[0] = true;
                }
                else
                {
                    await Sleeper.Sleep(delays.Span[index]);
                }
            }
            int nextStop = index + 1;

            if (hasDataFlags.Span[index])
            {
                var funcIsMouseMove = (tagINPUT x) =>
                {
                    if (x.type != InputType.Mouse) return false;
                    if ((x.mouseInput.dwFlags & MOUSEEVENTF_MOVE) == MOUSEEVENTF_MOVE) return true;
                    return false;
                };

                if (!funcIsMouseMove(data.Span[index]))
                {
                    while (nextStop < data.Length)
                    {
                        if (delays.Span[nextStop] != 0) break;
                        if (funcIsMouseMove(data.Span[nextStop])) break;
                        ++nextStop;
                    }
                }

                ReadOnlyMemory<tagINPUT> group = data.Slice(index, nextStop - index);

                bool isMouseMove = funcIsMouseMove(data.Span[index]); // handled one at a time

                bool handled = false;

                if (isMouseMove)
                {
                    var cursorPos = GetCursorPosition();
                    if (cursorPos.HasValue)
                    {
                        var targetPos = input.ScreenFromMouseInput(group.Span[0]);

                        int x, y, x0, y0;
                        x = cursorPos.Value.X;
                        y = cursorPos.Value.Y;
                        x0 = (int) targetPos.X;
                        y0 = (int) targetPos.Y;

                        DebugLog.Log(
                            $"Moving from {x},{y} to {x0},{y0}");

                        var wrapper = new InputWrapper(false);
                        while ((x0 != x) || (y0 != y))
                        {
                            if (x0 > x)
                            {
                                x = x + 1;
                                wrapper.AddMouseMove(x, y, 0);
                            }
                            if (x0 < x)
                            {
                                x = x - 1;
                                wrapper.AddMouseMove(x, y, 0);
                            }
                            if (y0 > y)
                            {
                                y = y + 1;
                                wrapper.AddMouseMove(x, y, 0);
                            }
                            if (y0 < y)
                            {
                                y = y - 1;
                                wrapper.AddMouseMove(x, y, 0);
                            }
                        }
                        var moveData = wrapper.GetData();
                        unsafe
                        {
                            fixed (byte* pbMoveData = &MemoryMarshal.GetReference(MemoryMarshal.Cast<tagINPUT, byte>(moveData.Span)))
                            {
                                var result = SendInput((uint)moveData.Length, pbMoveData, Marshal.SizeOf<tagINPUT>());
                                if (result != moveData.Length) return false;
                            }
                        }

                        handled = true;
                    }
                }
                if (! handled)
                {
                    unsafe
                    {
                        fixed (byte* pbData = &MemoryMarshal.GetReference(MemoryMarshal.Cast<tagINPUT, byte>(group.Span)))
                        {
                            var result = SendInput((uint)group.Length, pbData, Marshal.SizeOf<tagINPUT>());
                            if (result != group.Length) return false;
                            DebugLog.Log($"SendInput completed with {group.Length} entries");
                        }
                    }
                }
            }

            index = nextStop;
        }
        return true;
    }    
}