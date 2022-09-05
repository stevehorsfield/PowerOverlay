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
    /// Interaction logic for SequenceCommandConfigControl.xaml
    /// </summary>
    public partial class SequenceCommandConfigControl : UserControl
    {
        public IEnumerable<ActionCommandDefinition> ActionTypes => CommandFactory.GetCommandTypes();

        public SequenceCommandConfigControl()
        {
            InitializeComponent();


        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var actionDefinition = (ActionCommandDefinition)(((Button)sender).DataContext);
            ((SequenceCommand)this.DataContext).Actions.Add(actionDefinition.Create());
        }

        private void MoveActionUp_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var index = ItemsBox.Items.IndexOf(((Button)sender).DataContext);
            var ds = (SequenceCommand)this.DataContext;
            if (index <= 0) return;
            ds.Actions.Move(index, index - 1);
        }

        private void MoveActionDown_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var index = ItemsBox.Items.IndexOf(((Button)sender).DataContext);
            var ds = (SequenceCommand)this.DataContext;
            if (index >= (ItemsBox.Items.Count - 1)) return;
            ds.Actions.Move(index, index + 1);
        }
        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var index = ItemsBox.Items.IndexOf(((Button)sender).DataContext);
            var ds = (SequenceCommand)this.DataContext;
            if (index == -1) return;
            ds.Actions.Remove((ActionCommand)(((Button)sender).DataContext));
        }
    }
}
