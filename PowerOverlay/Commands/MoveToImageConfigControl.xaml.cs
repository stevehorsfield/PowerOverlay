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

namespace PowerOverlay.Commands
{
    /// <summary>
    /// Interaction logic for MoveToImageConfigControl.xaml
    /// </summary>
    public partial class MoveToImageConfigControl : UserControl
    {
        public MoveToImageConfigControl()
        {
            InitializeComponent();
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (Clipboard.ContainsImage())
            {
                try
                {
                    ((MoveToImage)this.DataContext)!.ImageSource = Clipboard.GetImage();
                }
                catch (Exception ex)
                {
                    DebugLog.Log($"Unable to retrieve image from clipboard: {ex.Message}");
                }
            }
        }
    }
}
