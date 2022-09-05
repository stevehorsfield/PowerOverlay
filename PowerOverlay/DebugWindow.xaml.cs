using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Shapes;

namespace PowerOverlay
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public bool IsTrackingEnabled { get; set; }

        public DebugWindow()
        {
            IsTrackingEnabled = true;

            InitializeComponent();

            this.DataContext = ((App)App.Current).DebugLog;

            var entries = ((App)App.Current).DebugLog.LogEntries;

            (entries as INotifyCollectionChanged).CollectionChanged += LogEntries_CollectionChanged;            
        }

        private void LogEntries_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!IsTrackingEnabled) return;
            // Invoke in background because ListBox cannot be manipulated while handling the NotifyCollectionChanged event
            Dispatcher.BeginInvoke(
                () => {
                    DebugLogListBox.SelectedIndex = DebugLogListBox.Items.Count - 1;
                    var item = DebugLogListBox.Items.GetItemAt(DebugLogListBox.SelectedIndex);
                    DebugLogListBox.ScrollIntoView(item);
                }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            ((App)App.Current).DebugLog.Clear();
        }

        private void CopyLog_Click(object sender, RoutedEventArgs e)
        {
            ((App)App.Current).DebugLog.CopyToClipboard();
        }
    }
}
