using PowerOverlay.IPC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
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

        private NamedPipeServer? Pipe { get; set; }

        static App()
        {
            Commands.CommandFactory.Init();
        }

        private void Launch()
        {
            Pipe = new IPC.NamedPipeServer();

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
            var screenMenu = new System.Windows.Forms.ToolStripMenuItem("Show on screen", null,
                new System.Windows.Forms.ToolStripMenuItem("Follow cursor", null, (o,e) => ((MainWindow)MainWindow).Settings.ShowOnCursorScreen = true),
                new System.Windows.Forms.ToolStripMenuItem("Screen 0", null, (o, e) => ((MainWindow)MainWindow).Settings.ShowOnScreen0 = true),
                new System.Windows.Forms.ToolStripMenuItem("Screen 1", null, (o, e) => ((MainWindow)MainWindow).Settings.ShowOnScreen1 = true),
                new System.Windows.Forms.ToolStripMenuItem("Screen 2", null, (o, e) => ((MainWindow)MainWindow).Settings.ShowOnScreen2 = true),
                new System.Windows.Forms.ToolStripMenuItem("Screen 3", null, (o, e) => ((MainWindow)MainWindow).Settings.ShowOnScreen3 = true),
                new System.Windows.Forms.ToolStripMenuItem("Screen 4", null, (o, e) => ((MainWindow)MainWindow).Settings.ShowOnScreen4 = true)
                );
            TrayContextMenu.Items.Add(screenMenu);
            TrayContextMenu.Items.Add("-");
            
            TrayContextMenu.Items.Add("Quit", null, (s, e) => App.Current.Shutdown(0));

            TrayContextMenu.Opening += (o, e) =>
            {
                foreach (var x in screenMenu.DropDownItems)
                {
                    (x as System.Windows.Forms.ToolStripMenuItem)!.Checked = false;
                }
                var screenIndex = ((MainWindow)MainWindow).Settings.ShowOnScreenNumber;
                if (screenIndex.HasValue) screenIndex = screenIndex.Value + 1;
                else screenIndex = 0;
                ((System.Windows.Forms.ToolStripMenuItem)(screenMenu.DropDownItems[screenIndex.Value])).Checked = true;

                var displays = NativeUtils.GetDisplayCoordinates();
                if (displays != null)
                {
                    for (var x = 1; x <= 5; ++x)
                    {
                        var item = ((System.Windows.Forms.ToolStripMenuItem)(screenMenu.DropDownItems[x]));
                        item.Enabled = displays.Count >= x;
                        if (displays.Count >= x)
                        {
                            item.Text = $"Screen {x - 1} ({displays[x - 1].monitorRect.Width},{displays[x - 1].monitorRect.Height})";
                        }
                        else item.Text = $"Screen {x - 1} - not available";
                    }
                }
            };
        }

        public void HandleIPC(Message msg)
        {
            (Application.Current.MainWindow as MainWindow)?.HandleIPC(msg);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (NamedPipeClient.IsServerAvailable())
            {
                NativeUtils.AllowSetForegroundWindow(NativeUtils.ASFW_ANY);
                if (e.Args.Length > 0)
                {
                    NamedPipeClient.SendMessage(new Message() { Action = MessageAction.ActivateMenu, TargetMenu = e.Args[0] });
                } else
                {
                    NamedPipeClient.SendMessage(new Message() { Action = MessageAction.Activate });
                }
                Thread.Sleep(200); // necessary to retain permission to change foreground app
                Shutdown(0);
                return;
            }

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
            if (Pipe != null) Pipe.Dispose();
            Pipe = null;
        }
    }
}
