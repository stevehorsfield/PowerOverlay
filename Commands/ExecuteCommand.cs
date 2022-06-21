using System;
using System.Windows;
using System.Windows.Controls;

namespace overlay_popup.Commands;

public class ExecuteCommand : IActionCommand
{

    public IActionCommandDefinition Definition { get { return ExecuteCommandDefinition.Instance; } }

    public event EventHandler? CanExecuteChanged;

    private readonly FrameworkElement configElement = ExecuteCommandDefinition.Instance.CreateConfigElement();
    public FrameworkElement ConfigElement => configElement;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public IActionCommand Clone()
    {
        return new ExecuteCommand();
    }

    public void Execute(object? parameter)
    {
        // TODO
        return;
    }
}

public class ExecuteCommandDefinition : IActionCommandDefinition
{
    public static ExecuteCommandDefinition Instance = new();

    public string ActionName => "ExecuteCommand";
    public string ActionDisplayName => "Execute a command";

    public IActionCommand Create()
    {
        return new ExecuteCommand();
    }

    public FrameworkElement CreateConfigElement()
    {
        return new TextBox() { Text = "Not implemented" };
    }
}
