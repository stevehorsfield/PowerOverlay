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
    /// Interaction logic for AudioControlConfigControl.xaml
    /// </summary>
    public partial class AudioControlConfigControl : UserControl
    {
        public AudioControlConfigControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dc = DataContext as AudioControl;
            if (dc == null) return;
            dc.PopulateDiscoveredDevices();
        }
    }
}
