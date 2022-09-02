using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Windows.Media.Animation;
using PowerOverlay.IPC;

namespace PowerOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ICommand
    {
        private bool lockActive = false;
        private readonly Storyboard MessageDisplayBoxStoryboard;
        private readonly AppSettings settings;
        public AppSettings Settings => settings;

        public MainWindow()
        {
            settings = AppSettings.Get();

            InitializeComponent();

            DisplayGrid.Width = settings.MainWindowWidth;
            DisplayGrid.Height = settings.MainWindowHeight;

            settings.PropertyChanged += Settings_PropertyChanged;
            ApplyZoom();

            this.DataContext = settings.AppViewModel;
            //((AppViewModel)this.DataContext).AddTestData();

            MessageDisplayBox.Visibility = Visibility.Collapsed;
            MessageDisplayBox.Text = "";
            MessageDisplayBoxStoryboard = (Storyboard)MessageDisplayBox.FindResource("HideMessage");

            this.InputBindings.Add(
                new InputBinding(this, 
                    new KeyGesture(Key.C, ModifierKeys.Alt)) 
                { CommandParameter = "Configure" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.M, ModifierKeys.Alt))
                { CommandParameter = "Menu" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.N, ModifierKeys.Control))
                { CommandParameter = "New" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.O, ModifierKeys.Control))
                { CommandParameter = "Open" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.S, ModifierKeys.Control))
                { CommandParameter = "Save" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.L, ModifierKeys.Control))
                { CommandParameter = "Lock" });
            this.InputBindings.Add(
                new InputBinding(this,
                new KeyGesture(Key.Add, ModifierKeys.Control))
                { CommandParameter = "ZoomMore" });
            this.InputBindings.Add(
                new InputBinding(this,
                new KeyGesture(Key.OemPlus, ModifierKeys.Control))
                { CommandParameter = "ZoomMore" });
            this.InputBindings.Add(
                new InputBinding(this,
                new KeyGesture(Key.Subtract, ModifierKeys.Control))
                { CommandParameter = "ZoomLess" });
            this.InputBindings.Add(
                new InputBinding(this,
                new KeyGesture(Key.OemMinus, ModifierKeys.Control))
                { CommandParameter = "ZoomLess" });

            #region Numeric key combination bindings

            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D1, ModifierKeys.Control))
                { CommandParameter = "Control1" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D2, ModifierKeys.Control))
                { CommandParameter = "Control2" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D3, ModifierKeys.Control))
                { CommandParameter = "Control3" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D4, ModifierKeys.Control))
                { CommandParameter = "Control4" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D5, ModifierKeys.Control))
                { CommandParameter = "Control5" });

            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D1, ModifierKeys.Alt))
                { CommandParameter = "Alt1" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D2, ModifierKeys.Alt))
                { CommandParameter = "Alt2" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D3, ModifierKeys.Alt))
                { CommandParameter = "Alt3" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D4, ModifierKeys.Alt))
                { CommandParameter = "Alt4" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D5, ModifierKeys.Alt))
                { CommandParameter = "Alt5" });

            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D1, ModifierKeys.Control | ModifierKeys.Shift))
                { CommandParameter = "ControlShift1" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D2, ModifierKeys.Control | ModifierKeys.Shift))
                { CommandParameter = "ControlShift2" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D3, ModifierKeys.Control | ModifierKeys.Shift))
                { CommandParameter = "ControlShift3" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D4, ModifierKeys.Control | ModifierKeys.Shift))
                { CommandParameter = "ControlShift4" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D5, ModifierKeys.Control | ModifierKeys.Shift))
                { CommandParameter = "ControlShift5" });

            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D1, ModifierKeys.Shift | ModifierKeys.Alt))
                { CommandParameter = "AltShift1" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D1, ModifierKeys.Shift | ModifierKeys.Alt))
                { CommandParameter = "AltShift2" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D1, ModifierKeys.Shift | ModifierKeys.Alt))
                { CommandParameter = "AltShift3" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D1, ModifierKeys.Shift | ModifierKeys.Alt))
                { CommandParameter = "AltShift4" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D1, ModifierKeys.Shift | ModifierKeys.Alt))
                { CommandParameter = "AltShift5" });

            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D1, ModifierKeys.Control | ModifierKeys.Alt))
                { CommandParameter = "ControlAlt1" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D2, ModifierKeys.Control | ModifierKeys.Alt))
                { CommandParameter = "ControlAlt2" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D3, ModifierKeys.Control | ModifierKeys.Alt))
                { CommandParameter = "ControlAlt3" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D4, ModifierKeys.Control | ModifierKeys.Alt))
                { CommandParameter = "ControlAlt4" });
            this.InputBindings.Add(
                new InputBinding(this,
                    new KeyGesture(Key.D5, ModifierKeys.Control | ModifierKeys.Alt))
                { CommandParameter = "ControlAlt5" });

            #endregion

            for (int i = 0; i < 5; ++i) {
                for (int j = 0; j < 5; ++j) {
                    if (i == 2 && j == 2) continue; // skip middle location

                    var ctrl = new CommandButton();

                    Grid.SetRow(ctrl, i);
                    Grid.SetColumn(ctrl, j);

                    var binding = new Binding($".[{i},{j}]");
                    ctrl.SetBinding(FrameworkElement.DataContextProperty, binding);

                    ButtonGrid.Children.Add(ctrl);
                }
            }
        }

        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AppSettings.DisplayZoom):
                    ApplyZoom();
                    return;
                case nameof(AppSettings.MainWindowHeight):
                    DisplayGrid.Width = settings.MainWindowHeight;
                    ApplyZoom();
                    return;
                case nameof(AppSettings.MainWindowWidth):
                    DisplayGrid.Width = settings.MainWindowWidth;
                    ApplyZoom();
                    return;
                case nameof(AppSettings.ShowOnScreenNumber):
                    if (this.Visibility == Visibility.Visible) ApplyLayout(); // no need to do it otherwise
                    return;
            }
        }

        public void HandleIPC(Message msg)
        {
            DebugLog.Log($"IPC received: {msg.Action}");
            switch (msg.Action)
            {
                case MessageAction.Activate:
                    InternalShowAndActivate(true);
                    break;
                case MessageAction.ActivateMenu:
                    {
                        var dc = (AppViewModel)this.DataContext;
                        var menu = dc.AllMenus.FirstOrDefault(m => m.Name.Equals(msg.TargetMenu, StringComparison.CurrentCultureIgnoreCase));
                        if (menu != null)
                        {
                            dc.CurrentMenu = menu;
                        }
                        InternalShowAndActivate(menu == null);
                    } 
                    break;
            }

        }

        public void onConfigure(object o, RoutedEventArgs e) {
            this.Execute("Configure");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr handle = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);

            (this.DataContext as AppViewModel)!.RefreshCurrentDesktopState();
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeUtils.WM_SYSCOMMAND:
                    switch (wParam.ToInt32())
                    {
                        case NativeUtils.SC_MOVE: handled = true; return IntPtr.Zero;
                        case NativeUtils.SC_SIZE: handled = true; return IntPtr.Zero;
                        case NativeUtils.SC_MAXIMIZE: handled = true; return IntPtr.Zero;
                        case NativeUtils.SC_MINIMIZE: handled = true; return IntPtr.Zero;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public void InternalShowAndActivate(bool autoSwitchMenu)
        {
            if (this.Visibility != Visibility.Visible)
            {
                (this.DataContext as AppViewModel)!.RefreshCurrentDesktopState();
                if (autoSwitchMenu)
                {
                    (this.DataContext as AppViewModel)!.SelectMenuFromApp();
                } 

                this.Show();
                this.Activate();
                this.ApplyLayout();
            }
        }

        public void AppHotKeyPressed()
        {
            DebugLog.Log("Application hotkey pressed");
            if (this.Visibility != Visibility.Visible)
            {
                InternalShowAndActivate(true);
            }
            else
            {
                // Toggle lock
                ((AppViewModel)this.DataContext).LockMenu = !((AppViewModel)this.DataContext)!.LockMenu;
            }
        }

        public void onDeactivated(object o, EventArgs e) {
            if (!this.lockActive)
            {
                this.Hide();
            }
            this.MenuPopup.IsOpen = false;
        }

        public void onMenuClick(object o, RoutedEventArgs e)
        {
            this.Execute("Menu");
        }

        public void onQuitClick(object o, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuList.SelectedIndex == -1) return;
            var menu = MenuList.SelectedItem as ButtonMenuViewModel;
            ((AppViewModel)DataContext).CurrentMenu = menu;
            this.MenuPopup.IsOpen = false;
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter)
        {
            if (!(parameter is string)) return false;
            if (String.IsNullOrEmpty((string)parameter)) return false;
            switch ((string)parameter)
            {
                case "New": return true;
                case "Open": return true;
                case "Save": return true;
                case "Menu": return true;
                case "Configure": return true;
                case "Lock": return true;
                case "ZoomMore": return true;
                case "ZoomLess": return true;
                default:
                    return Regex.IsMatch((string)parameter, @"^(Control|Alt|ControlShift|AltShift|ControlAlt)[1-5]$");
            }
        }

        public void Execute(object? parameter)
        {
            if (!(parameter is string)) return;
            var cmd = (string)parameter;
            if (String.IsNullOrEmpty(cmd)) return;
            var dc = (AppViewModel)this.DataContext;
            switch (cmd)
            {
                case "New":
                    DataContext = ((AppViewModel)DataContext).NewFromThis();
                    settings.AppViewModel = ((AppViewModel)DataContext);
                    return;
                case "Open":
                    {
                        lockActive = true;
                        Topmost = false;
                        try
                        {
                            var ofd = new OpenFileDialog();
                            ofd.Filter = "config files (*.overlayconfig.json)|*.overlayconfig.json|All files|*.*";
                            ofd.InitialDirectory = settings.FileAccessPath;
                            ofd.CheckFileExists = true;
                            var result = ofd.ShowDialog(this);
                            if (!(result ?? false)) return;
                            settings.FileAccessPath = System.IO.Path.GetDirectoryName(ofd.FileName)!;
                            this.DataContext = ((AppViewModel)DataContext).LoadFromFile(ofd.FileName)
                                ?? this.DataContext;
                            settings.AppViewModel = ((AppViewModel)DataContext);
                        }
                        finally
                        {
                            lockActive = false;
                            Topmost = true;
                            Keyboard.Focus(this);
                        }
                        return;
                    }

                case "Save":
                    {
                        lockActive = true;
                        Topmost = false;
                        try
                        {
                            var sfd = new SaveFileDialog();
                            sfd.Filter = "config files (*.overlayconfig.json)|*.overlayconfig.json|All files|*.*";
                            sfd.InitialDirectory = settings.FileAccessPath;
                            sfd.OverwritePrompt = true;
                            sfd.CheckPathExists = true;
                            sfd.AddExtension = true;
                            sfd.DefaultExt = ".overlayconfig.json";
                            var result = sfd.ShowDialog(this);
                            if (!(result ?? false)) return;
                            settings.FileAccessPath = System.IO.Path.GetDirectoryName(sfd.FileName)!;
                            ((AppViewModel)this.DataContext).SaveToFile(sfd.FileName);
                        }
                        finally
                        {
                            lockActive = false;
                            Topmost = true;
                            Keyboard.Focus(this);
                        }
                        return;
                    }
                case "Lock":
                    // Toggle lock
                    ((AppViewModel)this.DataContext).LockMenu = !((AppViewModel)this.DataContext)!.LockMenu;
                    return;
                case "Menu":
                    MenuList.SelectedIndex = -1;
                    this.MenuPopup.IsOpen = !this.MenuPopup.IsOpen;
                    return;
                case "Configure":
                    lockActive = true;
                    this.Topmost = false;

                    var configure = new ConfigurationWindow();
                    configure.DataContext = new ConfigurationViewModel((this.DataContext as AppViewModel)!);
                    if (configure.ShowDialog() ?? false)
                    {
                        ((AppViewModel)this.DataContext).ApplyFrom((ConfigurationViewModel)configure.DataContext);
                        settings.Save();
                    }

                    this.Topmost = true;
                    lockActive = false;
                    Keyboard.Focus(this);
                    return;
                case "ZoomMore":
                    {
                        var zoom = settings.DisplayZoomLogValue;
                        zoom += 0.1;
                        if (zoom > 2.0) zoom = 2.0;
                        if (zoom != settings.DisplayZoomLogValue) settings.DisplayZoomLogValue = zoom;
                    }
                    return;
                case "ZoomLess":
                    {
                        var zoom = settings.DisplayZoomLogValue;
                        zoom -= 0.1;
                        if (zoom < -2.0) zoom = -2.0;
                        if (zoom != settings.DisplayZoomLogValue) settings.DisplayZoomLogValue = zoom;
                    }
                    return;
            }
            if (!Regex.IsMatch(cmd, @"^(Control|Alt|ControlShift|AltShift|ControlAlt)[1-5]$")) return;
            if (dc?.CurrentMenu == null) return;

            int columnIndex = int.Parse(cmd.AsSpan().Slice(cmd.Length - 1)) - 1;
            int rowIndex = cmd.Substring(0, cmd.Length - 1) switch
            {
                "Control" => 0,
                "Alt" => 1,
                "ControlShift" => 2,
                "AltShift" => 3,
                "ControlAlt" => 4,
                _ => throw new InvalidOperationException()
            };
            if (columnIndex == 2 && rowIndex == 2) return;
            if (!dc.CurrentMenu[rowIndex, columnIndex].CanExecute(null)) return;
            dc.CurrentMenu[rowIndex, columnIndex].Execute(null);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.PageDown:
                    e.Handled = true;
                    {
                        var dc = (DataContext as AppViewModel);
                        if (dc == null) return;
                        var currentIndex = -1;
                        var menuCount = dc.AllMenus.Count;
                        if (dc.CurrentMenu != null) currentIndex = dc.AllMenus.IndexOf(dc.CurrentMenu);
                        var newIndex = currentIndex + 1;
                        if (newIndex >= menuCount) newIndex = 0;
                        if (menuCount == 0) newIndex = -1;
                        dc.CurrentMenu = newIndex == -1 ? null : dc.AllMenus[newIndex];
                        if (dc.CurrentMenu != null)
                        {
                            // Trigger animation
                            MessageDisplayBox.Visibility = Visibility.Visible;
                            MessageDisplayBox.Opacity = 0;
                            MessageDisplayBox.Text = dc.CurrentMenu.Name;
                            MessageDisplayBoxStoryboard.Stop(MessageDisplayBox);
                            MessageDisplayBoxStoryboard.Seek(MessageDisplayBox, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
                            MessageDisplayBoxStoryboard.Begin(MessageDisplayBox, true);
                        }
                    }
                    break;
                case Key.PageUp:
                    e.Handled = true;
                    {
                        var dc = (DataContext as AppViewModel);
                        if (dc == null) return;
                        var currentIndex = -1;
                        var menuCount = dc.AllMenus.Count;
                        if (dc.CurrentMenu != null) currentIndex = dc.AllMenus.IndexOf(dc.CurrentMenu);
                        var newIndex = currentIndex - 1;
                        if (newIndex < 0) newIndex = menuCount - 1;
                        dc.CurrentMenu = newIndex == -1 ? null : dc.AllMenus[newIndex];
                        if (dc.CurrentMenu != null)
                        {
                            // Trigger animation
                            MessageDisplayBox.Visibility = Visibility.Visible;
                            MessageDisplayBox.Opacity = 0;
                            MessageDisplayBox.Text = dc.CurrentMenu.Name;
                            MessageDisplayBoxStoryboard.Stop(MessageDisplayBox);
                            MessageDisplayBoxStoryboard.Seek(MessageDisplayBox, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
                            MessageDisplayBoxStoryboard.Begin(MessageDisplayBox, true);
                        }
                    }
                    break;
                case Key.Escape:
                    e.Handled = true;
                    if (!lockActive) Hide();
                    break;
            }
        }

        private void MessageOverlay_StoryboardCompleted(object sender, EventArgs e)
        {
            MessageDisplayBox.Visibility = Visibility.Collapsed;
            MessageDisplayBox.Text = "";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown(0);
        }

        private void ApplyZoom()
        {
            var midX = this.Left + (this.Width / 2);
            var midY = this.Top + (this.Height / 2);

            var newWidth = settings.MainWindowWidth * settings.DisplayZoom;
            var newHeight = settings.MainWindowHeight * settings.DisplayZoom;

            var widthDelta = newWidth - DisplayViewBox.Width;
            var heightDelta = newHeight - DisplayViewBox.Height;

            DisplayViewBox.Width = newWidth;
            DisplayViewBox.Height = newHeight;
            this.Left -= widthDelta / 2;
            this.Top -= heightDelta / 2;
        }

        private void ApplyLayout()
        {
            var displays = NativeUtils.GetDisplayCoordinates();
            if (displays == null) return;

            var hmonCursor = NativeUtils.MonitorFromPoint(
                new Point(((AppViewModel)DataContext).MouseX, ((AppViewModel)DataContext).MouseY));

            NativeUtils.DisplayInfo targetMonitor;
            var primaryMonitor = displays.FirstOrDefault(x => x.isPrimary);
            if (settings.ShowOnCursorScreen)
            {
                targetMonitor = displays.FirstOrDefault(x => x.hMonitor == hmonCursor, primaryMonitor);
            }
            else
            {
                var monitorIndex = settings.ShowOnScreenNumber!.Value;
                targetMonitor = displays.Count >= monitorIndex ? displays[monitorIndex] : primaryMonitor;
            }

            double boxWidth, boxHeight;
            if (settings.SizeToScreenPercent == 0)
            {
                boxWidth = settings.MainWindowWidth * settings.DisplayZoom;
                boxHeight = settings.MainWindowHeight * settings.DisplayZoom;
            }
            else
            {
                var rect = targetMonitor.clientRect;
                boxWidth = rect.Width * settings.SizeToScreenPercent / 100.0;
                boxHeight = rect.Height * settings.SizeToScreenPercent / 100.0;
            }
            DisplayViewBox.Width = boxWidth;
            DisplayViewBox.Height = boxHeight;

            this.Left = targetMonitor.clientRect.Left + (targetMonitor.clientRect.Width / 2) - (boxWidth / 2);
            this.Top = targetMonitor.clientRect.Top + (targetMonitor.clientRect.Height / 2) - (boxHeight / 2);
        }

        private void MenuPopup_Closed(object sender, EventArgs e)
        {
            MenuButton.IsEnabled = true;
        }

        private void MenuPopup_Opened(object sender, EventArgs e)
        {
            MenuButton.IsEnabled = false;
        }

        private void onDebugLogClick(object sender, RoutedEventArgs e)
        {
            lockActive = true;
            MenuPopup.IsOpen = false;
            ((App)App.Current).DebugWindow.Show();
            lockActive = false;
        }
    }
}
