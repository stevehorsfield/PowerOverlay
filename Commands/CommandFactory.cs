using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace overlay_popup.Commands;

public class CommandFactory
{
    private static readonly List<IActionCommandDefinition> builders = new();

    public static void RegisterCommandType<T>(T builder) where T: IActionCommandDefinition
    {
        builders.Add(builder);
    }

    public static IEnumerable<IActionCommandDefinition> GetCommandTypes()
    {
        return builders.Cast<IActionCommandDefinition>();
    }

    public static void Init()
    {
        RegisterCommandType(SequenceCommandDefinition.Instance);
        RegisterCommandType(SwitchToApplicationDefinition.Instance);
        RegisterCommandType(SendCharactersDefinition.Instance);
        RegisterCommandType(ExecuteCommandDefinition.Instance);
    }

}

public interface IActionCommandDefinition
{
    public string ActionName { get; }
    public string ActionDisplayName { get; }
    public IActionCommand Create();
    public FrameworkElement CreateConfigElement();

}

public interface IActionCommand : ICommand
{
    public IActionCommandDefinition Definition { get; }

    public FrameworkElement ConfigElement { get; }

    IActionCommand Clone();
}

