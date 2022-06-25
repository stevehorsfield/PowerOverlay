using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace overlay_popup.Commands;

public class ExecuteCommandArgument : INotifyPropertyChanged
{
    private string argument = String.Empty;
    public event PropertyChangedEventHandler PropertyChanged;
    public string Argument
    {
        get { return argument; }
        set { argument = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Argument")); }
    }

    public static implicit operator String(ExecuteCommandArgument a) => a.Argument;
    public static implicit operator ExecuteCommandArgument(string a) => new ExecuteCommandArgument { Argument = a };
}

public class ExecuteCommand : ActionCommand
{

    public override ExecuteCommandDefinition Definition { get { return ExecuteCommandDefinition.Instance; } }

    private const string NotSpecifiedVerb = "(not specified)";

    private readonly FrameworkElement configElement = ExecuteCommandDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    private string executablePath = String.Empty;
    public string ExecutablePath
    {
        get { return executablePath; }
        set
        {
            executablePath = value;
            RaisePropertyChanged(nameof(ExecutablePath));
            UpdateVerbList();
        }
    }

    private string workingDirectory = String.Empty;
    public string WorkingDirectory
    {
        get { return workingDirectory; }
        set
        {
            workingDirectory = value;
            RaisePropertyChanged(nameof(WorkingDirectory));
        }
    }

    private bool shellExecute;
    public bool ShellExecute
    {
        get { return shellExecute; }
        set
        {
            shellExecute = value;
            RaisePropertyChanged(nameof(ShellExecute));
        }
    }

    private bool waitForInputIdle;
    public bool WaitForInputIdle
    {
        get { return waitForInputIdle; }
        set
        {
            waitForInputIdle = value;
            RaisePropertyChanged(nameof(WaitForInputIdle));
        }
    }

    private bool waitForProcessExit;
    public bool WaitForProcessExit
    {
        get { return waitForProcessExit; }
        set
        {
            waitForProcessExit = value;
            RaisePropertyChanged(nameof(WaitForProcessExit));
        }
    }

    private int waitTimeoutMilliseconds = 5000;
    public int WaitTimeoutMilliseconds
    {
        get { return waitTimeoutMilliseconds; }
        set
        {
            waitTimeoutMilliseconds = value;
            RaisePropertyChanged(nameof(WaitTimeoutMilliseconds));
        }
    }

    private readonly ObservableCollection<ExecuteCommandArgument> arguments = new();
    public ObservableCollection<ExecuteCommandArgument> Arguments => arguments;

    private readonly ObservableCollection<string> verbs = new ObservableCollection<string>();
    public ObservableCollection<string> Verbs => verbs;

    public string verb = NotSpecifiedVerb;
    public string Verb
    {
        get { return verb; }
        set
        {
            verb = value;
            var view = CollectionViewSource.GetDefaultView(Verbs)!;
            view.MoveCurrentTo(value);
            RaisePropertyChanged(nameof(Verb));
        }
    }

    private void UpdateVerbList()
    {
        var view = CollectionViewSource.GetDefaultView(Verbs)!;
        if (view.CurrentPosition == -1) view.MoveCurrentToPosition(0); // Always has the default option

        if (String.IsNullOrWhiteSpace(ExecutablePath))
        {
            for (int i = Verbs.Count - 1; i > 0; i--)
            {
                Verbs.RemoveAt(i);
            }
            view.MoveCurrentToPosition(0);
            return;
        }

        var newVerbs = new HashSet<string>(new ProcessStartInfo(ExecutablePath).Verbs);
        var oldVerbs = Verbs.Select((val, index) => (index, val)).ToDictionary(x => x.val, x => x.index);
        var removals = new List<int>();

        foreach (var x in oldVerbs)
        {
            if (newVerbs.Contains(x.Key)) continue;
            if (String.Equals(x.Key, NotSpecifiedVerb)) continue;
            removals.Add(x.Value);
        }
        removals.Sort();
        removals.Reverse();
        foreach (var x in removals)
        {
            Verbs.RemoveAt(x);
        }
        foreach (var x in newVerbs)
        {
            if (oldVerbs.ContainsKey(x)) continue;
            Verbs.Add(x);
        }

        if (! newVerbs.Contains(Verb))
        {
            Verb = NotSpecifiedVerb;
            view.MoveCurrentTo(Verbs[0]);
        }
    }

    public override bool CanExecute(object? parameter)
    {
        if (String.IsNullOrEmpty(ExecutablePath)) return false;
        if (!File.Exists(ExecutablePath) && !ShellExecute) return false; // Shell can execute URLs, for example

        return true;
    }

    public ExecuteCommand()
    {
        Verbs.Add(NotSpecifiedVerb);
        UpdateVerbList();
    }

    public override ActionCommand Clone()
    {
        var clone = new ExecuteCommand() { 
            ExecutablePath = ExecutablePath,
            ShellExecute = ShellExecute,
            WorkingDirectory = WorkingDirectory,
            WaitForInputIdle = WaitForInputIdle,
            WaitForProcessExit = WaitForProcessExit,
            WaitTimeoutMilliseconds = WaitTimeoutMilliseconds,
            Verb = Verb,
        };
        foreach (var x in Arguments) clone.Arguments.Add(x);
        return clone;
    }

    public override void Execute(object? parameter)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = ExecutablePath;
            startInfo.Verb = Verb == NotSpecifiedVerb ? String.Empty : Verb;
            startInfo.UseShellExecute = ShellExecute;
            startInfo.WorkingDirectory = WorkingDirectory;
            foreach (var x in Arguments) startInfo.ArgumentList.Add(x);

            using var result = Process.Start(startInfo);
            if (result != null)
            {
                if (WaitForInputIdle)
                {
                    result.WaitForInputIdle(WaitTimeoutMilliseconds);
                }
                if (WaitForProcessExit)
                {
                    result.WaitForExit(WaitTimeoutMilliseconds);
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show($"Failed to launch process '{executablePath}'. Error: '{e.Message}'");
        }
    }
}

public class ExecuteCommandDefinition : ActionCommandDefinition
{
    public static ExecuteCommandDefinition Instance = new();

    public override string ActionName => "ExecuteCommand";
    public override string ActionDisplayName => "Execute a command";

    public override ActionCommand Create()
    {
        return new ExecuteCommand();
    }

    public override FrameworkElement CreateConfigElement()
    {
        return new ExecuteCommandControl();
    }
}

public class ExecuteCommandControl : ContentControl, System.Windows.Forms.IWin32Window
{
    public ExecuteCommandControl()
    {
        InitializeComponent();
    }

    public TextBox FilenameTextBox { get; private set; }
    public ComboBox VerbComboBox { get; private set; }
    public TextBox WorkingDirectoryTextBox { get; private set; }

    private WindowInteropHelper? helper;
    public IntPtr Handle
    {
        get
        {
            if (helper == null) helper = new WindowInteropHelper(Window.GetWindow(this));
            return helper.Handle;
        }
    }

    private bool _contentLoaded;

    public void InitializeComponent()
    {
        if (_contentLoaded) return;
        _contentLoaded = true;

        var ctrl = new Grid();
        int currentGridRow = 0;

        ctrl.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(220) });
        ctrl.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(300) });

        #region ExecutablePath

        {
            var l1 = new Label() { Content = "Program:" };
            Grid.SetRow(l1, currentGridRow); Grid.SetColumn(l1, 0);
            ctrl.Children.Add(l1);

            var d1 = new DockPanel();
            FilenameTextBox = new TextBox();
            FilenameTextBox.Name = "FilenameTextBox";
            var binding = new Binding("ExecutablePath");
            FilenameTextBox.SetBinding(TextBox.TextProperty, binding);
            var b1 = new Button() { Width = 20, Margin = new Thickness(10, 0, 0, 0) };
            b1.Content = "...";
            b1.Click += FilenameSelector_Click;
            DockPanel.SetDock(b1, Dock.Right);
            d1.Children.Add(b1);
            d1.Children.Add(FilenameTextBox);

            Grid.SetRow(d1, currentGridRow); Grid.SetColumn(d1, 1);
            ctrl.Children.Add(d1);
            ++currentGridRow;

            var l2 = new Label() { Content = "Execute via shell:" };
            Grid.SetRow(l2, currentGridRow); Grid.SetColumn(l2, 0);
            ctrl.Children.Add(l2);
            var cb2 = new CheckBox();
            Grid.SetRow(cb2, currentGridRow); Grid.SetColumn(cb2, 1);
            binding = new Binding(nameof(ExecuteCommand.ShellExecute));
            cb2.SetBinding(CheckBox.IsCheckedProperty, binding);
            ctrl.Children.Add(cb2);
            ++currentGridRow;
        }

        #endregion

        #region Verbs

        {
            var l2 = new Label() { Content = "Verb:" };
            Grid.SetRow(l2, currentGridRow); Grid.SetColumn(l2, 0);
            ctrl.Children.Add(l2);

            VerbComboBox = new ComboBox
            {
                Name = "VerbComboBox"
            };
            var binding = new Binding("Verb");
            VerbComboBox.SetBinding(ComboBox.SelectedValueProperty, binding);
            binding = new Binding("Verbs");
            VerbComboBox.SetBinding(ComboBox.ItemsSourceProperty, binding);
            Grid.SetRow(VerbComboBox, currentGridRow); Grid.SetColumn(VerbComboBox, 1);
            ctrl.Children.Add(VerbComboBox);
            ++currentGridRow;
        }

        #endregion

        {
            var l3 = new Label() { Content = "Working Directory:" };
            Grid.SetRow(l3, currentGridRow); Grid.SetColumn(l3, 0);
            ctrl.Children.Add(l3);

            var d1 = new DockPanel();
            WorkingDirectoryTextBox = new TextBox();
            WorkingDirectoryTextBox.Name = "WorkingDirectoryTextBox";
            var binding = new Binding("WorkingDirectory");
            WorkingDirectoryTextBox.SetBinding(TextBox.TextProperty, binding);
            var b1 = new Button() { Width = 20, Margin = new Thickness(10, 0, 0, 0) };
            b1.Content = "...";
            b1.Click += WorkingDirectorySelector_Click;
            DockPanel.SetDock(b1, Dock.Right);
            d1.Children.Add(b1);
            d1.Children.Add(WorkingDirectoryTextBox);

            Grid.SetRow(d1, currentGridRow); Grid.SetColumn(d1, 1);
            ctrl.Children.Add(d1);
            ++currentGridRow;
        }

        {
            var l4 = new Label() { Content = "Arguments:" };
            Grid.SetRow(l4, currentGridRow); Grid.SetColumn(l4, 0);
            ctrl.Children.Add(l4);

            var sp = new StackPanel() {  Orientation = Orientation.Horizontal };
            Grid.SetRow(sp, currentGridRow); Grid.SetColumn(sp, 1);
            ctrl.Children.Add(sp);

            sp.Children.Add(new Button() { Content = "Add", HorizontalAlignment = HorizontalAlignment.Left, MinWidth = 80, });
            sp.Children.Add(new Button() { Content = "Remove", HorizontalAlignment = HorizontalAlignment.Left, MinWidth = 80, });
            ((Button)sp.Children[0]).Click += (o, e) => ((ExecuteCommand)DataContext).Arguments.Add(String.Empty);
            ((Button)sp.Children[1]).Click += (o, e) =>
            {
                var dc = ((ExecuteCommand)DataContext);
                var view = CollectionViewSource.GetDefaultView(dc.Arguments);
                var index = view.CurrentPosition; 
                if (index == -1) return;
                dc.Arguments.RemoveAt(index);
                if (index > 0) view.MoveCurrentToPosition(index - 1);
                else if (dc.Arguments.Count > 0) view.MoveCurrentToPosition(0);
            };
            ++currentGridRow;

            var lv = new ListBox();
            lv.SetBinding(ListBox.ItemsSourceProperty, new Binding("Arguments"));
            lv.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            lv.IsSynchronizedWithCurrentItem = true;
            lv.GotFocus += (o, e) =>
            {
                if (Object.ReferenceEquals(e.OriginalSource, lv)) return;
                if (!(e.OriginalSource is TextBox)) return;

                var current= (DependencyObject) e.OriginalSource;
                while (current != null && !(current is ListBox)) {
                    current = VisualTreeHelper.GetParent(current);
                    if (current is ListBoxItem)
                    {
                        ((ListBoxItem)current).IsSelected = true;
                    }
                }
            };

            var xaml = Encoding.UTF8.GetBytes(ArgumentsDataTemplateXaml);
            using var ms = new MemoryStream(xaml);
            var dataTemplate = (DataTemplate) XamlReader.Load(ms);
            lv.ItemTemplate = dataTemplate;

            Grid.SetRow(lv, currentGridRow); Grid.SetColumn(lv, 1);
            ctrl.Children.Add(lv);
            ++currentGridRow;
        }

        {
            var l5 = new Label() { Content = "Wait for ready:" };
            Grid.SetRow(l5, currentGridRow); Grid.SetColumn(l5, 0);
            ctrl.Children.Add(l5);

            var cb5 = new CheckBox();
            Grid.SetRow(cb5, currentGridRow); Grid.SetColumn(cb5, 1);
            var binding = new Binding(nameof(ExecuteCommand.WaitForInputIdle));
            cb5.SetBinding(CheckBox.IsCheckedProperty, binding);
            ctrl.Children.Add(cb5);
            ++currentGridRow;

            var l6 = new Label() { Content = "Wait for exit:" };
            Grid.SetRow(l6, currentGridRow); Grid.SetColumn(l6, 0);
            ctrl.Children.Add(l6);

            var cb6 = new CheckBox();
            Grid.SetRow(cb6, currentGridRow); Grid.SetColumn(cb6, 1);
            binding = new Binding(nameof(ExecuteCommand.WaitForProcessExit));
            cb6.SetBinding(CheckBox.IsCheckedProperty, binding);
            ctrl.Children.Add(cb6);
            ++currentGridRow;

            var l7 = new Label() { Content = "Timeout (ms):" };
            Grid.SetRow(l7, currentGridRow); Grid.SetColumn(l7, 0);
            ctrl.Children.Add(l7);

            var s7 = new Slider();
            Grid.SetRow(s7, currentGridRow); Grid.SetColumn(s7, 1);
            binding = new Binding(nameof(ExecuteCommand.WaitTimeoutMilliseconds));
            s7.SetBinding(Slider.ValueProperty, binding);
            s7.Minimum = 0;
            s7.Maximum = 60000;
            s7.LargeChange = 5000;
            s7.SmallChange = 500;
            ctrl.Children.Add(s7);
            ++currentGridRow;
        }


        for (int i = 0; i < currentGridRow; ++i) {
            ctrl.RowDefinitions.Add(new RowDefinition() { });
        }

        this.Content = ctrl;
    }

    private void FilenameSelector_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog();
        ofd.FileName = FilenameTextBox.Text;
        ofd.AddExtension = false;
        ofd.CheckFileExists = true;
        ofd.ShowReadOnly = false;
        ofd.Title = "Select file to execute";
        ofd.Multiselect = false;
        ofd.CustomPlaces.Clear();
        ofd.CustomPlaces.Add(
            new FileDialogCustomPlace(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)));
        ofd.CustomPlaces.Add(
            new FileDialogCustomPlace(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)));
        ofd.DefaultExt = ".exe";
        ofd.Filter = "Executables (*.exe)|*.exe|All files|*.*";
        ofd.FilterIndex = FilenameTextBox.Text?.EndsWith("*.exe") ?? false ? 0 : 1;
        var result = ofd.ShowDialog(Window.GetWindow(this));
        if (result ?? false)
        {
            FilenameTextBox.Text = ofd.FileName;
        }
    }

    private void WorkingDirectorySelector_Click(object sender, RoutedEventArgs e)
    {
        var fbd = new FolderBrowserDialog();

        fbd.InitialDirectory = WorkingDirectoryTextBox.Text;
        fbd.ShowNewFolderButton = false;
        var dialogResult = fbd.ShowDialog(this);

        if (dialogResult != System.Windows.Forms.DialogResult.OK) return;

        WorkingDirectoryTextBox.Text = fbd.SelectedPath;
    }

    private const string ArgumentsDataTemplateXaml = @"<DataTemplate 
        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
        >
    <TextBox Text=""{Binding Path=.Argument,Mode=TwoWay}"" HorizontalAlignment=""Stretch"" />
</DataTemplate>";
}