using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace overlay_popup.Commands;

public class SwitchToApplication : IActionCommand
{

    public IActionCommandDefinition Definition { get { return SwitchToApplicationDefinition.Instance; } }

    public event EventHandler? CanExecuteChanged;
    private readonly FrameworkElement configElement = SwitchToApplicationDefinition.Instance.CreateConfigElement();
    public FrameworkElement ConfigElement => configElement;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public IActionCommand Clone()
    {
        return new SwitchToApplication();
    }

    public void Execute(object? parameter)
    {
        // todo: No-Op
    }
}

public class SwitchToApplicationDefinition : IActionCommandDefinition
{
    public static SwitchToApplicationDefinition Instance = new();

    public string ActionName => "SwitchApp";
    public string ActionDisplayName => "Switch to app";

    public IActionCommand Create()
    {
        return new SwitchToApplication();
    }

    public FrameworkElement CreateConfigElement()
    {
        return new TextBox() { Text = "Not implemented" };
    }
}
