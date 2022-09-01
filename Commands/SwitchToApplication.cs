using System;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PowerOverlay.Commands;

public class SwitchToApplication : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return SwitchToApplicationDefinition.Instance; } }

    private readonly FrameworkElement configElement = SwitchToApplicationDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    public ObservableCollection<ApplicationMatcherViewModel> ApplicationTargets { get; private set; }

    public SwitchToApplication()
    {
        ApplicationTargets = new();
        ApplicationTargets.CollectionChanged += (o, e) => { RaiseCanExecuteChanged(new EventArgs()); };
    }
    public override bool CanExecute(object? parameter)
    {
        return ApplicationTargets.Count > 0;
    }

    public override ActionCommand Clone()
    {
        var clone = new SwitchToApplication() { };
        foreach (var target in ApplicationTargets) clone.ApplicationTargets.Add(target);
        return clone;
    }

    public override void ExecuteWithContext(CommandExecutionContext context)
    {
        foreach (var hwnd in ApplicationTargets.EnumerateMatchedWindows(false, true))
        {
            NativeUtils.SetForegroundWindow(hwnd);
            return;
        }
    }

    public override void WriteJson(JsonObject o)
    {
        if (ApplicationTargets.Count > 0)
        {
            o.AddLowerCamel(nameof(ApplicationTargets), ApplicationTargets.ToJson());
        }
    }

    public static SwitchToApplication CreateFromJson(JsonObject o)
    {
        var result = new SwitchToApplication();
        o.TryGet<JsonArray>(nameof(ApplicationTargets), xs => {
            foreach (var x in xs)
            {
                result.ApplicationTargets.Add(
                    ApplicationMatcherViewModel.FromJson(x)
                );
            }
        });

        return result;
    }
}

public class SwitchToApplicationDefinition : ActionCommandDefinition
{
    public static SwitchToApplicationDefinition Instance = new();

    public override string ActionName => "SwitchApp";
    public override string ActionDisplayName => "Switch to app";
    public override string ActionShortName => "Switch App";

    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/SwitchToApp.png"));

    public override ActionCommand Create()
    {
        return new SwitchToApplication();
    }
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return SwitchToApplication.CreateFromJson(o);
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

        var selector = new ListBox();
        selector.Style = (Style)selector.FindResource("ApplicationTargetList");

        selector.SetBinding(ListBox.ItemsSourceProperty, new Binding(nameof(SwitchToApplication.ApplicationTargets)));

        RoutedEventHandler hClick = (object sender, RoutedEventArgs e) =>
        {
            Button? b = e.OriginalSource as Button;
            if (b == null) return;
            switch (b.Name)
            {
                case "TargetAdd":
                    e.Handled = true;
                    ((SwitchToApplication)b.DataContext).ApplicationTargets.Add(new ApplicationMatcherViewModel());
                    selector.SelectedIndex = selector.Items.Count - 1;
                    return;
                case "TargetRemove":
                    e.Handled = true;
                    if (selector.SelectedIndex == -1) return;
                    selector.SelectedIndex = selector.SelectedIndex - 1;
                    ((SwitchToApplication)b.DataContext).ApplicationTargets.RemoveAt(selector.SelectedIndex + 1);
                    return;
            }
        };

        selector.DataContextChanged += (o, e) =>
        {
            if (selector.DataContext == null) return;
            if (selector.SelectedIndex != -1) return;
            if (((SwitchToApplication)selector.DataContext).ApplicationTargets.Count > 0)
            {
                selector.SelectedIndex = 0;
            }
        };

        selector.AddHandler(Button.ClickEvent, hClick);

        DockPanel.SetDock(selector, Dock.Top);

        ctrl.Children.Add(appTargetsLbl);
        ctrl.Children.Add(selector);

        return ctrl;
    }
}
