using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PowerOverlay.Commands;

public class SendCharacters : ActionCommand
{

    public override ActionCommandDefinition Definition { get { return SendCharactersDefinition.Instance; } }

    private readonly FrameworkElement configElement = SendCharactersDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

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

    private string text = String.Empty;
    public string Text
    {
        get { return text; }
        set
        {
            text = value;
            RaisePropertyChanged(nameof(Text));
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
        return ((!String.IsNullOrEmpty(Text))
            && (
            (appTargets.Count > 0)
            || sendToDesktop || sendToShell || sendToActiveApplication
            ));
    }

    public override ActionCommand Clone()
    {
        var result = new SendCharacters()
        {
            Text = Text,
            SendToAllMatches = SendToAllMatches,
            SendToActiveApplication = SendToActiveApplication,
            SendToDesktop = SendToDesktop,
            SendToShell = SendToShell,
        };

        foreach (var x in ApplicationTargets)
        {
            result.ApplicationTargets.Add(x.Clone());
        }
        return result;
    }

    public override void ExecuteWithContext(CommandExecutionContext context)
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

        foreach (var hwnd in ApplicationTargets.EnumerateMatchedWindows(false, true))
        {
            if (hwnd == desktop || hwnd == shell) continue;
            targets.Add(hwnd);
            if (!SendToAllMatches) break;
        }
        var uniqueTargets = targets.Where(x => x != IntPtr.Zero).Distinct().ToList();

        var textUTF16 = MemoryMarshal.Cast<byte, Int16>(Encoding.Unicode.GetBytes(Text).AsSpan());

        foreach (var target in uniqueTargets)
        {
            NativeUtils.SendText(target, textUTF16);
        }

        return;
    }

    public override void WriteJson(JsonObject o)
    {
        o.AddLowerCamel(nameof(Text), JsonValue.Create(Text));
        if (ApplicationTargets.Count > 0)
        {
            o.AddLowerCamel(nameof(ApplicationTargets), ApplicationTargets.ToJson());
        }
        o.AddLowerCamel(nameof(SendToActiveApplication), JsonValue.Create(SendToActiveApplication));
        o.AddLowerCamel(nameof(SendToDesktop), JsonValue.Create(SendToDesktop));
        o.AddLowerCamel(nameof(SendToShell), JsonValue.Create(SendToShell));
        o.AddLowerCamel(nameof(SendToAllMatches), JsonValue.Create(SendToAllMatches));
    }

    public static SendCharacters CreateFromJson(JsonObject o)
    {
        var result = new SendCharacters();

        o.TryGet<string>(nameof(Text), s => result.Text = s);
        o.TryGet<JsonArray>(nameof(ApplicationTargets), xs => {
            foreach (var x in xs)
            {
                result.ApplicationTargets.Add(
                    ApplicationMatcherViewModel.FromJson(x!)
                );
            }
        });

        o.TryGetValue<bool>(nameof(SendToActiveApplication), b => result.SendToActiveApplication = b);
        o.TryGetValue<bool>(nameof(SendToDesktop), b => result.SendToDesktop = b);
        o.TryGetValue<bool>(nameof(SendToShell), b => result.SendToShell = b);
        o.TryGetValue<bool>(nameof(SendToAllMatches), b => result.SendToAllMatches = b);
        
        return result;
    }
}

public class SendCharactersDefinition : ActionCommandDefinition
{
    public static SendCharactersDefinition Instance = new();

    public override string ActionName => "SendCharacters";
    public override string ActionDisplayName => "Send text to window";
    public override string ActionShortName => "Text";

    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/SendCharacters.png"));

    public override ActionCommand Create()
    {
        return new SendCharacters();
    }
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return SendCharacters.CreateFromJson(o);
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

        var textToSendLbl = new TextBlock()
        {
            Text = "Text to send:",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        DockPanel.SetDock(textToSendLbl, Dock.Top);


        var selector = new ListBox();
        selector.Style = (Style)selector.FindResource("ApplicationTargetList");

        selector.SetBinding(ListBox.ItemsSourceProperty, new Binding(nameof(SendCharacters.ApplicationTargets)));

        RoutedEventHandler hClick = (object sender, RoutedEventArgs e) =>
        {
            Button? b = e.OriginalSource as Button;
            if (b == null) return;
            switch (b.Name)
            {
                case "TargetAdd":
                    e.Handled = true;
                    ((SendCharacters)b.DataContext).ApplicationTargets.Add(new ApplicationMatcherViewModel());
                    selector.SelectedIndex = selector.Items.Count - 1;
                    return;
                case "TargetRemove":
                    e.Handled = true;
                    if (selector.SelectedIndex == -1) return;
                    selector.SelectedIndex = selector.SelectedIndex - 1;
                    ((SendCharacters)b.DataContext).ApplicationTargets.RemoveAt(selector.SelectedIndex + 1);
                    return;
            }
        };

        selector.DataContextChanged += (o, e) =>
        {
            if (selector.DataContext == null) return;
            if (selector.SelectedIndex != -1) return;
            if (((SendCharacters)selector.DataContext).ApplicationTargets.Count > 0)
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

        addCheckbox("Send to desktop", nameof(SendCharacters.SendToDesktop));
        addCheckbox("Send to shell", nameof(SendCharacters.SendToShell));
        addCheckbox("Send to active application", nameof(SendCharacters.SendToActiveApplication));
        addCheckbox("Send to all application matches (otherwise first match)", nameof(SendCharacters.SendToAllMatches));

        var txtbox = new TextBox()
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 100,
        };
        txtbox.SetBinding(TextBox.TextProperty, "Text");

        ctrl.Children.Add(sp);
        ctrl.Children.Add(appTargetsLbl);
        ctrl.Children.Add(selector);
        ctrl.Children.Add(textToSendLbl);
        ctrl.Children.Add(txtbox);

        return ctrl;
    }
}
