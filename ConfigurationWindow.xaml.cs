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
using System.Windows.Shapes;

namespace overlay_popup;

/// <summary>
/// Interaction logic for ConfigurationWindow.xaml
/// </summary>
public partial class ConfigurationWindow : Window
{

    public ConfigurationWindow()
    {
        InitializeComponent();

        for (int i = 0; i < 5; ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                var btn = new Button();
                btn.Margin = new Thickness(5);
                if (i == 2 && j == 2) btn.Visibility = Visibility.Hidden;
                ButtonGrid.Children.Add(btn);

                btn.Tag = i * 5 + j;
                var binding = new Binding($".[{(int)btn.Tag}].DefaultStyle.BackgroundColourBrush");
                btn.SetBinding(Control.BackgroundProperty, binding);

                btn.BorderBrush = new SolidColorBrush(Colors.OrangeRed);
                btn.BorderThickness = new Thickness(0);

                if (i == 0 && j == 0) { btn.BorderThickness = new Thickness(3); }

                btn.Click += (s, e) => {
                    var index = (int)((Button)s).Tag;
                    var collectionView = CollectionViewSource.GetDefaultView(ButtonGrid.DataContext);
                    var currentIndex = collectionView.CurrentPosition;
                    ((Button)ButtonGrid.Children[currentIndex]).BorderThickness = new Thickness(0);
                    ((Button)ButtonGrid.Children[index]).BorderThickness = new Thickness(3);
                    collectionView.MoveCurrentToPosition(index);
                };
            }
        }
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        for (int i = 0; i < 5; ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                var b = (Button)ButtonGrid.Children[i * 5 + j];
                b.BorderThickness = new Thickness((i == 0 && j == 0) ? 3 : 0);
            }
        }
        if (ButtonGrid.DataContext != null)
        {
            CollectionViewSource.GetDefaultView(ButtonGrid.DataContext).MoveCurrentToPosition(0);
        }
    }

    private void ContentFormatRadio_Click(object sender, RoutedEventArgs e)
    {
        var contentSourceType = Enum.Parse<ButtonViewModel.ContentSourceType>((string) ((RadioButton)sender).Tag);
        (((RadioButton)sender).DataContext as ButtonViewModel)!.ContentFormat = contentSourceType;
    }

    private void CopyButtonStyle_Click(object sender, RoutedEventArgs e)
    {
        ((ConfigurationViewModel)DataContext).ClipboardButton = (ButtonViewModel)CollectionViewSource.GetDefaultView(ButtonGrid.DataContext).CurrentItem;
    }

    private void PasteButtonStyle_Click(object sender, RoutedEventArgs e)
    {
        var model = (ButtonViewModel)CollectionViewSource.GetDefaultView(ButtonGrid.DataContext).CurrentItem;
        var source = ((ConfigurationViewModel)DataContext).ClipboardButton;
        if (source == null) return;

        model.PasteStyleFrom(source);
    }

    private void ActionModeNoOp_Click(object sender, RoutedEventArgs e) {
        var model = (ButtonViewModel)CollectionViewSource.GetDefaultView(ButtonGrid.DataContext).CurrentItem;
        model.SetActionMode(ButtonViewModel.ActionMode.NoAction);
    }
    private void ActionModeMenu_Click(object sender, RoutedEventArgs e) {
        var model = (ButtonViewModel)CollectionViewSource.GetDefaultView(ButtonGrid.DataContext).CurrentItem;
        model.SetActionMode(ButtonViewModel.ActionMode.SelectMenu);
    }
    private void ActionModeActivity_Click(object sender, RoutedEventArgs e) {
        var model = (ButtonViewModel)CollectionViewSource.GetDefaultView(ButtonGrid.DataContext).CurrentItem;
        model.SetActionMode(ButtonViewModel.ActionMode.PerformTask);
    }

    private void TargetMenuList_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var button = (ButtonViewModel)TargetMenuList.DataContext;
        if (button == null) return;
        var source = TargetMenuList.GetBindingExpression(ListBox.ItemsSourceProperty).ResolvedSource as ListCollectionView;
        if (source == null) return;
        foreach (var x in source)
        {
            if (((ConfigurationButtonMenuViewModel)x).Name.Equals(button.TargetMenu,StringComparison.InvariantCultureIgnoreCase))
            {
                source.MoveCurrentTo(x);
                return;
            }
        }
        source.MoveCurrentToPosition(-1);
    }

    private void TargetMenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var button = (ButtonViewModel)TargetMenuList.DataContext;
        if (button == null) return;
        button.TargetMenu = 
            (TargetMenuList.SelectedItem as ConfigurationButtonMenuViewModel)?.Name ?? String.Empty;
    }
}