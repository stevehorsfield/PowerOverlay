using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PowerOverlay.Commands;

/// <summary>
/// Interaction logic for CloseCommandConfigControl.xaml
/// </summary>
public partial class CloseCommandConfigControl : UserControl
{
    public CloseCommandConfigControl()
    {
        InitializeComponent();
    }

    private void selector_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (selector.DataContext == null) return;
        if (selector.SelectedIndex != -1) return;
        if (((CloseCommand)selector.DataContext).ApplicationTargets.Count > 0)
        {
            selector.SelectedIndex = 0;
        }
    }

    private void selector_Click(object sender, RoutedEventArgs e)
    {
        Button? b = e.OriginalSource as Button;
        if (b == null) return;
        switch (b.Name)
        {
            case "TargetAdd":
                e.Handled = true;
                ((CloseCommand)b.DataContext).ApplicationTargets.Add(new ApplicationMatcherViewModel());
                selector.SelectedIndex = selector.Items.Count - 1;
                return;
            case "TargetRemove":
                e.Handled = true;
                if (selector.SelectedIndex == -1) return;
                selector.SelectedIndex = selector.SelectedIndex - 1;
                ((CloseCommand)b.DataContext).ApplicationTargets.RemoveAt(selector.SelectedIndex + 1);
                return;
        }
    }
}

