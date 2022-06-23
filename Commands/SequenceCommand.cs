using System;
using System.Windows;
using System.Windows.Controls;

namespace overlay_popup.Commands;

public class SequenceCommand : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return SequenceCommandDefinition.Instance; } }

    private readonly FrameworkElement configElement = SequenceCommandDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    public override bool CanExecute(object? parameter)
    {
        return true;
    }

    public override ActionCommand Clone()
    {
        return new SequenceCommand();
    }

    public override void Execute(object? parameter)
    {
        // TODO
        return;
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

    public override FrameworkElement CreateConfigElement()
    {
        return new TextBox() { Text = "Not implemented" };
    }
}
