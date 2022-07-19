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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PowerOverlay
{
    /// <summary>
    /// Interaction logic for HotKeyWindow.xaml
    /// </summary>
    public partial class HotKeyWindow : Window
    {
        private const int HOTKEY_ID = 15000;
        public HotKeyWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr handle = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);

            if (!NativeUtils.RegisterHotKey(this, HOTKEY_ID, Key.F2, ModifierKeys.Windows))
            {
                MessageBox.Show("Unable to assign hot key, exiting.", "Error launching", MessageBoxButton.OK, MessageBoxImage.Stop);
                App.Current.Shutdown(1);
            };

            App.Current.MainWindow = new MainWindow();
            Visibility = Visibility.Collapsed;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeUtils.WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            (Application.Current.MainWindow as MainWindow)?.AppHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown(0);
        }
    }
}
