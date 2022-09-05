using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace PowerOverlay;

public partial class NativeUtils
{
    public static int GetMonitorCount()
    {
        const int SM_CMONITORS = 80;
        return GetSystemMetrics(SM_CMONITORS);
    }

    public struct DisplayInfo
    {
        public Rect clientRect;
        public Rect monitorRect;
        public bool isPrimary;
        public int monitorIndex;
        public IntPtr hMonitor;
    }

    public static List<DisplayInfo>? GetDisplayCoordinates()
    {
        uint deviceCount = 0;
        List<DISPLAY_DEVICEW> devices = new();
        List<Tuple<uint, DISPLAY_DEVICEW>> monitors = new();

        while (true)
        {
            DISPLAY_DEVICEW device = new DISPLAY_DEVICEW();
            device.cb = (uint)Marshal.SizeOf<DISPLAY_DEVICEW>();
            if (EnumDisplayDevicesW(null, deviceCount, ref device, 0) == 0) break;

            devices.Add(device);

            uint monitorCount = 0;
            while (true)
            {
                DISPLAY_DEVICEW monitor = new DISPLAY_DEVICEW();
                monitor.cb = (uint)Marshal.SizeOf<DISPLAY_DEVICEW>();

                if (EnumDisplayDevicesW(device.DeviceName, monitorCount, ref monitor, 0) == 0) break;

                monitors.Add(Tuple.Create(deviceCount, monitor));

                ++monitorCount;
            }
            ++deviceCount;
        }

        var monitorInfos = new List<Tuple<string, int, IntPtr, tagMONITORINFOEX>>();
        int monitorIndex = 0;

        MonitorEnumProc enumProc = (IntPtr hMonitor, IntPtr hDC, ref tagRECT intersectOrMonitorRect, IntPtr dwData) =>
        {
            tagMONITORINFOEX monitorInfo = new();
            monitorInfo.cbSize = (uint) Marshal.SizeOf<tagMONITORINFOEX>();

            if (GetMonitorInfoW(hMonitor, ref monitorInfo) != 0)
            {
                monitorInfos.Add(Tuple.Create(monitorInfo.deviceName, monitorIndex, hMonitor, monitorInfo));
                ++monitorIndex;
            };

            return true;
        };
        unsafe
        {
            if (EnumDisplayMonitors(IntPtr.Zero, null, enumProc, IntPtr.Zero) == 0) return null;
        }

        var result = new List<DisplayInfo>();

        var monitorList =
            devices.SelectMany(
            (d, i) => monitors.Where(x => x.Item1 == i).Select(m => Tuple.Create(i, d, m.Item1, m.Item2))
            ).ToArray();

        int index = 0;
        foreach (var monitor in monitorList)
        {
            const uint DISPLAY_DEVICE_ACTIVE = 0x00000001;
            const uint DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004;
            const uint DISPLAY_DEVICE_MIRRORING_DRIVER = 0x00000008;
            if ((monitor.Item2.StateFlags & DISPLAY_DEVICE_ACTIVE) == 0) continue;
            if ((monitor.Item2.StateFlags & DISPLAY_DEVICE_MIRRORING_DRIVER) != 0) continue;
            bool isPrimaryDevice = ((monitor.Item2.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0);
            var info = monitorInfos.FirstOrDefault(x => String.Equals(monitor.Item2.DeviceName, x.Item1, StringComparison.Ordinal));
            if (info == null) continue;

            result.Add(new DisplayInfo()
            {
                isPrimary = isPrimaryDevice && monitor.Item3 == 0,
                clientRect = new Rect(info.Item4.rcWorkLeft, info.Item4.rcWorkTop, 
                    info.Item4.rcWorkRight - info.Item4.rcWorkLeft,
                    info.Item4.rcWorkBottom - info.Item4.rcWorkTop
                    ),
                monitorRect = new Rect(info.Item4.rcMonitorLeft, info.Item4.rcMonitorTop, 
                info.Item4.rcMonitorRight - info.Item4.rcMonitorLeft,
                    info.Item4.rcMonitorBottom - info.Item4.rcMonitorTop
                    ),
                monitorIndex = index,
                hMonitor = info.Item3,
            });

            ++index;
        }
        if (index == 0) return null;
        return result;
    }

    public static IntPtr MonitorFromWindowOrPrimary(IntPtr hwnd)
    {

        return MonitorFromWindow(hwnd, MONITOR_DEFAULTTOPRIMARY);
    }

    public static IntPtr MonitorFromPoint(Point p)
    {
        tagPOINT np = new tagPOINT()
        {
            x = (int) p.X,
            y = (int) p.Y,
        };
        IntPtr hMon = MonitorFromPoint(np, MONITOR_DEFAULTTONEAREST);
        return hMon;
    }
}

