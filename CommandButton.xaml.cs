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
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace overlay_popup;

public partial class CommandButton : UserControl {

    //public static readonly RoutedEvent ClickEvent = 
    //    EventManager.RegisterRoutedEvent("Click",
    //        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommandButton));

    //public event RoutedEventHandler? Click {
    //    add {
    //        AddHandler(ClickEvent, value);
    //    }
    //    remove {
    //        RemoveHandler(ClickEvent, value);
    //    }
    //}

    public CommandButton() {
        InitializeComponent();
        this.DataContext = this;
    }

    //public void bubbleClick(object o, RoutedEventArgs e) {
    //    RaiseEvent(new RoutedEventArgs(ClickEvent));
    //    // if (this.Click != null) {
    //    //     this.Click(this, e);
    //    // }
    //}

    public ICommand Command
    {
        get => (ICommand)GetValue(Button.CommandProperty);
        set => SetValue(Button.CommandProperty, value);
    }
}