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
    /// Interaction logic for SendMouseConfigControl.xaml
    /// </summary>
    public partial class SendMouseConfigControl : UserControl
    {
        public SendMouseConfigControl()
        {
            InitializeComponent();
        }

        private void ActionAdd_Click(object sender, RoutedEventArgs e)
        {
            ((SendMouse)this.DataContext).MouseActions.Add(new SendMouseAction());
            e.Handled = true;
        }

        private void ActionRemove_Click(object sender, RoutedEventArgs e)
        {
            if (MouseActionsList.SelectedIndex != -1)
            {
                ((SendMouse)this.DataContext).MouseActions.RemoveAt(MouseActionsList.SelectedIndex);
            }
            e.Handled = true;
        }

        private void MouseActionsMoveUp_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (MouseActionsList.SelectedIndex <= 0) return;
            var dc = DataContext as SendMouse;
            if (dc == null) return;

            dc!.MouseActions.Move(MouseActionsList.SelectedIndex, MouseActionsList.SelectedIndex - 1);
        }

        private void MouseActionsMoveDown_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (MouseActionsList.SelectedIndex == -1) return;
            var dc = DataContext as SendMouse;
            if (dc == null) return;

            if (MouseActionsList.SelectedIndex == dc!.MouseActions.Count - 1) return;
            dc!.MouseActions.Move(MouseActionsList.SelectedIndex, MouseActionsList.SelectedIndex + 1);
        }
    }
}
