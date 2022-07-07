using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PowerOverlay.Commands;

public class LayoutDetails : DependencyObject // Need a DO to properly handle coercion constraints with data binding
{
    private const int maxMonitor = 10;

    public static readonly DependencyProperty MonitorProperty;

    public int Monitor
    {
        get { return (int)GetValue(MonitorProperty); }
        set { SetValue(MonitorProperty, value); }
    }

    static LayoutDetails()
    {
        MonitorProperty = DependencyProperty.Register(nameof(Monitor), typeof(int), typeof(LayoutDetails),
            new PropertyMetadata(0, OnMonitorChanged, CoerceMonitor));
    }

    private static object CoerceMonitor(DependencyObject d, object baseValue)
    {
        var newVal = (int)baseValue;
        if (newVal < 0) return 0;
        if (newVal > maxMonitor) return maxMonitor;
        return newVal;
    }

    private static void OnMonitorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

}

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
        Layout,
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
            RaisePropertyChanged(nameof(IsLayout));
            RaisePropertyChanged(nameof(IsRestore));
            RaisePropertyChanged(nameof(IsMinimise));
            RaisePropertyChanged(nameof(IsMaximise));
            RaisePropertyChanged(nameof(PositionPanelVisibility));
            RaisePropertyChanged(nameof(LayoutPanelVisibility));
        }
    }

    public Visibility PositionPanelVisibility
    {
        get
        {
            return (mode == PositionMode.Positioned) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public Visibility LayoutPanelVisibility
    {
        get
        {
            return (mode == PositionMode.Layout) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public bool IsPositioned
    {
        get { return mode == PositionMode.Positioned; }
        set { Mode = PositionMode.Positioned; }
    }
    public bool IsLayout
    {
        get { return mode == PositionMode.Layout; }
        set { Mode = PositionMode.Layout; }
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

    private LayoutDetails layoutDetails = new LayoutDetails();
    public LayoutDetails LayoutDetails => layoutDetails;

    private bool layoutTargetPrimaryMonitorIfNotMatched;
    private bool layoutKeepOnCurrentMonitor;
    private int layoutRowCount, layoutColumnCount;
    private int layoutRowSpan, layoutColumnSpan;
    private int layoutTargetRow, layoutTargetColumn;
    private int layoutOffsetLeft, layoutOffsetRight, layoutOffsetTop, layoutOffsetBottom;

    public bool LayoutKeepOnCurrentMonitor
    {
        get { return layoutKeepOnCurrentMonitor; }
        set {
            layoutKeepOnCurrentMonitor = value; 
            RaisePropertyChanged(nameof(LayoutKeepOnCurrentMonitor));
            RaisePropertyChanged(nameof(LayoutMonitorIsEnabled));
        }
    }

    public bool LayoutMonitorIsEnabled
    {
        get { return !LayoutKeepOnCurrentMonitor; }
    }

    public bool LayoutTargetPrimaryMonitorIfNotMatched
    {
        get { return layoutTargetPrimaryMonitorIfNotMatched; }
        set { layoutTargetPrimaryMonitorIfNotMatched = value; RaisePropertyChanged(nameof(LayoutTargetPrimaryMonitorIfNotMatched)); }
    }

    public int LayoutTargetRow { 
        get { return layoutTargetRow; }
        set {
            if (value < 0) value = 0;
            if (value >= LayoutRowCount) value = LayoutRowCount - 1;
            layoutTargetRow = value; RaisePropertyChanged(nameof(LayoutTargetRow)); 
        }
    }

    public int LayoutTargetColumn
    {
        get { return layoutTargetColumn; }
        set {
            if (value < 0) value = 0;
            if (value >= LayoutColumnCount) value = LayoutColumnCount - 1;
            layoutTargetColumn = value; RaisePropertyChanged(nameof(LayoutTargetColumn));
        }
    }

    public int LayoutRowSpan
    {
        get { return layoutRowSpan; }
        set {
            if (value < 1) value = 1;
            if (value >= LayoutRowCount) value = LayoutRowCount;
            layoutRowSpan = value; 
            RaisePropertyChanged(nameof(LayoutRowSpan)); 
        }
    }

    public int LayoutColumnSpan
    {
        get { return layoutColumnSpan; }
        set {
            if (value < 1) value = 1;
            if (value >= LayoutColumnCount) value = LayoutColumnCount;
            layoutColumnSpan = value;
            RaisePropertyChanged(nameof(LayoutColumnSpan));
        }
    }

    public int LayoutRowCount
    {
        get { return layoutRowCount; }
        set {
            if (value < 1) throw new ArgumentOutOfRangeException(nameof(LayoutRowCount));
            layoutRowCount = value; 
            RaisePropertyChanged(nameof(LayoutRowCount)); 
            while (LayoutRowWeights.Count > layoutRowCount)
            {
                LayoutRowWeights.RemoveAt(LayoutRowWeights.Count - 1);
            }
            while (LayoutRowWeights.Count < layoutRowCount)
            {
                LayoutRowWeights.Add(new LayoutWeight() { Weight = 1 });
            }
        }
    }

    public int LayoutColumnCount
    {
        get { return layoutColumnCount; }
        set {
            if (value < 1) throw new ArgumentOutOfRangeException(nameof(LayoutColumnCount));
            layoutColumnCount = value; 
            RaisePropertyChanged(nameof(LayoutColumnCount));
            while (LayoutColumnWeights.Count > layoutColumnCount)
            {
                LayoutColumnWeights.RemoveAt(LayoutColumnWeights.Count - 1);
            }
            while (LayoutColumnWeights.Count < layoutColumnCount)
            {
                LayoutColumnWeights.Add(new LayoutWeight() { Weight = 1 });
            }
        }
    }

    public int LayoutOffsetLeft
    {
        get { return layoutOffsetLeft; }
        set
        {
            layoutOffsetLeft = value;
            RaisePropertyChanged(nameof(LayoutOffsetLeft));
        }
    }
    public int LayoutOffsetRight
    {
        get { return layoutOffsetRight; }
        set
        {
            layoutOffsetRight = value;
            RaisePropertyChanged(nameof(LayoutOffsetRight));
        }
    }
    public int LayoutOffsetTop
    {
        get { return layoutOffsetTop; }
        set
        {
            layoutOffsetTop = value;
            RaisePropertyChanged(nameof(LayoutOffsetTop));
        }
    }
    public int LayoutOffsetBottom
    {
        get { return layoutOffsetBottom; }
        set
        {
            layoutOffsetBottom = value;
            RaisePropertyChanged(nameof(LayoutOffsetBottom));
        }
    }

    public ObservableCollection<LayoutWeight> LayoutRowWeights { get; private set; }
    public ObservableCollection<LayoutWeight> LayoutColumnWeights { get; private set; }

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
        LayoutRowWeights = new ObservableCollection<LayoutWeight>();
        LayoutColumnWeights = new ObservableCollection<LayoutWeight>();
        // Force weight construction
        LayoutColumnCount = 2;
        LayoutRowCount = 2;
        // These depend on row/column count
        LayoutRowSpan = 1;
        LayoutColumnSpan = 1;
        LayoutTargetRow = 0;
        LayoutTargetColumn = 0;
    }
    public override bool CanExecute(object? parameter)
    {
        if (ApplicationTargets.Count == 0) return false;
        switch (Mode)
        {
            case PositionMode.Positioned:
                return SetSize || SetPosition;
            case PositionMode.Layout:
                return true;
            default:
                return true;
        }
    }

    public override ActionCommand Clone()
    {
        var clone = new PositionWindow()
        {
            PositionAllMatches = PositionAllMatches,
            Mode = Mode,
            SetPosition = SetPosition,
            SetSize = SetSize,
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height,
            LayoutColumnCount = LayoutColumnCount,
            LayoutRowCount = LayoutRowCount,
            LayoutTargetRow = LayoutTargetRow,
            LayoutTargetColumn = LayoutTargetColumn,
            layoutKeepOnCurrentMonitor = LayoutKeepOnCurrentMonitor,
            LayoutTargetPrimaryMonitorIfNotMatched = LayoutTargetPrimaryMonitorIfNotMatched,
            LayoutRowSpan = LayoutRowSpan,
            LayoutColumnSpan = LayoutColumnSpan,
            LayoutOffsetLeft = LayoutOffsetLeft,
            LayoutOffsetRight = LayoutOffsetRight,
            LayoutOffsetTop = LayoutOffsetTop,
            LayoutOffsetBottom = LayoutOffsetBottom,
        };
        foreach (var target in ApplicationTargets) clone.ApplicationTargets.Add(target);
        int index;
        index = 0;
        foreach (var weight in LayoutColumnWeights)
        {
            clone.LayoutColumnWeights[index].Weight = weight.Weight;
            ++index;
        }
        index = 0;
        foreach (var weight in LayoutRowWeights)
        {
            clone.LayoutRowWeights[index].Weight = weight.Weight;
            ++index;
        }
        clone.LayoutDetails.Monitor = LayoutDetails.Monitor;
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
                NativeUtils.RepositionWindow(hwnd, SetPosition, SetSize, Left, Top, Width, Height);
                return;
            case PositionMode.Layout:
                LayoutWindow(hwnd);
                return;
        }
    }

    private void LayoutWindow(IntPtr hwnd)
    {
        var screen = GetScreenSizeForLayout(hwnd);
        if (! screen.HasValue) return;
        
        var windowLayout = CalculateWindowLayout(screen.Value);
        if (!windowLayout.HasValue) return;

        NativeUtils.RepositionWindow(hwnd, true, true,
            (int)windowLayout.Value.Left, (int)windowLayout.Value.Top,
            (int)windowLayout.Value.Width, (int)windowLayout.Value.Height);
    }

    private Rect? GetScreenSizeForLayout(IntPtr hwnd)
    {
        var screens = NativeUtils.GetDisplayCoordinates();
        if (screens == null) return null;

        int targetScreenIndex = -1;

        if (LayoutKeepOnCurrentMonitor)
        {
            IntPtr targetMonitor = NativeUtils.MonitorFromWindowOrPrimary(hwnd);
            targetScreenIndex = screens.FindIndex(x => x.hMonitor == targetMonitor);
        }
        else
        {
            targetScreenIndex = screens.Count > LayoutDetails.Monitor ? LayoutDetails.Monitor : -1;
            if ((targetScreenIndex == -1) && (LayoutTargetPrimaryMonitorIfNotMatched))
            {
                targetScreenIndex = screens.FindIndex(x => x.isPrimary);
            }
        }
        if (targetScreenIndex == -1) return null; // no match

        return screens[targetScreenIndex].clientRect;
    }

    private Rect? CalculateWindowLayout(Rect screen)
    {
        if (LayoutTargetRow >= LayoutRowCount) return null;
        if (LayoutTargetColumn >= LayoutColumnCount) return null;

        // screen layout calculation must be:
        // 1. deterministic
        // 2. complete => no gaps
        // 3. non-overlapping
        // 4. balanced => remainers shared appropriately

        Func<ObservableCollection<LayoutWeight>,int,int,List<int>> spread = (weights, first, last) =>
        {
            List<int> boundaries = new();
            int size = last - first;
            int bands = weights.Count;
            int units = weights.Sum(x => x.Weight);

            var remainder = size % units;
            var unitSize = size / units;

            List<int> sizes = new();
            foreach (var w in weights)
            {
                sizes.Add(unitSize * w.Weight);
            }

            // remainder is tricky:
            // imagine weights are: 2, 5, 3, 2
            // units = 2+5+3+2 = 12
            // remainder could be 11
            // what does a fair allocation look like?
            /*
             * Imagine we expand the weights to a priority list:
             * (index 0, weight 2)     1
             * (index 0, weight 1)         1
             * (index 1, weight 5) 1
             * (index 1, weight 4)  1
             * (index 1, weight 3)   1
             * (index 1, weight 2)     1
             * (index 1, weight 1)          1
             * (index 2, weight 3)    1
             * (index 2, weight 2)      1
             * (index 2, weight 1)           X
             * (index 3, weight 2)       1
             * (index 3, weight 2)        1
             * Then we allocate from highest priority to lowest
             * 
            // remainder could be 5
             * (index 0, weight 2)     1
             * (index 0, weight 1)      
             * (index 1, weight 5) 1
             * (index 1, weight 4)  1
             * (index 1, weight 3)   1
             * (index 1, weight 2)     
             * (index 1, weight 1)     
             * (index 2, weight 3)    1
             * (index 2, weight 2)     
             * (index 2, weight 1)     
             * (index 3, weight 2)     
             * (index 3, weight 2)     
             * 
             */
            var intIdentity = Microsoft.FSharp.Core.FuncConvert.FromFunc<int, int>(i => i);
            var integers = Microsoft.FSharp.Collections.SeqModule.Initialize<int>(1000, intIdentity);

            var indexedPriorityExpansion =
                weights
                .Select((x, i) => Tuple.Create(i, x.Weight)) // index,weight
                .SelectMany( t => integers.Take(t.Item2).Select(x => Tuple.Create(t.Item1, x)) ) // index, weight
                .OrderByDescending(x => x.Item2) // sort by decreasing weight
                .ThenBy(x => x.Item1) // and then by increasing index
                .ToList();

            int remainderPosition = 0;
            while (remainder > 0)
            {
                sizes[indexedPriorityExpansion[remainderPosition].Item1]++;
                ++remainderPosition;
                --remainder;
            }

            boundaries.Add(first);
            int previousPosition = first;
            foreach (var s in sizes)
            {
                boundaries.Add(previousPosition + s);
                previousPosition += s;
            }

            return boundaries;
        };

        var rowBoundaries = spread(LayoutRowWeights, (int)screen.Top, (int)screen.Bottom);
        var columnBoundaries = spread(LayoutColumnWeights, (int)screen.Left, (int)screen.Right);

        var result = new Rect();

        int columnBoundary = LayoutTargetColumn + LayoutColumnSpan;
        if (columnBoundary >= columnBoundaries.Count) columnBoundary = columnBoundaries.Count - 1;
        result.X = columnBoundaries[LayoutTargetColumn];
        result.Width = columnBoundaries[columnBoundary] - result.X;
        result.X += LayoutOffsetLeft;
        result.Width -= LayoutOffsetLeft;
        result.Width += LayoutOffsetRight;

        int rowBoundary = LayoutTargetRow + LayoutRowSpan;
        if (rowBoundary >= rowBoundaries.Count) rowBoundary = rowBoundaries.Count - 1;
        result.Y = rowBoundaries[LayoutTargetRow];
        result.Height = rowBoundaries[rowBoundary] - result.Y;
        result.Y += LayoutOffsetTop;
        result.Height -= LayoutOffsetTop;
        result.Height += LayoutOffsetBottom;

        return result;
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

    public override void WriteJson(JsonObject o)
    {
        if (ApplicationTargets.Count > 0)
        {
            o.AddLowerCamel(nameof(ApplicationTargets), ApplicationTargets.ToJson());
        }
        o.AddLowerCamel(nameof(PositionAllMatches), JsonValue.Create(PositionAllMatches));
        o.AddLowerCamel(nameof(Mode), JsonValue.Create(Mode.ToString().ToLowerCamelCase()));

        if (Mode == PositionMode.Positioned)
        {
            if (SetPosition)
            {
                o.AddLowerCamel(nameof(SetPosition), JsonValue.Create(SetPosition));
                o.AddLowerCamel(nameof(Left), JsonValue.Create(Left));
                o.AddLowerCamel(nameof(Top), JsonValue.Create(Top));
            }
            if (SetSize)
            {
                o.AddLowerCamel(nameof(SetSize), JsonValue.Create(SetSize));
                o.AddLowerCamel(nameof(Width), JsonValue.Create(Width));
                o.AddLowerCamel(nameof(Height), JsonValue.Create(Height));
            }
        }
        if (Mode == PositionMode.Layout)
        {
            o.AddLowerCamel(nameof(LayoutKeepOnCurrentMonitor), JsonValue.Create(LayoutKeepOnCurrentMonitor));
            if (! LayoutKeepOnCurrentMonitor)
            {
                o.AddLowerCamelValue(
                    $"{nameof(PositionWindow.LayoutDetails)}{nameof(LayoutDetails.Monitor)}",
                    LayoutDetails.Monitor);
                o.AddLowerCamelValue(nameof(LayoutTargetPrimaryMonitorIfNotMatched),
                    LayoutTargetPrimaryMonitorIfNotMatched);
            }
            o.AddLowerCamelValue(nameof(LayoutTargetRow), LayoutTargetRow);
            o.AddLowerCamelValue(nameof(LayoutTargetColumn), LayoutTargetColumn);
            o.AddLowerCamelValue(nameof(LayoutRowSpan), LayoutRowSpan);
            o.AddLowerCamelValue(nameof(LayoutColumnSpan), LayoutColumnSpan);
            o.AddLowerCamelValue(nameof(LayoutRowCount), LayoutRowCount);
            o.AddLowerCamelValue(nameof(LayoutColumnCount), LayoutColumnCount);
            o.AddLowerCamelValue(nameof(LayoutOffsetLeft), LayoutOffsetLeft);
            o.AddLowerCamelValue(nameof(LayoutOffsetRight), LayoutOffsetRight);
            o.AddLowerCamelValue(nameof(LayoutOffsetTop), LayoutOffsetTop);
            o.AddLowerCamelValue(nameof(LayoutOffsetBottom), LayoutOffsetBottom);
            o.AddLowerCamel(nameof(LayoutRowWeights),
                new JsonArray(
                    LayoutRowWeights.Select(x => JsonValue.Create<int>(x.Weight)).ToArray()));
            o.AddLowerCamel(nameof(LayoutColumnWeights),
                new JsonArray(
                    LayoutColumnWeights.Select(x => JsonValue.Create<int>(x.Weight)).ToArray()));
        }
    }

    public static PositionWindow CreateFromJson(JsonObject o)
    {
        var result = new PositionWindow();
        o.TryGet<JsonArray>(nameof(ApplicationTargets), xs => {
            foreach (var x in xs)
            {
                result.ApplicationTargets.Add(
                    ApplicationMatcherViewModel.FromJson(x)
                );
            }
        });
        o.TryGet<string>(nameof(Mode), m =>
        {
            result.Mode = Enum.Parse<PositionMode>(m, true);
        });
        switch (result.Mode)
        {
            case PositionMode.Minimise: break;
            case PositionMode.Restore: break;
            case PositionMode.Maximise: break;
            case PositionMode.Positioned:
                {
                    o.TryGetValue<bool>(nameof(SetSize), x => result.SetSize = x);
                    if (result.SetSize)
                    {
                        o.TryGetValue<int>(nameof(Width), x => result.Width = x);
                        o.TryGetValue<int>(nameof(Height), x => result.Height = x);
                    }
                    o.TryGetValue<bool>(nameof(SetPosition), x => result.SetPosition = x);
                    if (result.SetPosition)
                    {
                        o.TryGetValue<int>(nameof(Left), x => result.Left = x);
                        o.TryGetValue<int>(nameof(Top), x => result.Top = x);
                    }
                };
                break;
            case PositionMode.Layout:
                {
                    o.TryGetValue<int>(nameof(LayoutTargetRow), x => result.LayoutTargetRow = x);
                    o.TryGetValue<int>(nameof(LayoutTargetColumn), x => result.LayoutTargetColumn = x);
                    o.TryGetValue<int>(nameof(LayoutRowSpan), x => result.LayoutRowSpan = x);
                    o.TryGetValue<int>(nameof(LayoutColumnSpan), x => result.LayoutColumnSpan = x);
                    o.TryGetValue<int>(nameof(LayoutRowCount), x => result.LayoutRowCount = x);
                    o.TryGetValue<int>(nameof(LayoutColumnCount), x => result.LayoutColumnCount = x);
                    o.TryGetValue<int>(nameof(LayoutOffsetLeft), x => result.LayoutOffsetLeft = x);
                    o.TryGetValue<int>(nameof(LayoutOffsetRight), x => result.LayoutOffsetRight = x);
                    o.TryGetValue<int>(nameof(LayoutOffsetTop), x => result.LayoutOffsetTop = x);
                    o.TryGetValue<int>(nameof(LayoutOffsetBottom), x => result.LayoutOffsetBottom = x);

                    o.TryGetValue<bool>(nameof(LayoutKeepOnCurrentMonitor), x => result.LayoutKeepOnCurrentMonitor = x);
                    if (! result.LayoutKeepOnCurrentMonitor)
                    {
                        o.TryGetValue<int>(
                            $"{nameof(PositionWindow.LayoutDetails)}{nameof(PowerOverlay.Commands.LayoutDetails.Monitor)}",
                            x => result.LayoutDetails.Monitor = x);
                        o.TryGetValue<bool>(nameof(LayoutTargetPrimaryMonitorIfNotMatched), x => result.LayoutTargetPrimaryMonitorIfNotMatched = x);
                    }
                    if (o.ContainsKey(nameof(LayoutRowWeights)))
                    {
                        var weights = o[nameof(LayoutRowWeights)]!.AsArray();
                        int index = 0;
                        foreach (var w in weights)
                        {
                            if (index >= result.LayoutRowWeights.Count) break;
                            result.LayoutRowWeights[index].Weight = w?.GetValue<int>() ?? 1;
                            ++index;
                        }
                    }
                    if (o.ContainsKey(nameof(LayoutColumnWeights)))
                    {
                        var weights = o[nameof(LayoutColumnWeights)]!.AsArray();
                        int index = 0;
                        foreach (var w in weights)
                        {
                            if (index >= result.LayoutColumnWeights.Count) break;
                            result.LayoutColumnWeights[index].Weight = w?.GetValue<int>() ?? 1;
                            ++index;
                        }
                    }
                }
                break;
        }
        o.TryGetValue<bool>(nameof(PositionAllMatches), b => result.PositionAllMatches = b);

        return result;
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
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return PositionWindow.CreateFromJson(o);
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
        var chkLayout = createChk("Layout", nameof(PositionWindow.IsLayout), HorizontalAlignment.Left);

        sp2.Children.Add(chkRestore);
        sp2.Children.Add(chkMinimise);
        sp2.Children.Add(chkMaximise);
        sp2.Children.Add(chkPosition);
        sp2.Children.Add(chkLayout);

        var position = new Grid();
        {
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

            var addNumericTextBox = (int r, string label, string propName, string enabledProp) =>
            {
                var lbl = new TextBlock { Text = label };
                var ntb = new NumericTextBox();
                ntb.SetBinding(NumericTextBox.ValueProperty, propName);
                ntb.SetBinding(NumericTextBox.IsEnabledProperty, enabledProp);

                ntb.MinValue = -65536;
                ntb.MaxValue = 65536;
                ntb.Width = 100;
                ntb.HorizontalAlignment = HorizontalAlignment.Left;

                addToGrid(r, 0, lbl);
                addToGrid(r, 2, ntb);
            };

            addToGrid(0, 0, chkMove);
            addNumericTextBox(1, "Left", nameof(PositionWindow.Left), nameof(PositionWindow.SetPosition));
            addNumericTextBox(2, "Top", nameof(PositionWindow.Top), nameof(PositionWindow.SetPosition));
            addToGrid(3, 0, chkSize);
            addNumericTextBox(4, "Width", nameof(PositionWindow.Width), nameof(PositionWindow.SetSize));
            addNumericTextBox(5, "Height", nameof(PositionWindow.Height), nameof(PositionWindow.SetSize));

            position.Margin = new Thickness(10, 10, 10, 0);

        }

        var layout = new Grid();
        {
            layout.SetBinding(Grid.VisibilityProperty, new Binding(nameof(PositionWindow.LayoutPanelVisibility)));
            layout.Margin = new Thickness(10, 10, 10, 0);
            layout.MaxWidth = 600;
            layout.HorizontalAlignment = HorizontalAlignment.Left;

            for (int i = 0; i < 10; ++i)
            {
                layout.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }
            layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            int currentRowIndex = 0;
            var addToGrid = (int c, FrameworkElement item) =>
            { Grid.SetRow(item, currentRowIndex); Grid.SetColumn(item, c); layout.Children.Add(item);
                item.Margin = new Thickness(5);
            };

            var positionDisplayGrid = new Grid()
            {
                Width = 400, Height = 225,
                ShowGridLines = true,
                Background = XamlUtils.SolidColourBrush(null, System.Windows.Media.Colors.Blue),
            };

            var lblMonitor = new Label() { Content = "Monitor", VerticalAlignment = VerticalAlignment.Center };
            var chkActiveMonitor = new CheckBox() { Content = "Keep window on current monitor", VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumnSpan(chkActiveMonitor, 3);
            chkActiveMonitor.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(PositionWindow.LayoutKeepOnCurrentMonitor)));
            var ntbMonitor = new NumericTextBox() { MaxValue = 10, MinValue = 0 };
            ntbMonitor.SetBinding(NumericTextBox.ValueProperty,
                new Binding($"{nameof(PositionWindow.LayoutDetails)}.{nameof(LayoutDetails.Monitor)}"));
            ntbMonitor.SetBinding(UIElement.IsEnabledProperty, new Binding(nameof(PositionWindow.LayoutMonitorIsEnabled)));
            var chkPrimaryMonitor = new CheckBox() { Content = "Apply to primary monitor if selected unavailable" };
            chkPrimaryMonitor.SetBinding(UIElement.IsEnabledProperty, new Binding(nameof(PositionWindow.LayoutMonitorIsEnabled)));
            chkPrimaryMonitor.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(PositionWindow.LayoutTargetPrimaryMonitorIfNotMatched)));
            Grid.SetColumnSpan(chkPrimaryMonitor, 3);

            var lb2 = new Label() { Content = "Row" };
            var ntb2 = new NumericTextBox() { MinValue = 0, MaxValue = 0 };
            ntb2.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutTargetRow)));
            var lb2b = new Label() { Content = "Span" };
            var ntb2b = new NumericTextBox() { MinValue = 1, MaxValue = 1 };
            ntb2b.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutRowSpan)));
            var lb3 = new Label() { Content = "Column" };
            var ntb3 = new NumericTextBox() { MinValue = 0, MaxValue = 0 };
            ntb3.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutTargetColumn)));
            var lb3b = new Label() { Content = "Span" };
            var ntb3b = new NumericTextBox() { MinValue = 1, MaxValue = 1 };
            ntb3b.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutColumnSpan)));

            var lblRowCount = new Label() { Content = "Row count" };
            var ntbRowCount = new NumericTextBox() { MinValue = 1, MaxValue = 10 };
            ntbRowCount.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutRowCount)));
            ntbRowCount.ValueChanged += (sender, e) =>
            {
                var rowCount = ntbRowCount.Value;
                ntb2.MaxValue = rowCount - 1;
                ntb2b.MaxValue = rowCount - 1;
                while (positionDisplayGrid.RowDefinitions.Count > rowCount)
                {
                    positionDisplayGrid.RowDefinitions.RemoveAt(positionDisplayGrid.RowDefinitions.Count - 1);
                }
                while (positionDisplayGrid.RowDefinitions.Count < rowCount)
                {
                    var rd = new RowDefinition();
                    rd.SetBinding(RowDefinition.HeightProperty, new Binding(
                        $"{nameof(PositionWindow.LayoutRowWeights)}" +
                        $"[{positionDisplayGrid.RowDefinitions.Count}]" +
                        $".{nameof(LayoutWeight.GridLength)}"));
                    positionDisplayGrid.RowDefinitions.Add(rd);
                }
            };
            var lblColumnCount = new Label() { Content = "Column count" };
            var ntbColumnCount = new NumericTextBox() { MinValue = 1, MaxValue = 10 };
            ntbColumnCount.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutColumnCount)));
            ntbColumnCount.ValueChanged += (sender, e) =>
            {
                var columnCount = ntbColumnCount.Value;
                ntb3.MaxValue = columnCount - 1;
                ntb3b.MaxValue = columnCount - 1;
                while (positionDisplayGrid.ColumnDefinitions.Count > columnCount)
                {
                    positionDisplayGrid.ColumnDefinitions.RemoveAt(positionDisplayGrid.ColumnDefinitions.Count - 1);
                }
                while (positionDisplayGrid.ColumnDefinitions.Count < columnCount)
                {
                    var cd = new ColumnDefinition();
                    cd.SetBinding(ColumnDefinition.WidthProperty, new Binding(
                        $"{nameof(PositionWindow.LayoutColumnWeights)}" +
                        $"[{positionDisplayGrid.ColumnDefinitions.Count}]" +
                        $".{nameof(LayoutWeight.GridLength)}"));
                    positionDisplayGrid.ColumnDefinitions.Add(cd);
                }
            };

            var lblOffsetLeft = new Label() { Content = "Offset left" };
            var lblOffsetRight = new Label() { Content = "Offset right" };
            var lblOffsetTop = new Label() { Content = "Offset top" };
            var lblOffsetBottom = new Label() { Content = "Offset bottom" };
            var ntbOffsetLeft = new NumericTextBox() { MinValue = -100000, MaxValue = 100000 };
            ntbOffsetLeft.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutOffsetLeft)));
            var ntbOffsetRight = new NumericTextBox() { MinValue = -100000, MaxValue = 100000 };
            ntbOffsetRight.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutOffsetRight)));
            var ntbOffsetTop = new NumericTextBox() { MinValue = -100000, MaxValue = 100000 };
            ntbOffsetTop.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutOffsetTop)));
            var ntbOffsetBottom = new NumericTextBox() { MinValue = -100000, MaxValue = 100000 };
            ntbOffsetBottom.SetBinding(NumericTextBox.ValueProperty, new Binding(nameof(PositionWindow.LayoutOffsetBottom)));

            var lblRowWeights = new Label() { Content = "Row weights:" };
            var lbRowWeights = new ItemsControl()
            {
                Width = 200,
                MaxHeight = 300,
            };
            lbRowWeights.Style = (Style)lbRowWeights.FindResource("LayoutSizing");
            lbRowWeights.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(PositionWindow.LayoutRowWeights)));

            var lblColumnWeights = new Label() { Content = "Column Weights:" };
            var lbColumnWeights = new ItemsControl()
            {
                Width = 200,
                MaxHeight = 300,
            };
            lbColumnWeights.Style = (Style)lblColumnWeights.FindResource("LayoutSizing");
            lbColumnWeights.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(PositionWindow.LayoutColumnWeights)));

            addToGrid(0, lblMonitor);
            addToGrid(1, chkActiveMonitor);
            ++currentRowIndex;
            addToGrid(1, ntbMonitor);
            ++currentRowIndex;
            addToGrid(1, chkPrimaryMonitor);
            ++currentRowIndex;
            addToGrid(0, lb2);
            addToGrid(1, ntb2);
            addToGrid(2, lb2b);
            addToGrid(3, ntb2b);
            ++currentRowIndex;
            addToGrid(0, lb3);
            addToGrid(1, ntb3);
            addToGrid(2, lb3b);
            addToGrid(3, ntb3b);
            ++currentRowIndex;
            addToGrid(0, lblRowCount);
            addToGrid(1, ntbRowCount);
            addToGrid(2, lblColumnCount);
            addToGrid(3, ntbColumnCount);
            ++currentRowIndex;

            addToGrid(0, positionDisplayGrid);
            Grid.SetColumnSpan(positionDisplayGrid, 4);
            ++currentRowIndex;

            addToGrid(0, lblOffsetLeft);
            addToGrid(1, ntbOffsetLeft);
            addToGrid(2, lblOffsetRight);
            addToGrid(3, ntbOffsetRight);
            ++currentRowIndex;
            addToGrid(0, lblOffsetTop);
            addToGrid(1, ntbOffsetTop);
            addToGrid(2, lblOffsetBottom);
            addToGrid(3, ntbOffsetBottom);
            ++currentRowIndex;

            addToGrid(0, lblRowWeights);
            addToGrid(1, lbRowWeights);
            addToGrid(2, lblColumnWeights);
            addToGrid(3, lbColumnWeights);
            ++currentRowIndex;

            var panel = new StackPanel() { Background = XamlUtils.SolidColourBrush(null, System.Windows.Media.Colors.Orange) };
            panel.SetBinding(Grid.RowProperty, new Binding()
            {
                Source = ntb2,
                Path = new PropertyPath("Value"),
            });
            panel.SetBinding(Grid.RowSpanProperty, new Binding()
            {
                Source = ntb2b,
                Path = new PropertyPath("Value"),
            });
            panel.SetBinding(Grid.ColumnProperty, new Binding()
            {
                Source = ntb3,
                Path = new PropertyPath("Value"),
            });
            panel.SetBinding(Grid.ColumnSpanProperty, new Binding()
            {
                Source = ntb3b,
                Path = new PropertyPath("Value"),
            });

            positionDisplayGrid.Children.Add(panel);

        }

        ctrl.Children.Add(sp);
        ctrl.Children.Add(appTargetsLbl);
        ctrl.Children.Add(selector);
        ctrl.Children.Add(sp2);
        ctrl.Children.Add(position);
        ctrl.Children.Add(layout);

        return ctrl;
    }
}
