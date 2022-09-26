using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PowerOverlay.Commands;

public class SequenceCommand : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return SequenceCommandDefinition.Instance; } }

    private readonly Lazy<FrameworkElement> configElement = new(SequenceCommandDefinition.Instance.CreateConfigElement);
    public override FrameworkElement ConfigElement => configElement.Value;

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

    public override async Task ExecuteWithContext(CommandExecutionContext context)
    {
        foreach (var a in Actions)
        {
            await a.AsTask(context);
        }
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
    public override string ActionShortName => "Sequence";

    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/Sequence.png"));

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
        return new SequenceCommandConfigControl();
    }
}
