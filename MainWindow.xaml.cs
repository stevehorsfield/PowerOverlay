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

namespace overlay_popup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const int HOTKEY_ID = 15000;
        private bool lockActive = false;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new AppViewModel();

            
            for (int i = 0; i < 5; ++i) {
                for (int j = 0; j < 5; ++j) {
                    if (i == 2 && j == 2) continue; // skip middle location

                    var ctrl = new CommandButton();

                    Grid.SetRow(ctrl, i);
                    Grid.SetColumn(ctrl, j);

                    var binding = new Binding($".[{i},{j}]");
                    ctrl.SetBinding(FrameworkElement.DataContextProperty, binding);
                    binding = new Binding($".[{i},{j}]");
                    ctrl.SetBinding(Button.CommandProperty, binding);

                    ButtonGrid.Children.Add(ctrl);
                }
            }
        }

        public void onConfigure(object o, RoutedEventArgs e) {
            lockActive = true;
            this.Topmost = false;

            var configure = new ConfigurationWindow();
            configure.DataContext = new ConfigurationViewModel((this.DataContext as AppViewModel)!);
            if (configure.ShowDialog() ?? false)
            {
                ((AppViewModel)this.DataContext).ApplyFrom((ConfigurationViewModel)configure.DataContext);
            }

            this.Topmost = true;
            lockActive = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr handle = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);

            NativeUtils.RegisterHotKey(this, HOTKEY_ID, Key.F2, ModifierKeys.Windows);

            (this.DataContext as AppViewModel)!.RefreshCurrentApp();
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeUtils.WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            if (this.Visibility != Visibility.Visible)
                            {
                                (this.DataContext as AppViewModel)!.RefreshCurrentApp();
                                this.Show();
                                this.Activate();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public void onDeactivated(object o, EventArgs e) {
            if (!this.lockActive)
            {
                this.Hide();
            }
            this.MenuPopup.Visibility = Visibility.Hidden;
        }

        public void onMenuClick(object o, RoutedEventArgs e)
        {
            MenuList.SelectedIndex = -1;
            this.MenuPopup.Visibility =
                this.MenuPopup.Visibility == Visibility.Hidden ?
                Visibility.Visible : Visibility.Hidden;
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
            this.MenuPopup.Visibility = Visibility.Hidden;
        }
    }
}
