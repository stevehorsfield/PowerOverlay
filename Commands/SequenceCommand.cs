using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace PowerOverlay.Commands;

public class SequenceCommand : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return SequenceCommandDefinition.Instance; } }

    private readonly FrameworkElement configElement = SequenceCommandDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    private readonly ObservableCollection<ActionCommand> actions = new();

    public ObservableCollection<ActionCommand> Actions => actions;

    public override bool CanExecute(object? parameter)
    {
        return actions.All(a => a.CanExecute(null));
    }

    public override ActionCommand Clone()
    {
        var clone = new SequenceCommand();
        foreach (var action in Actions) clone.Actions.Add(action.Clone());
        return clone;
    }

    public override void Execute(object? parameter)
    {
        foreach (var a in Actions) a.Execute(null);
    }

    public override void WriteJson(JsonObject o)
    {
        o.AddLowerCamel(nameof(Actions),
            new JsonArray(Actions.Select(x => CommandFactory.ToJson(x)).ToArray()));
    }
    public static SequenceCommand CreateFromJson(JsonObject o)
    {
        var result = new SequenceCommand();
        o.TryGet<JsonArray>(nameof(Actions), xs => {
            foreach (var x in xs)
            {
                if ((!(x is JsonObject)) || (x == null)) 
                    throw new InvalidOperationException("Sequence command action array contains invalid data");
                var action = CommandFactory.FromJson(x.AsObject());
                if (action != null) result.Actions.Add(action);
            }
        });
        return result;
    }
}

public class SequenceCommandDefinition : ActionCommandDefinition
{
    public static SequenceCommandDefinition Instance = new();

    public override string ActionName => "SequenceCommand";
    public override string ActionDisplayName => "Execute a sequence of commands";

    public override ActionCommand Create()
    {
        return new SequenceCommand();
    }
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return SequenceCommand.CreateFromJson(o);
    }


    public override FrameworkElement CreateConfigElement()
    {
        return new SequenceCommandControl();
    }
}

public class SequenceCommandControl : ContentControl
{
    public SequenceCommandControl()
    {
        InitializeComponent();
    }

    private bool _contentLoaded;

    public void InitializeComponent()
    {
        if (_contentLoaded) return;
        _contentLoaded = true;

        var ctrl = new DockPanel();

        var wp = new WrapPanel()
        {
            Orientation = Orientation.Horizontal,

        };
        DockPanel.SetDock(wp, Dock.Top);
        ctrl.Children.Add(wp);
        foreach (var x in CommandFactory.GetCommandTypes())
        {
            var b = new Button()
            {
                Content = $"Add {x.ActionDisplayName}",
                Margin = new Thickness(2),
            };
            b.Click += (_, _) => ((SequenceCommand)this.DataContext).Actions.Add(x.Create());
            wp.Children.Add(b);
        }

        var lb = new ListBox();
        ctrl.Children.Add(lb);
        lb.SetBinding(ListBox.ItemsSourceProperty, new Binding("Actions"));
        lb.IsSynchronizedWithCurrentItem = true;
        lb.HorizontalContentAlignment = HorizontalAlignment.Stretch;

        RoutedEventHandler h = (object o, RoutedEventArgs e) =>
        {
            if (!(e.OriginalSource is Button)) return;
            var b = (Button)e.OriginalSource;
            var current = (DependencyObject)e.OriginalSource;
            ListBoxItem? item = null;
            while (current != null && !(current is ListBox))
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is ListBoxItem)
                {
                    item = ((ListBoxItem)current);
                }
            }
            if (item == null) return;
            var index = lb.Items.IndexOf(item.DataContext);
            var ds = (SequenceCommand)this.DataContext;

            switch (b.Name)
            {
                case "MoveActionUp":
                    if (index <= 0) break;
                    ds.Actions.Move(index, index - 1);
                    break;
                case "MoveActionDown":
                    if (index >= (lb.Items.Count - 1)) break;
                    ds.Actions.Move(index, index + 1);
                    break;
                case "RemoveAction":
                    if (index == -1) break;
                    ds.Actions.Remove((ActionCommand) item.DataContext!);
                    break;
                default:
                    return;
            }
            e.Handled = true;
        };

        lb.AddHandler(Button.ClickEvent, h);

        var xaml = Encoding.UTF8.GetBytes(ActionsDataTemplateXaml);
        using var ms = new MemoryStream(xaml);
        var dataTemplate = (DataTemplate)XamlReader.Load(ms);
        lb.ItemTemplate = dataTemplate;

        this.Content = ctrl;

    }

    private const string ActionsDataTemplateXaml = @"<DataTemplate 
        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
        >
    <StackPanel>
        <ContentPresenter Content=""{Binding Path=ConfigElement}"" />
        <WrapPanel Orientation=""Horizontal"">
            <Button Name=""MoveActionUp"" Content=""Move up"" Margin=""2"" />
            <Button Name=""MoveActionDown"" Content=""Move down"" Margin=""2"" />
            <Button Name=""RemoveAction"" Content=""Delete"" Margin=""2"" />
        </WrapPanel>
    </StackPanel>
    
</DataTemplate>";

}