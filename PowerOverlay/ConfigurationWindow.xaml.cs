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

namespace PowerOverlay;

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

    private void MenusAdd_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ((ConfigurationViewModel)this.DataContext).Menus.Add(new ConfigurationButtonMenuViewModel(new ButtonMenuViewModel() { Name = "untitled" }));
    }

    private void MenusRemove_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (MenuList.SelectedItem == null) return;
        var selected = (ConfigurationButtonMenuViewModel)MenuList.SelectedItem;
        if (!selected.CanChangeName) return;
        var result = MessageBox.Show(this, $"Confirm delete menu '{selected.Name}'?'", "Confirm menu deletion", MessageBoxButton.YesNo);
        if (result == MessageBoxResult.Yes)
        {
            MenuList.SelectedIndex =
                MenuList.SelectedIndex == 0 ? 0 :
                MenuList.SelectedIndex - 1; // Move away from deleted item
            ((ConfigurationViewModel)this.DataContext).Menus.Remove(selected);
        }
    }

    private void SelectorsAdd_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ((ConfigurationButtonMenuViewModel)SelectorsList.DataContext)
            .MenuSelectors.Add(new ApplicationMatcherViewModel());
    }

    private void SelectorsRemove_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (SelectorsList.SelectedItem == null) return;
        var selected = (ApplicationMatcherViewModel)SelectorsList.SelectedItem;
        SelectorsList.SelectedIndex = 
            SelectorsList.SelectedIndex == 0 ? 0 :
            SelectorsList.SelectedIndex - 1; // Move away from deleted item
        ((ConfigurationButtonMenuViewModel)SelectorsList.DataContext).MenuSelectors.Remove(selected);
    }

    //private void ActionModeList_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    //{
    //    if (ActionModeList.DataContext == null)
    //    {
    //        ActionModeList.SelectedIndex = -1;
    //        return;
    //    }

    //    var actionMode = ((ButtonViewModel)ActionModeList.DataContext).ActionMode;
    //    var actionModeName = actionMode.ToString();
    //    foreach (var item in ActionModeList.Items.Cast<ListBoxItem>())
    //    {
    //        if (((string)item.Tag).Equals(actionModeName,StringComparison.InvariantCultureIgnoreCase))
    //        {
    //            item.IsSelected = true;
    //            return;
    //        }
    //    }
    //    ActionModeList.SelectedIndex = -1;
    //}
    //private void ActionModeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    e.Handled = true;
    //    if (ActionModeList.SelectedIndex == -1) return;
    //    if (ActionModeList.DataContext == null) return;

    //    var dc = (ButtonViewModel)ActionModeList.DataContext;
    //    var mode = Enum.Parse<ActionMode>((string)((ListBoxItem)ActionModeList.SelectedItem).Tag);
    //    dc.SetActionMode(mode);
    //}
}