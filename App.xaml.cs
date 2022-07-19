using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PowerOverlay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public System.Windows.Forms.NotifyIcon? TrayIcon { get; private set; }
        private System.Windows.Forms.ContextMenuStrip? TrayContextMenu { get; set; }
        private System.Drawing.Icon? AppIcon { get; set; }

        static App()
        {
            Commands.CommandFactory.Init();
        }

        private void Launch()
        {

            AppIcon = new System.Drawing.Icon(typeof(App), "AppIcon.ico");
            TrayContextMenu = new System.Windows.Forms.ContextMenuStrip();
            TrayIcon = new System.Windows.Forms.NotifyIcon()
            {
                Text = "Power Overlay",
                Visible = true,
                ContextMenuStrip = TrayContextMenu,
                Icon = AppIcon,
            };
            var displayMainWindow = () =>
            {
                (App.Current?.MainWindow as MainWindow)?.InternalShowAndActivate(true);
            };
            TrayIcon.DoubleClick += (s, e) => displayMainWindow();
            TrayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    displayMainWindow();
                }
            };
            TrayContextMenu.Items.Add("Show [Win+F2]", null, (s, e) => displayMainWindow());
            TrayContextMenu.Items.Add("-");
            TrayContextMenu.Items.Add("Quit", null, (s, e) => App.Current.Shutdown(0));
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            Launch();
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (TrayIcon != null) TrayIcon.Dispose();
            TrayIcon = null;
            if (AppIcon != null) AppIcon.Dispose();
            AppIcon = null;
            if (TrayContextMenu != null) TrayContextMenu.Dispose();
            TrayContextMenu = null;
        }
    }
}
