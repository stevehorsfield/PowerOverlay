using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace overlay_popup.Commands;

public class CommandFactory
{
    private static readonly List<ActionCommandDefinition> builders = new();

    public static void RegisterCommandType<T>(T builder) where T: ActionCommandDefinition
    {
        builders.Add(builder);
    }

    public static IEnumerable<ActionCommandDefinition> GetCommandTypes()
    {
        return builders.Cast<ActionCommandDefinition>();
    }

    public static void Init()
    {
        RegisterCommandType(SequenceCommandDefinition.Instance);
        RegisterCommandType(SwitchToApplicationDefinition.Instance);
        RegisterCommandType(SendCharactersDefinition.Instance);
        RegisterCommandType(ExecuteCommandDefinition.Instance);
    }

}

public abstract class ActionCommandDefinition
{
    public abstract string ActionName { get; }
    public abstract string ActionDisplayName { get; }
    public abstract ActionCommand Create();
    public abstract FrameworkElement CreateConfigElement();

}

public abstract class ActionCommand : ICommand, INotifyPropertyChanged
{
    public abstract ActionCommand Clone();
    public abstract ActionCommandDefinition Definition { get; }
    public abstract FrameworkElement ConfigElement { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void RaisePropertyChanged(string propertyName)
    { 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event EventHandler? CanExecuteChanged;
    protected void RaiseCanExecuteChanged(EventArgs e)
    {
        CanExecuteChanged?.Invoke(this, e);
    }

    public abstract bool CanExecute(object? parameter);
    public abstract void Execute(object? parameter);
}

