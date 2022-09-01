using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PowerOverlay.Commands;

public enum MoveCursorOrigin
{
    Absolute,
    RelativeMonitor,
    RelativeWindow,
    RelativeCursor
}

public enum MoveCursorAnchor
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    CenterPoint,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
}

public enum MouseActionType
{
    Move,

    LeftClick,
    LeftDoubleClick,

    RightClick,
    RightDoubleClick,

    MiddleClick,
    MiddleDoubleClick,

    LeftDown,
    LeftUp,

    RightDown,
    RightUp,

    MiddleDown,
    MiddleUp,

    VerticalWheel,
    HorizontalWheel,

    BlockDoubleClickPause,
    ReleaseModifiers,

    Sleep,
}

public class SendMouseAction : INotifyPropertyChanged, IApplicationJson
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(params string[] propNames)
    {
        foreach (var prop in propNames)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    private MouseActionType action;
    public MouseActionType Action
    {
        get { return action; }
        set
        {
            action = value;
            RaisePropertyChanged(
                nameof(Action),
                nameof(CoordinatesEnabled),
                nameof(ModifiersEnabled),
                nameof(MovementRelativeEnabled),
                nameof(DisplayValue),
                nameof(MouseActionIndex),
                nameof(WheelDeltaEnabled)
                );
        }
    }

    private readonly List<string> mouseActions = Enum.GetNames<MouseActionType>().ToList();
    public IEnumerable<string> MouseActions => mouseActions;

    public int MouseActionIndex
    {
        get { return mouseActions.IndexOf(action.ToString()); }
        set
        {
            if (value >= 0 && value < mouseActions.Count)
            {
                Action = Enum.Parse<MouseActionType>(mouseActions[value]);
            }
            else
            {
                RaisePropertyChanged(nameof(MouseActionIndex)); // action unchanged
            }
        }
    }

    public bool CoordinatesEnabled => Action == MouseActionType.Move;
    public bool ModifiersEnabled => Action != MouseActionType.BlockDoubleClickPause && Action != MouseActionType.ReleaseModifiers && Action != MouseActionType.Sleep;
    public bool SleepEnabled => Action != MouseActionType.BlockDoubleClickPause;
    public bool MovementRelativeEnabled => Action == MouseActionType.Move;
    public bool WheelDeltaEnabled => Action == MouseActionType.VerticalWheel || Action == MouseActionType.HorizontalWheel;

    #region Modifier accessors and event helper
    private SendKeyModifierFlags modifiers;

    private void UpdateModifier(bool isSet, SendKeyModifierFlags mask)
    {
        var val = modifiers;
        val = val & (~mask);
        if (isSet) val |= mask;
        Modifiers = val;
    }

    public SendKeyModifierFlags Modifiers
    {
        get { return modifiers; }
        set { modifiers = value; RaisePropertyChangedModifiers(); }
    }
    public bool LeftShift
    {
        get { return Modifiers.HasFlag(SendKeyModifierFlags.LeftShift); }
        set { UpdateModifier(value, SendKeyModifierFlags.LeftShift); }
    }
    public bool RightShift
    {
        get { return Modifiers.HasFlag(SendKeyModifierFlags.RightShift); }
        set { UpdateModifier(value, SendKeyModifierFlags.RightShift); }
    }
    public bool LeftControl
    {
        get { return Modifiers.HasFlag(SendKeyModifierFlags.LeftControl); }
        set { UpdateModifier(value, SendKeyModifierFlags.LeftControl); }
    }
    public bool RightControl
    {
        get { return Modifiers.HasFlag(SendKeyModifierFlags.RightControl); }
        set { UpdateModifier(value, SendKeyModifierFlags.RightControl); }
    }
    public bool LeftAlt
    {
        get { return Modifiers.HasFlag(SendKeyModifierFlags.LeftAlt); }
        set { UpdateModifier(value, SendKeyModifierFlags.LeftAlt); }
    }
    public bool RightAlt
    {
        get { return Modifiers.HasFlag(SendKeyModifierFlags.RightAlt); }
        set { UpdateModifier(value, SendKeyModifierFlags.RightAlt); }
    }
    public bool LeftWindows
    {
        get { return Modifiers.HasFlag(SendKeyModifierFlags.LeftWindows); }
        set { UpdateModifier(value, SendKeyModifierFlags.LeftWindows); }
    }
    public bool RightWindows
    {
        get { return Modifiers.HasFlag(SendKeyModifierFlags.RightWindows); }
        set { UpdateModifier(value, SendKeyModifierFlags.RightWindows); }
    }

    private void RaisePropertyChangedModifiers()
    {
        RaisePropertyChanged(
          nameof(DisplayValue)
        , nameof(ModifiersDisplayValue)
        , nameof(Modifiers)
        , nameof(LeftShift)
        , nameof(RightShift)
        , nameof(LeftControl)
        , nameof(RightControl)
        , nameof(LeftAlt)
        , nameof(RightAlt)
        , nameof(LeftWindows)
        , nameof(RightWindows)
        );
    }
    #endregion


    public int MinCoordinateValue => -65535;
    public int MaxCoordinateValue => 65535;

    public int MinWheelDelta => -1000;
    public int MaxWheelDelta => 1000;

    private int x, y, wheelDelta, sleepDelayBeforeMilliseconds;
    private bool isMovementRelative;

    public int X
    {
        get { return x; }
        set
        {
            if (value < MinCoordinateValue) value = MinCoordinateValue;
            if (value > MaxCoordinateValue) value = MaxCoordinateValue;
            x = value;
            RaisePropertyChanged(nameof(X), nameof(DisplayValue));
        }
    }
    public int Y
    {
        get { return y; }
        set
        {
            if (value < MinCoordinateValue) value = MinCoordinateValue;
            if (value > MaxCoordinateValue) value = MaxCoordinateValue;
            y = value;
            RaisePropertyChanged(nameof(Y), nameof(DisplayValue));
        }
    }

    public int WheelDelta
    {
        get { return wheelDelta; }
        set
        {
            wheelDelta = value;
            RaisePropertyChanged(nameof(WheelDelta), nameof(DisplayValue));
        }
    }

    public bool IsMovementRelative
    {
        get { return isMovementRelative; }
        set { isMovementRelative = value; RaisePropertyChanged(nameof(IsMovementRelative), nameof(DisplayValue)); }
    }

    public int SleepDelayBeforeMilliseconds
    {
        get { return sleepDelayBeforeMilliseconds; }
        set
        {
            sleepDelayBeforeMilliseconds = value;
            RaisePropertyChanged(nameof(SleepDelayBeforeMilliseconds), nameof(SleepDisplay), nameof(DisplayValue));
        }
    }


    public string ModifiersDisplayValue
    {
        get
        {
            StringBuilder b = new StringBuilder();
            if (LeftShift) b.Append("LShift|");
            if (RightShift) b.Append("RShift|");
            if (LeftControl) b.Append("LCtrl|");
            if (RightControl) b.Append("RCtrl|");
            if (LeftAlt) b.Append("LAlt|");
            if (RightAlt) b.Append("RAlt|");
            if (LeftWindows) b.Append("LWin|");
            if (RightWindows) b.Append("RWin|");
            if (b.Length > 0) b.Length = b.Length - 1;
            return b.ToString();
        }
    }

    public string SleepDisplay
    {
        get
        {
            if (!SleepEnabled) return String.Empty;
            return sleepDelayBeforeMilliseconds == 0 ? String.Empty : $"(+ {sleepDelayBeforeMilliseconds}ms) ";
        }
    }

    public string DisplayValue
    {
        get
        {
            switch (Action)
            {
                case MouseActionType.Move:
                    if (IsMovementRelative)
                    {
                        var xSign = X > 0 ? "+" : String.Empty;
                        var ySign = Y > 0 ? "+" : String.Empty;
                        return $"{SleepDisplay}Move ({xSign}{X},{ySign}{Y}) ({ModifiersDisplayValue})";
                    } else
                    {
                        return $"{SleepDisplay}Move ({X},{Y}) ({ModifiersDisplayValue})";
                    }
                case MouseActionType.BlockDoubleClickPause:
                    return Action.ToString();
                case MouseActionType.ReleaseModifiers:
                    return $"{SleepDisplay}{Action.ToString()}";
                case MouseActionType.HorizontalWheel:
                    return $"{SleepDisplay}{Action.ToString()} (delta:{WheelDelta}) ({ModifiersDisplayValue})";
                case MouseActionType.VerticalWheel:
                    return $"{SleepDisplay}{Action.ToString()} (delta:{WheelDelta}) ({ModifiersDisplayValue})";
                default:
                    return $"{SleepDisplay}{Action.ToString()} ({ModifiersDisplayValue})";
            }
        }
    }

    public SendMouseAction Clone()
    {
        return new SendMouseAction
        {
            X = X,
            Y = Y,
            IsMovementRelative = IsMovementRelative,
            SleepDelayBeforeMilliseconds = SleepDelayBeforeMilliseconds,
            WheelDelta = WheelDelta,
            Action = Action,
            Modifiers = Modifiers,
        };
    }

    public JsonNode ToJson()
    {
        var o = new JsonObject();
        o.AddLowerCamelValue(nameof(Action), Action.ToString());
        if (ModifiersEnabled)
        {
            var flags = new List<JsonValue>();
            foreach (var flag in Enum.GetValues<SendKeyModifierFlags>())
            {
                if (flag == 0) continue;
                if (Modifiers.HasFlag(flag))
                    flags.Add(JsonValue.Create(Enum.GetName<SendKeyModifierFlags>(flag)!.ToLowerCamelCase())!);
            }
            if (flags.Count > 0) o.AddLowerCamel(nameof(SendMouseAction.Modifiers), new JsonArray(flags.ToArray()));
        }
        if (CoordinatesEnabled)
        {
            o.AddLowerCamelValue(nameof(X), X);
            o.AddLowerCamelValue(nameof(Y), Y);
        }
        if (WheelDeltaEnabled)
        {
            o.AddLowerCamelValue(nameof(WheelDelta), WheelDelta);
        }
        if (SleepEnabled)
        {
            o.AddLowerCamelValue(nameof(SleepDelayBeforeMilliseconds), SleepDelayBeforeMilliseconds);
        }
        if (MovementRelativeEnabled)
        {
            o.AddLowerCamelValue(nameof(IsMovementRelative), IsMovementRelative);
        }
        return o;
    }

    public static SendMouseAction FromJson(JsonNode n)
    {
        var o = n.AsObject()!;
        var result = new SendMouseAction();
        o.TryGet<string>(nameof(Action), a =>
        {
            result.Action = Enum.Parse<MouseActionType>(a, true);
        });
        if (result.ModifiersEnabled)
        {
            o.TryGet<JsonArray>(nameof(SendMouseAction.Modifiers), ms =>
            {
                SendKeyModifierFlags flags = new SendKeyModifierFlags();
                foreach (var m in ms)
                {
                    var f = Enum.Parse<SendKeyModifierFlags>(m.GetValue<string>(), true);
                    flags |= f;
                }
                result.Modifiers = flags;
            });
        }
        if (result.CoordinatesEnabled)
        {
            o.TryGetValue<int>(nameof(X), d => result.X = d);
            o.TryGetValue<int>(nameof(Y), d => result.Y = d);
        }
        if (result.WheelDeltaEnabled)
        {
            o.TryGetValue<int>(nameof(WheelDelta), d => result.WheelDelta = d);
        }
        if (result.SleepEnabled)
        {
            o.TryGetValue<int>(nameof(SleepDelayBeforeMilliseconds), d => result.SleepDelayBeforeMilliseconds = d);
        }
        if (result.MovementRelativeEnabled)
        {
            o.TryGetValue<bool>(nameof(IsMovementRelative), b => result.IsMovementRelative = b);
        }
        return result;
    }
}

public class SendMouse : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return SendMouseDefinition.Instance; } }

    private readonly FrameworkElement configElement = SendMouseDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    private MoveCursorOrigin cursorOrigin;
    public MoveCursorOrigin CursorOrigin
    {
        get { return cursorOrigin; }
        set
        {
            cursorOrigin = value;
            RaisePropertyChanged(nameof(CursorOrigin));
            RaisePropertyChanged(nameof(IsAbsolute));
            RaisePropertyChanged(nameof(IsMonitorRelative));
            RaisePropertyChanged(nameof(IsWindowRelative));
            RaisePropertyChanged(nameof(IsCursorRelative));
            RaisePropertyChanged(nameof(TargetAppEnabled));
            RaisePropertyChanged(nameof(IsAnchorEnabled));
        }
    }
    private MoveCursorAnchor cursorAnchor;
    public MoveCursorAnchor CursorAnchor
    {
        get { return cursorAnchor; }
        set
        {
            cursorAnchor = value;
            RaisePropertyChanged(nameof(CursorAnchor));
            RaisePropertyChanged(nameof(IsAnchorTopLeft));
            RaisePropertyChanged(nameof(IsAnchorTopCenter));
            RaisePropertyChanged(nameof(IsAnchorTopRight));
            RaisePropertyChanged(nameof(IsAnchorMiddleLeft));
            RaisePropertyChanged(nameof(IsAnchorCenterPoint));
            RaisePropertyChanged(nameof(IsAnchorMiddleRight));
            RaisePropertyChanged(nameof(IsAnchorBottomLeft));
            RaisePropertyChanged(nameof(IsAnchorBottomCenter));
            RaisePropertyChanged(nameof(IsAnchorBottomRight));
        }
    }
    public bool IsAbsolute { get { return CursorOrigin == MoveCursorOrigin.Absolute; } set { CursorOrigin = MoveCursorOrigin.Absolute; } }
    public bool IsMonitorRelative { get { return CursorOrigin == MoveCursorOrigin.RelativeMonitor; } set { CursorOrigin = MoveCursorOrigin.RelativeMonitor; } }
    public bool IsWindowRelative { get { return CursorOrigin == MoveCursorOrigin.RelativeWindow; } set { CursorOrigin = MoveCursorOrigin.RelativeWindow; } }
    public bool IsCursorRelative { get { return CursorOrigin == MoveCursorOrigin.RelativeCursor; } set { CursorOrigin = MoveCursorOrigin.RelativeCursor; } }

    public bool IsAnchorEnabled => CursorOrigin != MoveCursorOrigin.RelativeCursor;
    public bool IsAnchorTopLeft { get { return CursorAnchor == MoveCursorAnchor.TopLeft; } set { CursorAnchor = MoveCursorAnchor.TopLeft; } }
    public bool IsAnchorTopCenter { get { return CursorAnchor == MoveCursorAnchor.TopCenter; } set { CursorAnchor = MoveCursorAnchor.TopCenter; } }
    public bool IsAnchorTopRight { get { return CursorAnchor == MoveCursorAnchor.TopRight; } set { CursorAnchor = MoveCursorAnchor.TopRight; } }
    public bool IsAnchorMiddleLeft { get { return CursorAnchor == MoveCursorAnchor.MiddleLeft; } set { CursorAnchor = MoveCursorAnchor.MiddleLeft; } }
    public bool IsAnchorCenterPoint { get { return CursorAnchor == MoveCursorAnchor.CenterPoint; } set { CursorAnchor = MoveCursorAnchor.CenterPoint; } }
    public bool IsAnchorMiddleRight { get { return CursorAnchor == MoveCursorAnchor.MiddleRight; } set { CursorAnchor = MoveCursorAnchor.MiddleRight; } }
    public bool IsAnchorBottomLeft { get { return CursorAnchor == MoveCursorAnchor.BottomLeft; } set { CursorAnchor = MoveCursorAnchor.BottomLeft; } }
    public bool IsAnchorBottomCenter { get { return CursorAnchor == MoveCursorAnchor.BottomCenter; } set { CursorAnchor = MoveCursorAnchor.BottomCenter; } }
    public bool IsAnchorBottomRight { get { return CursorAnchor == MoveCursorAnchor.BottomRight; } set { CursorAnchor = MoveCursorAnchor.BottomRight; } }

    public ObservableCollection<SendMouseAction> MouseActions { get; private set; }

    private bool includeNonClientArea;
    public bool IncludeNonClientArea
    {
        get { return includeNonClientArea; }
        set
        {
            includeNonClientArea = value;
            RaisePropertyChanged(nameof(IncludeNonClientArea));
        }
    }

    private bool sendToActiveApplication;
    public bool SendToActiveApplication
    {
        get { return sendToActiveApplication; }
        set
        {
            sendToActiveApplication = value;
            RaisePropertyChanged(nameof(SendToActiveApplication));
            RaisePropertyChanged(nameof(TargetAppEnabled));
        }
    }

    private ApplicationMatcherViewModel targetApp = new ApplicationMatcherViewModel();
    public ApplicationMatcherViewModel TargetApp
    {
        get { return targetApp; }
        set
        {
            targetApp = value;
            RaisePropertyChanged(nameof(TargetApp));
        }
    }

    public bool TargetAppEnabled => SendToActiveApplication == false;

    private bool releaseModifiersAtSequenceEnd = true;
    public bool ReleaseModifiersAtSequenceEnd
    {
        get { return releaseModifiersAtSequenceEnd; }
        set
        {
            releaseModifiersAtSequenceEnd = value;
            RaisePropertyChanged(nameof(ReleaseModifiersAtSequenceEnd));
        }
    }

    public SendMouse()
    {
        MouseActions = new();
    }

    public override bool CanExecute(object? parameter)
    {
        return MouseActions.Count > 0;
    }

    public override ActionCommand Clone()
    {
        var result = new SendMouse() { 
            CursorOrigin = CursorOrigin,
            CursorAnchor = CursorAnchor,
            TargetApp = TargetApp.Clone(),
            SendToActiveApplication = SendToActiveApplication,
            IncludeNonClientArea = IncludeNonClientArea,
            ReleaseModifiersAtSequenceEnd = ReleaseModifiersAtSequenceEnd,
        };
        foreach (var action in MouseActions)
        {
            result.MouseActions.Add(action.Clone());
        }
        return result;
    }

    public override void ExecuteWithContext(CommandExecutionContext context)
    {
        var active = NativeUtils.GetActiveAppHwnd();
        var target = IntPtr.Zero;
        if (sendToActiveApplication) target = active;
        else
        {
            foreach (var hwnd in (new[] { TargetApp }).EnumerateMatchedWindows(false, false))
            {
                target = hwnd;
                break;
            }
        }
        if (target == IntPtr.Zero) return; // no match

        Point cursor = new Point(context.MouseCursorPositionX, context.MouseCursorPositionY);
        Point location = GetInitialPoint(target, cursor);

        ProcessActions(target, location, cursor);

        var newLocation = NativeUtils.GetCursorPosition();
        if (newLocation.HasValue)
        {
            context.MouseCursorPositionX = newLocation.Value.X;
            context.MouseCursorPositionY = newLocation.Value.Y;
        }
    }

    private void ProcessActions(IntPtr hwnd, Point originLocation, Point cursor)
    {
        var wrapper = new NativeUtils.InputWrapper();
        var flags = (SendKeyModifierFlags)0;

        int delayBefore = 0;

        var processModifier = (SendKeyModifierFlags newFlags, SendKeyModifierFlags flag, NativeUtils.Win32VirtualKey vk, bool extended) =>
        {
            if (newFlags.HasFlag(flag) && (!flags.HasFlag(flag)))
            {
                // Key down
                wrapper.AddKeyDown(vk, extended, delayBefore);
                flags |= flag;
                delayBefore = 0;
            }
            else if (flags.HasFlag(flag) && (!newFlags.HasFlag(flag)))
            {
                // Key up
                wrapper.AddKeyUp(vk, extended, delayBefore);
                flags &= ~flag;
                delayBefore = 0;
            }
        };

        foreach (var action in MouseActions)
        {
            delayBefore = action.SleepDelayBeforeMilliseconds;

            var modifiers = action.Action == MouseActionType.ReleaseModifiers ? 0 : action.Modifiers;
            if (action.ModifiersEnabled || action.Action == MouseActionType.ReleaseModifiers)
            {
                processModifier(action.Modifiers, SendKeyModifierFlags.LeftShift, NativeUtils.Win32VirtualKey.VK_LSHIFT, false);
                processModifier(action.Modifiers, SendKeyModifierFlags.LeftControl, NativeUtils.Win32VirtualKey.VK_LCONTROL, false);
                processModifier(action.Modifiers, SendKeyModifierFlags.LeftAlt, NativeUtils.Win32VirtualKey.VK_LMENU, false);
                processModifier(action.Modifiers, SendKeyModifierFlags.LeftWindows, NativeUtils.Win32VirtualKey.VK_LWIN, false);
                processModifier(action.Modifiers, SendKeyModifierFlags.RightShift, NativeUtils.Win32VirtualKey.VK_RSHIFT, false);
                processModifier(action.Modifiers, SendKeyModifierFlags.RightControl, NativeUtils.Win32VirtualKey.VK_RCONTROL, false);
                processModifier(action.Modifiers, SendKeyModifierFlags.RightAlt, NativeUtils.Win32VirtualKey.VK_RMENU, false);
                processModifier(action.Modifiers, SendKeyModifierFlags.RightWindows, NativeUtils.Win32VirtualKey.VK_RWIN, false);
            }

            switch (action.Action)
            {
                case MouseActionType.Move:
                    if (action.IsMovementRelative)
                    {
                        cursor.X += action.X;
                        cursor.Y += action.Y;
                    }
                    else
                    {
                        cursor.X = action.X + originLocation.X;
                        cursor.Y = action.Y + originLocation.Y;
                    }
                    wrapper.AddMouseMove((int) cursor.X, (int) cursor.Y, delayBefore);
                    break;
                case MouseActionType.LeftClick:
                    wrapper.AddMouseLClick(delayBefore);
                    break;
                case MouseActionType.LeftDoubleClick:
                    wrapper.AddMouseLDoubleClick(delayBefore);
                    break;
                case MouseActionType.LeftDown:
                    wrapper.AddMouseLDown(delayBefore);
                    break;
                case MouseActionType.LeftUp:
                    wrapper.AddMouseLUp(delayBefore);
                    break;
                case MouseActionType.RightClick:
                    wrapper.AddMouseRClick(delayBefore);
                    break;
                case MouseActionType.RightDoubleClick:
                    wrapper.AddMouseRDoubleClick(delayBefore);
                    break;
                case MouseActionType.RightDown:
                    wrapper.AddMouseRDown(delayBefore);
                    break;
                case MouseActionType.RightUp:
                    wrapper.AddMouseRUp(delayBefore);
                    break;
                case MouseActionType.MiddleClick:
                    wrapper.AddMouseMClick(delayBefore);
                    break;
                case MouseActionType.MiddleDoubleClick:
                    wrapper.AddMouseMDoubleClick(delayBefore);
                    break;
                case MouseActionType.MiddleDown:
                    wrapper.AddMouseMDown(delayBefore);
                    break;
                case MouseActionType.MiddleUp:
                    wrapper.AddMouseMUp(delayBefore);
                    break;
                case MouseActionType.HorizontalWheel:
                    wrapper.AddMouseHorizontalScroll(action.WheelDelta, delayBefore);
                    break;
                case MouseActionType.VerticalWheel:
                    wrapper.AddMouseVerticalScroll(action.WheelDelta, delayBefore);
                    break;
                case MouseActionType.BlockDoubleClickPause:
                    wrapper.AddMouseDoubleClickBlockWait();
                    break;
                case MouseActionType.ReleaseModifiers:
                    break; // no further action
                case MouseActionType.Sleep:
                    wrapper.AddSleep(delayBefore);
                    break;
            }
        }
        if (releaseModifiersAtSequenceEnd)
        {
            processModifier(0, SendKeyModifierFlags.LeftShift, NativeUtils.Win32VirtualKey.VK_LSHIFT, false);
            processModifier(0, SendKeyModifierFlags.LeftControl, NativeUtils.Win32VirtualKey.VK_LCONTROL, false);
            processModifier(0, SendKeyModifierFlags.LeftAlt, NativeUtils.Win32VirtualKey.VK_LMENU, false);
            processModifier(0, SendKeyModifierFlags.LeftWindows, NativeUtils.Win32VirtualKey.VK_LWIN, false);
            processModifier(0, SendKeyModifierFlags.RightShift, NativeUtils.Win32VirtualKey.VK_RSHIFT, false);
            processModifier(0, SendKeyModifierFlags.RightControl, NativeUtils.Win32VirtualKey.VK_RCONTROL, false);
            processModifier(0, SendKeyModifierFlags.RightAlt, NativeUtils.Win32VirtualKey.VK_RMENU, false);
            processModifier(0, SendKeyModifierFlags.RightWindows, NativeUtils.Win32VirtualKey.VK_RWIN, false);
        }

        System.Diagnostics.Debug.WriteLine(
            $"Origin: {originLocation.X},{originLocation.Y}");
        NativeUtils.SendKeys(hwnd, wrapper);
    }

    private Point GetInitialPoint(IntPtr hwndTarget, Point cursor)
    {
        var displays = NativeUtils.GetDisplayCoordinates();
        if (displays == null) throw new Exception("Unable to retrieve monitor info");

        Rect referenceArea = Rect.Empty;

        switch (CursorOrigin)
        {
            case MoveCursorOrigin.Absolute:
                referenceArea = new Rect(
                    new Point(displays.Min(x => x.clientRect.Left), displays.Min(x => x.clientRect.Top)),
                    new Point(displays.Max(x => x.clientRect.Right), displays.Max(x => x.clientRect.Bottom))
                    );
                break;
            case MoveCursorOrigin.RelativeMonitor:
                referenceArea = GetMonitorAreaFromPoint(cursor);
                break;
            case MoveCursorOrigin.RelativeCursor:
                return cursor;
            case MoveCursorOrigin.RelativeWindow:
                if (IncludeNonClientArea)
                {
                    var r = new NativeUtils.tagRECT();
                    if (NativeUtils.GetWindowRect(hwndTarget, ref r) == 0)
                    {
                        throw new Exception("Failed to get window coordinates");
                    }
                    referenceArea = new Rect(new Point(r.left, r.top), new Point(r.right, r.bottom));
                    break;
                } else
                {
                    var p = new NativeUtils.tagPOINT() { x = 0, y = 0 };
                    if (NativeUtils.ClientToScreen(hwndTarget, ref p) == 0)
                    {
                        throw new Exception("Failed to translate coordinates");
                    }
                    var r = new NativeUtils.tagRECT();
                    if (NativeUtils.GetClientRect(hwndTarget, ref r) == 0)
                    {
                        throw new Exception("Failed to translate coordinates");
                    }
                    var p2 = new NativeUtils.tagPOINT() { x = r.right - 1, y = r.bottom - 1 }; // tagRECT lower-right coords are exclusive
                    if (NativeUtils.ClientToScreen(hwndTarget, ref p2) == 0)
                    {
                        throw new Exception("Failed to translate coordinates");
                    }
                    referenceArea = new Rect(new Point(p.x, p.y), new Point(p2.x, p2.y));
                    break;
                }
            default:
                throw new InvalidOperationException();
        }
        var center = new Point(
            referenceArea.Left + Math.Floor(referenceArea.Width / 2),
            referenceArea.Top + Math.Floor(referenceArea.Height / 2));
        switch (CursorAnchor)
        {
            case MoveCursorAnchor.TopLeft:
                return referenceArea.TopLeft;
            case MoveCursorAnchor.TopRight:
                return referenceArea.TopRight;
            case MoveCursorAnchor.TopCenter:
                return new Point(center.X, referenceArea.Top);
            case MoveCursorAnchor.MiddleLeft:
                return new Point(referenceArea.Left, center.Y);
            case MoveCursorAnchor.CenterPoint:
                return center;
            case MoveCursorAnchor.MiddleRight:
                return new Point(referenceArea.Right, center.Y);
            case MoveCursorAnchor.BottomLeft:
                return referenceArea.BottomLeft;
            case MoveCursorAnchor.BottomCenter:
                return new Point(center.X, referenceArea.Bottom);
            case MoveCursorAnchor.BottomRight:
                return referenceArea.BottomRight;
            default:
                throw new InvalidOperationException("Invalid enumeration value");
        }
    }

    private Rect GetMonitorAreaFromPoint(Point refPoint)
    {
        IntPtr hMon = NativeUtils.MonitorFromPoint(refPoint);
        var displays = NativeUtils.GetDisplayCoordinates();
        if (displays == null) return new Rect(0, 0, 1, 1);
        return displays
            .Where(d => d.hMonitor == hMon)
            .Select(d => d.monitorRect)
            .FirstOrDefault();
    }

    public override void WriteJson(JsonObject o)
    {
        o.AddLowerCamelValue(nameof(CursorOrigin), CursorOrigin.ToString());
        o.AddLowerCamelValue(nameof(SendToActiveApplication), SendToActiveApplication);
        if (!SendToActiveApplication)
        {
            o.AddLowerCamel(nameof(TargetApp), TargetApp.ToJson());
        }
        if (CursorOrigin == MoveCursorOrigin.RelativeWindow)
        {
            o.AddLowerCamelValue<bool>(nameof(IncludeNonClientArea), IncludeNonClientArea);
        }
        if (IsAnchorEnabled)
        {
            o.AddLowerCamel(nameof(CursorAnchor), CursorAnchor.ToString());
        }
        o.AddLowerCamel(nameof(MouseActions), MouseActions.ToJson());
        o.AddLowerCamelValue(nameof(ReleaseModifiersAtSequenceEnd), ReleaseModifiersAtSequenceEnd);
    }

    public static SendMouse CreateFromJson(JsonObject o)
    {
        var result = new SendMouse();
        o.TryGet<string>(nameof(CursorOrigin), m =>
        {
            result.CursorOrigin = Enum.Parse<MoveCursorOrigin>(m, true);
        });
        o.TryGetValue<bool>(nameof(SendToActiveApplication), b => result.SendToActiveApplication = b);
        if (!result.SendToActiveApplication)
        {
            if (o.ContainsKey(nameof(TargetApp)))
            {
                result.TargetApp = ApplicationMatcherViewModel.FromJson(o[nameof(TargetApp)]!);
            }
        }
        if (result.CursorOrigin == MoveCursorOrigin.RelativeWindow)
        {
            o.TryGetValue<bool>(nameof(IncludeNonClientArea), b => result.IncludeNonClientArea = b);
        }
        if (result.IsAnchorEnabled)
        {
            o.TryGet<string>(nameof(CursorAnchor), m =>
            {
                result.cursorAnchor = Enum.Parse<MoveCursorAnchor>(m, true);
            });
        }
        o.TryGet<JsonArray>(nameof(MouseActions), actions =>
        {
            foreach (var a in actions)
            {
                result.MouseActions.Add(SendMouseAction.FromJson(a!));
            }
        });
        o.TryGetValue<bool>(nameof(ReleaseModifiersAtSequenceEnd), b => result.ReleaseModifiersAtSequenceEnd = b);

        return result;
    }
}

public class SendMouseDefinition : ActionCommandDefinition
{
    public static SendMouseDefinition Instance = new();

    public override string ActionName => "SendMouse";
    public override string ActionDisplayName => "Send mouse inputs";

    public override string ActionShortName => "Mouse";

    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/SendMouse.png"));

    public override ActionCommand Create()
    {
        return new SendMouse();
    }
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return SendMouse.CreateFromJson(o);
    }

    public override FrameworkElement CreateConfigElement()
    {
        return new SendMouseConfigControl();
    }
}
