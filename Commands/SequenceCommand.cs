using System;
using System.Windows;
using System.Windows.Controls;

namespace overlay_popup.Commands;

public class SequenceCommand : IActionCommand
{

    public IActionCommandDefinition Definition { get { return SequenceCommandDefinition.Instance; } }

    public event EventHandler? CanExecuteChanged;
    private readonly FrameworkElement configElement = SequenceCommandDefinition.Instance.CreateConfigElement();
    public FrameworkElement ConfigElement => configElement;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public IActionCommand Clone()
    {
        return new SequenceCommand();
    }

    public void Execute(object? parameter)
    {
        // TODO
        return;
    }
}

public class SequenceCommandDefinition : IActionCommandDefinition
{
    public static SequenceCommandDefinition Instance = new();

    public string ActionName => "SequenceCommand";
    public string ActionDisplayName => "Execute a sequence of commands";

    public IActionCommand Create()
    {
        return new SequenceCommand();
    }

    public FrameworkElement CreateConfigElement()
    {
        return new TextBox() { Text = "Not implemented" };
    }
}
