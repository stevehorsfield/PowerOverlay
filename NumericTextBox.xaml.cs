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

namespace PowerOverlay
{
    /// <summary>
    /// Interaction logic for NumericTextBox.xaml
    /// </summary>
    public partial class NumericTextBox : UserControl
    {
        // Min/Max/Current properties: https://docs.microsoft.com/en-us/dotnet/desktop/wpf/properties/dependency-property-callbacks-and-validation?view=netdesktop-6.0
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public event RoutedEventHandler? ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        public static readonly DependencyProperty ValueProperty;
        public static readonly DependencyProperty MaxValueProperty;
        public static readonly DependencyProperty MinValueProperty;
        public static readonly RoutedEvent ValueChangedEvent;

        static NumericTextBox()
        {
            ValueChangedEvent = EventManager.RegisterRoutedEvent(
                nameof(ValueChanged), 
                RoutingStrategy.Bubble, 
                typeof(RoutedEventHandler), 
                typeof(NumericTextBox));

            MaxValueProperty =
                DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(NumericTextBox),
                    new FrameworkPropertyMetadata(Int32.MaxValue, FrameworkPropertyMetadataOptions.None,
                    new PropertyChangedCallback(OnMaxValueChanged),
                    new CoerceValueCallback(CoerceMaxValue),
                    true));
            MinValueProperty =
                DependencyProperty.Register(nameof(MinValue), typeof(int), typeof(NumericTextBox),
                    new FrameworkPropertyMetadata(Int32.MinValue, FrameworkPropertyMetadataOptions.None,
                    new PropertyChangedCallback(OnMinValueChanged),
                    new CoerceValueCallback(CoerceMinValue),
                    true));

            ValueProperty =
                DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericTextBox),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(OnValueChanged),
                new CoerceValueCallback(CoerceValue),
                true));
        }

        private static object CoerceMaxValue(DependencyObject d, object baseValue)
        {
            var ntb = (NumericTextBox)d;
            var value = (int)baseValue;
            return value < ntb.MinValue ? ntb.MinValue : value;
        }
        private static object CoerceMinValue(DependencyObject d, object baseValue)
        {
            var ntb = (NumericTextBox)d;
            var value = (int)baseValue;
            return value > ntb.MaxValue ? ntb.MaxValue : value;
        }
        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var ntb = (NumericTextBox)d;
            var value = (int)baseValue;
            value = value > ntb.MaxValue ? ntb.MaxValue : value;
            value = value < ntb.MinValue ? ntb.MinValue : value;
            return value;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(MinValueProperty);
            d.CoerceValue(MaxValueProperty);

            var ntb = (NumericTextBox)d;
            var valueString = ntb.Value.ToString();
            if (! String.Equals(valueString, ntb.DataEntry.Text, StringComparison.Ordinal))
            {
                ntb.DataEntry.Text = valueString;
            }
            
            ((NumericTextBox)d).RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(MinValueProperty);
            d.CoerceValue(ValueProperty);
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(MaxValueProperty);
            d.CoerceValue(ValueProperty);
        }

        public NumericTextBox()
        {
            InitializeComponent();
            DataEntry.Text = Value.ToString();
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            Value++;
            e.Handled = true;
        }
        private void Down_Click(object sender, RoutedEventArgs e)
        {
            Value--;
            e.Handled = true;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (! String.Equals(DataEntry.Text, Value.ToString(), StringComparison.Ordinal))
            {
                int value;
                if (Int32.TryParse(DataEntry.Text, out value))
                {
                    if (Value != value) Value = value;
                }

                if (!String.Equals(DataEntry.Text, Value.ToString(), StringComparison.Ordinal))
                {
                    DataEntry.Text = Value.ToString();
                }
            }
        }
    }
}
