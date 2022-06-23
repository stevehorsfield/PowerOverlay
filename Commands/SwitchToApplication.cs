using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace overlay_popup.Commands;

public class SwitchToApplication : ActionCommand
{

    public override ActionCommandDefinition Definition { get { return SwitchToApplicationDefinition.Instance; } }

    private readonly FrameworkElement configElement = SwitchToApplicationDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    public override bool CanExecute(object? parameter)
    {
        return true;
    }

    public override ActionCommand Clone()
    {
        return new SwitchToApplication();
    }

    public override void Execute(object? parameter)
    {
        // todo: No-Op
    }
}

public class SwitchToApplicationDefinition : ActionCommandDefinition
{
    public static SwitchToApplicationDefinition Instance = new();

    public override string ActionName => "SwitchApp";
    public override string ActionDisplayName => "Switch to app";

    public override ActionCommand Create()
    {
        return new SwitchToApplication();
    }

    public override FrameworkElement CreateConfigElement()
    {
        return new TextBox() { Text = "Not implemented" };
    }
}
