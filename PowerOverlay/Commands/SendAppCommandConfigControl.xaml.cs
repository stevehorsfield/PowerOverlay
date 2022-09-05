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
    /// Interaction logic for SendAppCommandConfigControl.xaml
    /// </summary>
    public partial class SendAppCommandConfigControl : UserControl
    {
        public SendAppCommandConfigControl()
        {
            InitializeComponent();
        }

        private void AppSelector_Click(object sender, RoutedEventArgs e)
        {
            Button? b = e.OriginalSource as Button;
            if (b == null) return;
            switch (b.Name)
            {
                case "TargetAdd":
                    e.Handled = true;
                    ((SendAppCommand)b.DataContext).ApplicationTargets.Add(new ApplicationMatcherViewModel());
                    selector.SelectedIndex = selector.Items.Count - 1;
                    return;
                case "TargetRemove":
                    e.Handled = true;
                    if (selector.SelectedIndex == -1) return;
                    selector.SelectedIndex = selector.SelectedIndex - 1;
                    ((SendAppCommand)b.DataContext).ApplicationTargets.RemoveAt(selector.SelectedIndex + 1);
                    return;
            }

        }
    }
}
