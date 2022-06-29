using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace overlay_popup.Commands;

[Flags]
public enum SendKeyModifierFlags
{
    LeftShift = 0x01,
    RightShift = 0x02,

    LeftControl = 0x10,
    RightControl = 0x20,

    LeftAlt = 0x100,
    RightAlt = 0x200,

    LeftWindows = 0x1000,
    RightWindows = 0x2000,

}

public class SendKeySpecialKey
{
    public readonly byte VirtualKeyCode;
    public readonly string Name;

    private SendKeySpecialKey(string name, byte virtualKeyCode)
    {
        Name = name;
        VirtualKeyCode = virtualKeyCode;
    }

    public static readonly SendKeySpecialKey F1 = new SendKeySpecialKey("F1",0x70);
    public static readonly SendKeySpecialKey F2 = new SendKeySpecialKey("F2",0x71);
    public static readonly SendKeySpecialKey F3 = new SendKeySpecialKey("F3",0x72);
    public static readonly SendKeySpecialKey F4 = new SendKeySpecialKey("F4",0x73);
    public static readonly SendKeySpecialKey F5 = new SendKeySpecialKey("F5",0x74);
    public static readonly SendKeySpecialKey F6 = new SendKeySpecialKey("F6",0x75);
    public static readonly SendKeySpecialKey F7 = new SendKeySpecialKey("F7",0x76);
    public static readonly SendKeySpecialKey F8 = new SendKeySpecialKey("F8",0x77);
    public static readonly SendKeySpecialKey F9 = new SendKeySpecialKey("F9",0x78);
    public static readonly SendKeySpecialKey F10 = new SendKeySpecialKey("F10",0x79);
    public static readonly SendKeySpecialKey F11 = new SendKeySpecialKey("F11",0x7A);
    public static readonly SendKeySpecialKey F12 = new SendKeySpecialKey("F12",0x7B);
    public static readonly SendKeySpecialKey Escape = new SendKeySpecialKey("ESCAPE",0x1B);
    public static readonly SendKeySpecialKey Tab = new SendKeySpecialKey("TAB", 0x09);
    public static readonly SendKeySpecialKey SpaceBar = new SendKeySpecialKey("SPACE", 0x20);
    public static readonly SendKeySpecialKey Backspace = new SendKeySpecialKey("BACKSPACE", 0x08);
    public static readonly SendKeySpecialKey CapsLock = new SendKeySpecialKey("CAPS LOCK", 0x14);
    public static readonly SendKeySpecialKey Enter = new SendKeySpecialKey("ENTER", 0x0D);
    public static readonly SendKeySpecialKey Insert = new SendKeySpecialKey("INSERT", 0x2D);
    public static readonly SendKeySpecialKey Delete = new SendKeySpecialKey("DELETE",0x2E);
    public static readonly SendKeySpecialKey Home = new SendKeySpecialKey("HOME", 0x24);
    public static readonly SendKeySpecialKey End = new SendKeySpecialKey("END",0x23);
    public static readonly SendKeySpecialKey PageUp = new SendKeySpecialKey("PAGE UP",0x21);
    public static readonly SendKeySpecialKey PageDown = new SendKeySpecialKey("PAGE DOWN",0x22);
    public static readonly SendKeySpecialKey PrintScreen = new SendKeySpecialKey("PRINT SCREEN",0x2C);
    public static readonly SendKeySpecialKey ScrollLock = new SendKeySpecialKey("SCROLL LOCK",0x91);
    public static readonly SendKeySpecialKey PauseBreak = new SendKeySpecialKey("PAUSE/BREAK", 0x13);
    public static readonly SendKeySpecialKey ArrowLeft = new SendKeySpecialKey("Arrow left", 0x25);
    public static readonly SendKeySpecialKey ArrowRight = new SendKeySpecialKey("Arrow right", 0x27);
    public static readonly SendKeySpecialKey ArrowUp = new SendKeySpecialKey("Arrow up", 0x26);
    public static readonly SendKeySpecialKey ArrowDown = new SendKeySpecialKey("Arrow down", 0x28);
    public static readonly SendKeySpecialKey NumLock = new SendKeySpecialKey("NUM LOCK", 0x90);
    public static readonly SendKeySpecialKey NumericDivide = new SendKeySpecialKey("Numeric /", 0x6F);
    public static readonly SendKeySpecialKey NumericMultiply = new SendKeySpecialKey("Numeric *", 0x6A);
    public static readonly SendKeySpecialKey NumericSubtract = new SendKeySpecialKey("Numeric -",0x6D);
    public static readonly SendKeySpecialKey NumericAdd = new SendKeySpecialKey("Numeric +",0x6B);
    public static readonly SendKeySpecialKey NumericDecimal = new SendKeySpecialKey("Numeric decimal", 0x6E);
    public static readonly SendKeySpecialKey Numeric0 = new SendKeySpecialKey("Numeric 0",0x60);
    public static readonly SendKeySpecialKey Numeric1 = new SendKeySpecialKey("Numeric 1",0x61);
    public static readonly SendKeySpecialKey Numeric2 = new SendKeySpecialKey("Numeric 2",0x62);
    public static readonly SendKeySpecialKey Numeric3 = new SendKeySpecialKey("Numeric 3",0x63);
    public static readonly SendKeySpecialKey Numeric4 = new SendKeySpecialKey("Numeric 4",0x64);
    public static readonly SendKeySpecialKey Numeric5 = new SendKeySpecialKey("Numeric 5",0x65);
    public static readonly SendKeySpecialKey Numeric6 = new SendKeySpecialKey("Numeric 6",0x66);
    public static readonly SendKeySpecialKey Numeric7 = new SendKeySpecialKey("Numeric 7",0x67);
    public static readonly SendKeySpecialKey Numeric8 = new SendKeySpecialKey("Numeric 8",0x68);
    public static readonly SendKeySpecialKey Numeric9 = new SendKeySpecialKey("Numeric 9",0x69);
}

public class SendKeyValue : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

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
        Notify(
          nameof(DisplayValue)
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

    private SendKeySpecialKey? specialKey;
    private string normalKeyText = String.Empty;
    private bool isSpecialKey;

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
    public string DisplayValue { get
        {
            var keyText = String.Empty;

            if (isSpecialKey && specialKey == null) return "<invalid>";
            if (isSpecialKey) keyText = specialKey!.Name;
            else if (normalKeyText.Length != 1) return "<invalid>";
            else keyText = normalKeyText;
            return $"{keyText} ({ModifiersDisplayValue})";
        } 
    }

    public bool IsSpecialKey
    {
        get { return isSpecialKey; }
        set
        {
            isSpecialKey = value;
            Notify(nameof(IsSpecialKey), nameof(IsNormalKey), nameof(DisplayValue));
        }
    }
    public bool IsNormalKey
    {
        get { return !isSpecialKey; }
        set
        {
            isSpecialKey = !value;
            Notify(nameof(IsSpecialKey), nameof(IsNormalKey), nameof(DisplayValue));
        }
    }

    private List<SendKeySpecialKey> specialKeys = typeof(SendKeySpecialKey)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(x => x.FieldType.IsAssignableTo(typeof(SendKeySpecialKey)))
                .Select(x => (SendKeySpecialKey) x.GetValue(null)!)
                .ToList();
    public IEnumerable<string> SpecialKeys => specialKeys.Select(x => x.Name);

    public int SpecialKeyIndex
    {
        get
        {
            if (specialKey == null) return -1;
            return specialKeys.FindIndex(x => x.Name.Equals(specialKey.Name, StringComparison.OrdinalIgnoreCase));
        }
        set
        {
            if (value == -1 || value < 0 || value >= specialKeys.Count)
            {
                specialKey = null;
            }
            else
            {
                specialKey = specialKeys[value];
            }
            Notify(nameof(SpecialKeyIndex), nameof(SpecialKey), nameof(DisplayValue));
        }
    }

    public string SpecialKey
    {
        get { return specialKey?.Name ?? String.Empty; }
        set
        {
            if (String.IsNullOrEmpty(value)) specialKey = null;
            else
            {
                specialKey = specialKeys.Find(x => x.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            }
            Notify(nameof(SpecialKeyIndex), nameof(SpecialKey), nameof(DisplayValue));
        }
    }

    public SendKeySpecialKey? SpecialKeyKey => specialKey;

    public string NormalKeyText
    {
        get { return normalKeyText; }
        set
        {
            normalKeyText = value;
            Notify(nameof(NormalKeyText), nameof(DisplayValue));
        }
    }

    private void Notify(params string[] names)
    {
        foreach (var n in names) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public bool IsValid()
    {
        return (IsSpecialKey && specialKey != null)
            || ((!IsSpecialKey) && normalKeyText.Length == 1 && NativeUtils.InputWrapper.IsValidChar(normalKeyText[0]));
    }

    public SendKeyValue Clone()
    {
        return new SendKeyValue()
        {
            Modifiers = Modifiers,
            SpecialKey = SpecialKey,
            NormalKeyText = NormalKeyText,
            IsSpecialKey = IsSpecialKey,
        };
    }
}

public class SendKeys : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return SendKeysDefinition.Instance; } }

    private readonly FrameworkElement configElement = SendKeysDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    private ObservableCollection<SendKeyValue> keySequence = new();
    public ObservableCollection<SendKeyValue> KeySequence => keySequence;

    private ObservableCollection<ApplicationMatcherViewModel> appTargets = new();
    public ObservableCollection<ApplicationMatcherViewModel> ApplicationTargets => appTargets;

    private bool sendToDesktop;
    public bool SendToDesktop
    {
        get { return sendToDesktop; }
        set
        {
            sendToDesktop = value;
            RaisePropertyChanged(nameof(SendToDesktop));
        }
    }

    private bool sendToShell;
    public bool SendToShell
    {
        get { return sendToShell; }
        set
        {
            sendToShell = value;
            RaisePropertyChanged(nameof(SendToShell));
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
        }
    }

    private bool sendToAllMatches;
    public bool SendToAllMatches
    {
        get { return sendToAllMatches; }
        set
        {
            sendToAllMatches = value;
            RaisePropertyChanged(nameof(SendToAllMatches));
        }
    }

    public override bool CanExecute(object? parameter)
    {
        return 
            (KeySequence.Count > 0) 
            && KeySequence.All(x => x.IsValid())
            && (
                (appTargets.Count > 0)
                || sendToDesktop || sendToShell || sendToActiveApplication
            );
    }

    public override ActionCommand Clone()
    {
        var result = new SendKeys()
        {
            SendToAllMatches = SendToAllMatches,
            SendToActiveApplication = SendToActiveApplication,
            SendToDesktop = SendToDesktop,
            SendToShell = SendToShell,
        };

        foreach (var x in ApplicationTargets)
        {
            result.ApplicationTargets.Add(x.Clone());
        }
        foreach (var x in KeySequence)
        {
            result.KeySequence.Add(x.Clone());
        }
        return result;
    }

    public override void Execute(object? parameter)
    {
        var active = NativeUtils.GetActiveAppHwnd();
        var shell = NativeUtils.GetShellWindow();
        var desktop = NativeUtils.GetDesktopWindow();

        var targets = new List<IntPtr>();

        if (this.SendToActiveApplication)
        {
            targets.Add(active);
        }
        if (this.SendToShell)
        {
            targets.Add(shell);
        }
        if (this.SendToDesktop)
        {
            targets.Add(desktop);
        }

        bool hasMatch = false;
        foreach (var hwnd in NativeUtils.EnumerateTopLevelWindows(false, true))
        {
            if (hwnd == desktop || hwnd == shell) continue;

            foreach (var target in ApplicationTargets)
            {
                if (target.Matches(hwnd))
                {
                    targets.Add(hwnd);
                    if (!SendToAllMatches)
                    {
                        hasMatch = true;
                    }
                    break;
                }
            }
            if (hasMatch) break;
        }
        var uniqueTargets = targets.Where(x => x != IntPtr.Zero).Distinct().ToList();

        var keys = BuildKeyPresses();
        if (keys == null) return;

        foreach (var target in uniqueTargets)
        {
            SendKeysToWindow(target, keys);
        }

        return;
    }

    private NativeUtils.InputWrapper? BuildKeyPresses()
    {
        var result = new NativeUtils.InputWrapper();

        //result.AddKeyModifierClear();

        foreach (var key in KeySequence)
        {
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.LeftShift)) 
                result.AddKeyDown(NativeUtils.Win32VirtualKey.VK_LSHIFT, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.RightShift))
                result.AddKeyDown(NativeUtils.Win32VirtualKey.VK_RSHIFT, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.LeftControl))
                result.AddKeyDown(NativeUtils.Win32VirtualKey.VK_LCONTROL, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.RightControl))
                result.AddKeyDown(NativeUtils.Win32VirtualKey.VK_RCONTROL, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.LeftAlt))
                result.AddKeyDown(NativeUtils.Win32VirtualKey.VK_LMENU, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.RightAlt))
                result.AddKeyDown(NativeUtils.Win32VirtualKey.VK_RMENU, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.LeftWindows))
                result.AddKeyDown(NativeUtils.Win32VirtualKey.VK_LWIN, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.RightWindows))
                result.AddKeyDown(NativeUtils.Win32VirtualKey.VK_RWIN, false);

            if (key.IsNormalKey)
            {
                if (!result.AddKeyPress(key.NormalKeyText[0])) return null;
            } else
            {
                var vk = key.SpecialKeyKey!.VirtualKeyCode;
                result.AddKeyPress((NativeUtils.Win32VirtualKey) (byte) vk, false);
            }

            if (key.Modifiers.HasFlag(SendKeyModifierFlags.LeftShift))
                result.AddKeyUp(NativeUtils.Win32VirtualKey.VK_LSHIFT, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.RightShift))
                result.AddKeyUp(NativeUtils.Win32VirtualKey.VK_RSHIFT, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.LeftControl))
                result.AddKeyUp(NativeUtils.Win32VirtualKey.VK_LCONTROL, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.RightControl))
                result.AddKeyUp(NativeUtils.Win32VirtualKey.VK_RCONTROL, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.LeftAlt))
                result.AddKeyUp(NativeUtils.Win32VirtualKey.VK_LMENU, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.RightAlt))
                result.AddKeyUp(NativeUtils.Win32VirtualKey.VK_RMENU, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.LeftWindows))
                result.AddKeyUp(NativeUtils.Win32VirtualKey.VK_LWIN, false);
            if (key.Modifiers.HasFlag(SendKeyModifierFlags.RightWindows))
                result.AddKeyUp(NativeUtils.Win32VirtualKey.VK_RWIN, false);
        }
        return result;
    }

    private void SendKeysToWindow(IntPtr hwnd, NativeUtils.InputWrapper keys)
    {
        NativeUtils.SendKeys(hwnd, keys);
    }
}

public class SendKeysDefinition : ActionCommandDefinition
{
    public static SendKeysDefinition Instance = new();

    public override string ActionName => "SendKeys";
    public override string ActionDisplayName => "Send key presses";

    public override ActionCommand Create()
    {
        return new SendKeys();
    }

    public override FrameworkElement CreateConfigElement()
    {
        var ctrl = new DockPanel();

        var appTargetsLbl = new TextBlock()
        {
            Text = "Application targets:",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        DockPanel.SetDock(appTargetsLbl, Dock.Top);

        var keysToSendLbl = new TextBlock()
        {
            Text = "Keys to send:",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        DockPanel.SetDock(keysToSendLbl, Dock.Top);


        var selector = new ListBox();
        selector.Style = (Style)selector.FindResource("ApplicationTargetList");

        selector.SetBinding(ListBox.ItemsSourceProperty, new Binding(nameof(SendKeys.ApplicationTargets)));

        RoutedEventHandler hClick = (object sender, RoutedEventArgs e) =>
        {
            Button? b = e.OriginalSource as Button;
            if (b == null) return;
            switch (b.Name)
            {
                case "TargetAdd":
                    e.Handled = true;
                    ((SendKeys)b.DataContext).ApplicationTargets.Add(new ApplicationMatcherViewModel());
                    selector.SelectedIndex = selector.Items.Count - 1;
                    return;
                case "TargetRemove":
                    e.Handled = true;
                    if (selector.SelectedIndex == -1) return;
                    selector.SelectedIndex = selector.SelectedIndex - 1;
                    ((SendKeys)b.DataContext).ApplicationTargets.RemoveAt(selector.SelectedIndex + 1);
                    return;
            }
        };

        selector.DataContextChanged += (o, e) =>
        {
            if (selector.DataContext == null) return;
            if (selector.SelectedIndex != -1) return;
            if (((SendKeys)selector.DataContext).ApplicationTargets.Count > 0)
            {
                selector.SelectedIndex = 0;
            }
        };

        selector.AddHandler(Button.ClickEvent, hClick);

        //    new ApplicationTargetControl();
        //selector.SetBinding(ApplicationTargetControl.DataContextProperty, 
        //    new Binding(nameof(SendCharacters.ApplicationTargets)));
        DockPanel.SetDock(selector, Dock.Top);

        var sp = new StackPanel();
        sp.Orientation = Orientation.Vertical;
        DockPanel.SetDock(sp, Dock.Top);

        var addCheckbox = (string txt, string prop) =>
        {
            var cb = new CheckBox() { Content = txt, HorizontalAlignment = HorizontalAlignment.Left };
            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding(prop));
            sp.Children.Add(cb);
        };

        addCheckbox("Send to desktop", nameof(SendKeys.SendToDesktop));
        addCheckbox("Send to shell", nameof(SendKeys.SendToShell));
        addCheckbox("Send to active application", nameof(SendKeys.SendToActiveApplication));
        addCheckbox("Send to all application matches (otherwise first match)", nameof(SendKeys.SendToAllMatches));

        var keylist = new ListBox();
        keylist.Style = (Style)selector.FindResource("KeySequenceList");

        keylist.SetBinding(ListBox.ItemsSourceProperty, new Binding(nameof(SendKeys.KeySequence)));

        RoutedEventHandler hClick2 = (object sender, RoutedEventArgs e) =>
        {
            Button? b = e.OriginalSource as Button;
            if (b == null) return;
            switch (b.Name)
            {
                case "TargetAdd":
                    e.Handled = true;
                    ((SendKeys)b.DataContext).KeySequence.Add(new SendKeyValue());
                    keylist.SelectedIndex = keylist.Items.Count - 1;
                    return;
                case "TargetRemove":
                    e.Handled = true;
                    if (keylist.SelectedIndex == -1) return;
                    keylist.SelectedIndex = keylist.SelectedIndex - 1;
                    ((SendKeys)b.DataContext).KeySequence.RemoveAt(keylist.SelectedIndex + 1);
                    return;
            }
        };

        keylist.DataContextChanged += (o, e) =>
        {
            if (keylist.DataContext == null) return;
            if (keylist.SelectedIndex != -1) return;
            if (((SendKeys)keylist.DataContext).KeySequence.Count > 0)
            {
                keylist.SelectedIndex = 0;
            }
        };

        keylist.AddHandler(Button.ClickEvent, hClick2);
        ctrl.Children.Add(sp);
        ctrl.Children.Add(appTargetsLbl);
        ctrl.Children.Add(selector);
        ctrl.Children.Add(keysToSendLbl);
        ctrl.Children.Add(keylist);

        return ctrl;
    }
}
