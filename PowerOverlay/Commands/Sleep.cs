﻿using System;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PowerOverlay.Commands;

public class Sleep : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return SleepDefinition.Instance; } }

    private readonly Lazy<FrameworkElement> configElement = new(SleepDefinition.Instance.CreateConfigElement);
    public override FrameworkElement ConfigElement => configElement.Value;

    private int sleepMilliseconds;
    public int SleepMilliseconds
    {
        get { return sleepMilliseconds; }
        set { 
            sleepMilliseconds = value; 
            RaisePropertyChanged(nameof(SleepMilliseconds)); 
            RaiseCanExecuteChanged(new EventArgs());
        }
    }

    public override bool CanExecute(object? parameter)
    {
        return SleepMilliseconds > 0;
    }

    public override ActionCommand Clone()
    {
        return new Sleep() { SleepMilliseconds = SleepMilliseconds };
    }

    public override Task ExecuteWithContext(CommandExecutionContext context)
    {
        return Sleeper.Sleep(SleepMilliseconds);
    }

    public override void WriteJson(JsonObject o)
    {
        o.AddLowerCamel(nameof(SleepMilliseconds), JsonValue.Create(SleepMilliseconds));
    }
    public static Sleep CreateFromJson(JsonObject o)
    {
        var result = new Sleep();
        o.TryGetValue<int>(nameof(SleepMilliseconds), i => result.SleepMilliseconds = i);
        return result;
    }
}

public class SleepDefinition : ActionCommandDefinition
{
    public static SleepDefinition Instance = new();

    public override string ActionName => "Sleep";
    public override string ActionDisplayName => "Delay";

    public override string ActionShortName => "Delay";

    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/Sleep.png"));
    public override ActionCommand Create()
    {
        return new Sleep();
    }
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return Sleep.CreateFromJson(o);
    }

    public override FrameworkElement CreateConfigElement()
    {
        var ctrl = new Grid();
        ctrl.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        ctrl.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        ctrl.ColumnDefinitions.Add(new ColumnDefinition() { });

        var addToGrid = (int r, int c, FrameworkElement item) =>
            { Grid.SetRow(item, r); Grid.SetColumn(item, c); ctrl.Children.Add(item); };

        var addNumericTextBox = (int r, string label, string propName) =>
        {
            var lbl = new TextBlock { Text = label };
            var ntb = new NumericTextBox();
            ntb.SetBinding(NumericTextBox.ValueProperty, propName);

            ntb.MinValue = 0;
            ntb.MaxValue = 30000;
            ntb.Width = 100;
            ntb.HorizontalAlignment = HorizontalAlignment.Left;

            addToGrid(r, 0, lbl);
            addToGrid(r, 2, ntb);
        };

        addNumericTextBox(1, "Delay (ms)", nameof(Sleep.SleepMilliseconds));

        return ctrl;
    }
}
