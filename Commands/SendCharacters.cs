using System;
using System.Windows;
using System.Windows.Controls;

namespace overlay_popup.Commands;

public class SendCharacters : ActionCommand
{

    public override ActionCommandDefinition Definition { get { return SendCharactersDefinition.Instance; } }

    private readonly FrameworkElement configElement = SendCharactersDefinition.Instance.CreateConfigElement();
    public override FrameworkElement ConfigElement => configElement;

    public override bool CanExecute(object? parameter)
    {
        return true;
    }

    public override ActionCommand Clone()
    {
        return new SendCharacters();
    }

    public override void Execute(object? parameter)
    {
        //TODO
        return;
    }
}

public class SendCharactersDefinition : ActionCommandDefinition
{
    public static SendCharactersDefinition Instance = new();

    public override string ActionName => "SendCharacters";
    public override string ActionDisplayName => "Send text to window";

    public override ActionCommand Create()
    {
        return new SendCharacters();
    }

    public override FrameworkElement CreateConfigElement()
    {
        return new TextBox() { Text = "Not implemented" };
    }
}
