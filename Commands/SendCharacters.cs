using System;
using System.Windows;
using System.Windows.Controls;

namespace overlay_popup.Commands;

public class SendCharacters : IActionCommand
{

    public IActionCommandDefinition Definition { get { return SendCharactersDefinition.Instance; } }

    public event EventHandler? CanExecuteChanged;

    private readonly FrameworkElement configElement = SendCharactersDefinition.Instance.CreateConfigElement();
    public FrameworkElement ConfigElement => configElement;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public IActionCommand Clone()
    {
        return new SendCharacters();
    }

    public void Execute(object? parameter)
    {
        //TODO
        return;
    }
}

public class SendCharactersDefinition : IActionCommandDefinition
{
    public static SendCharactersDefinition Instance = new();

    public string ActionName => "SendCharacters";
    public string ActionDisplayName => "Send text to window";

    public IActionCommand Create()
    {
        return new SendCharacters();
    }

    public FrameworkElement CreateConfigElement()
    {
        return new TextBox() { Text = "Not implemented" };
    }
}
