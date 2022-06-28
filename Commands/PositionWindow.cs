using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace overlay_popup.Commands;

public class PositionWindow : ActionCommand
{
    public override PositionWindowDefinition Definition { get { return PositionWindowDefinition.Instance; } }

    private readonly FrameworkElement configElement = PositionWindowDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    public ObservableCollection<ApplicationMatcherViewModel> ApplicationTargets { get; private set; }

    public enum PositionMode
    {
        Positioned,
        Restore,
        Minimise,
        Maximise,
    }

    private PositionMode mode;

    public PositionMode Mode
    {
        get { return mode; }
        set
        {
            mode = value;
            RaisePropertyChanged(nameof(Mode));
            RaisePropertyChanged(nameof(IsPositioned));
            RaisePropertyChanged(nameof(IsRestore));
            RaisePropertyChanged(nameof(IsMinimise));
            RaisePropertyChanged(nameof(IsMaximise));
            RaisePropertyChanged(nameof(PositionPanelVisibility));
        }
    }

    public Visibility PositionPanelVisibility
    {
        get
        {
            return (mode == PositionMode.Positioned) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public bool IsPositioned
    {
        get { return mode == PositionMode.Positioned; }
        set { Mode = PositionMode.Positioned; }
    }
    public bool IsRestore
    {
        get { return mode == PositionMode.Restore; }
        set { Mode = PositionMode.Restore; }
    }
    public bool IsMinimise
    {
        get { return mode == PositionMode.Minimise; }
        set { Mode = PositionMode.Minimise; }
    }
    public bool IsMaximise
    {
        get { return mode == PositionMode.Maximise; }
        set { Mode = PositionMode.Maximise; }
    }

    private int left, top, height, width;
    private bool setPosition;
    private bool setSize;
    public bool SetPosition { get { return setPosition; } set { setPosition = value; RaisePropertyChanged(nameof(SetPosition)); } }
    public bool SetSize { get { return setSize; } set { setSize = value; RaisePropertyChanged(nameof(SetSize)); } }
    public int Left { get { return left; } set { left = value; RaisePropertyChanged(nameof(Left)); } }
    public int Top { get { return top; } set { top = value; RaisePropertyChanged(nameof(Top)); } }
    public int Width { get { return width; } set { width = value; RaisePropertyChanged(nameof(Width)); } }
    public int Height { get { return height; } set { height = value; RaisePropertyChanged(nameof(Height)); } }

    private bool positionAllMatches;
    public bool PositionAllMatches
    {
        get { return positionAllMatches; }
        set
        {
            positionAllMatches = value;
            RaisePropertyChanged(nameof(PositionAllMatches));
        }
    }

    public PositionWindow()
    {
        ApplicationTargets = new();
        ApplicationTargets.CollectionChanged += (o, e) => { RaiseCanExecuteChanged(new EventArgs()); };
    }
    public override bool CanExecute(object? parameter)
    {
        return ApplicationTargets.Count > 0 &&
            ( Mode != PositionMode.Positioned
            || SetPosition || SetSize
            );
    }

    public override ActionCommand Clone()
    {
        var clone = new PositionWindow() {
            PositionAllMatches = PositionAllMatches,
            Mode = Mode,
            SetPosition = SetPosition,
            SetSize = SetSize,
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height,
        };
        foreach (var target in ApplicationTargets) clone.ApplicationTargets.Add(target);
        return clone;
    }

    private void Resize(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return;
        switch (Mode)
        {
            case PositionMode.Restore:
                NativeUtils.ShowWindowAsync(hwnd, NativeUtils.Win32ShowCmd.NoActivate);
                return;
            case PositionMode.Minimise:
                NativeUtils.ShowWindowAsync(hwnd, NativeUtils.Win32ShowCmd.MinimizeOnly);
                return;
            case PositionMode.Maximise:
                NativeUtils.ShowWindowAsync(hwnd, NativeUtils.Win32ShowCmd.ActivateAndMaximize); // no option to not activate
                return;
            case PositionMode.Positioned:
                break;
        }
        NativeUtils.RepositionWindow(hwnd, SetPosition, SetSize, Left, Top, Width, Height);
    }

    public override void Execute(object? parameter)
    {
        foreach (var hwnd in NativeUtils.EnumerateTopLevelWindows(false, true))
        {
            foreach (var target in ApplicationTargets)
            {
                if (target.Matches(hwnd))
                {
                    Resize(hwnd);
                    if (!PositionAllMatches) return;
                }
            }
        }
    }
}

public class PositionWindowDefinition : ActionCommandDefinition
{
    public static PositionWindowDefinition Instance = new();

    public override string ActionName => "PositionWindow";
    public override string ActionDisplayName => "Position window";

    public override ActionCommand Create()
    {
        return new PositionWindow();
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
                    ((PositionWindow)b.DataContext).ApplicationTargets.Add(new ApplicationMatcherViewModel());
                    selector.SelectedIndex = selector.Items.Count - 1;
                    return;
                case "TargetRemove":
                    e.Handled = true;
                    if (selector.SelectedIndex == -1) return;
                    selector.SelectedIndex = selector.SelectedIndex - 1;
                    ((PositionWindow)b.DataContext).ApplicationTargets.RemoveAt(selector.SelectedIndex + 1);
                    return;
            }
        };

        selector.DataContextChanged += (o, e) =>
        {
            if (selector.DataContext == null) return;
            if (selector.SelectedIndex != -1) return;
            if (((PositionWindow)selector.DataContext).ApplicationTargets.Count > 0)
            {
                selector.SelectedIndex = 0;
            }
        };

        selector.AddHandler(Button.ClickEvent, hClick);

        DockPanel.SetDock(selector, Dock.Top);

        var sp = new StackPanel();
        sp.Orientation = Orientation.Vertical;
        DockPanel.SetDock(sp, Dock.Top);

        var createChk = (string txt, string prop, HorizontalAlignment? align) =>
        {
            var cb = new CheckBox() { Content = txt };
            if (align.HasValue) cb.HorizontalAlignment = align.Value;
            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding(prop));
            return cb;
        };
        var chkAllMatches = createChk("Reposition all matches (otherwise first only)", 
            nameof(PositionWindow.PositionAllMatches), HorizontalAlignment.Left);
        
        sp.Children.Add(chkAllMatches);

        var sp2 = new StackPanel();
        sp.Orientation = Orientation.Vertical;
        DockPanel.SetDock(sp2, Dock.Top);

        var chkRestore  = createChk("Restore", nameof(PositionWindow.IsRestore), HorizontalAlignment.Left);
        var chkMinimise = createChk("Minimise", nameof(PositionWindow.IsMinimise), HorizontalAlignment.Left);
        var chkMaximise = createChk("Maximise", nameof(PositionWindow.IsMaximise), HorizontalAlignment.Left);
        var chkPosition = createChk("Move/Size", nameof(PositionWindow.IsPositioned), HorizontalAlignment.Left);

        sp2.Children.Add(chkRestore);
        sp2.Children.Add(chkMinimise);
        sp2.Children.Add(chkMaximise);
        sp2.Children.Add(chkPosition);

        var position = new Grid();
        position.SetBinding(Grid.VisibilityProperty, new Binding(nameof(PositionWindow.PositionPanelVisibility)));
        position.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        position.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        position.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        position.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        position.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        position.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        position.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        position.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        position.ColumnDefinitions.Add(new ColumnDefinition() { });


        var chkMove = createChk("Move window", nameof(PositionWindow.SetPosition), null);
        var chkSize = createChk("Size window", nameof(PositionWindow.SetSize), null);

        var addToGrid = (int r, int c, FrameworkElement item) => 
            { Grid.SetRow(item, r); Grid.SetColumn(item, c); position.Children.Add(item); };

        var addSlider = (int r, string label, string propName, string enabledProp) =>
        {
            var lbl = new TextBlock { Text = label };
            var slider = new Slider();
            var txtValue = new TextBlock();
            slider.SetBinding(Slider.ValueProperty, propName);
            slider.SetBinding(Slider.IsEnabledProperty, enabledProp);
            txtValue.SetBinding(TextBlock.TextProperty, propName);

            slider.Minimum = -65536;
            slider.Maximum = 65536;
            slider.SmallChange = 1;
            slider.LargeChange = 50;
            slider.Width = 200;
            slider.HorizontalAlignment = HorizontalAlignment.Left;

            addToGrid(r, 0, lbl);
            addToGrid(r, 1, txtValue);
            addToGrid(r, 2, slider);
        };

        addToGrid(0, 0, chkMove);
        addSlider(1, "Left", nameof(PositionWindow.Left), nameof(PositionWindow.SetPosition));
        addSlider(2, "Top", nameof(PositionWindow.Top), nameof(PositionWindow.SetPosition));
        addToGrid(3, 0, chkSize);
        addSlider(4, "Width", nameof(PositionWindow.Width), nameof(PositionWindow.SetSize));
        addSlider(5, "Height", nameof(PositionWindow.Height), nameof(PositionWindow.SetSize));

        position.Margin = new Thickness(10, 10, 10, 0);

        ctrl.Children.Add(sp);
        ctrl.Children.Add(appTargetsLbl);
        ctrl.Children.Add(selector);
        ctrl.Children.Add(sp2);
        ctrl.Children.Add(position);

        return ctrl;
    }
}
