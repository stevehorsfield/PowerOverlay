﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PowerOverlay.Commands;

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
        RegisterCommandType(PositionWindowDefinition.Instance);
        RegisterCommandType(SleepDefinition.Instance);
        RegisterCommandType(SendKeysDefinition.Instance);
        RegisterCommandType(SendMouseDefinition.Instance);
        RegisterCommandType(SendAppCommandDefinition.Instance);
        RegisterCommandType(CloseCommandDefinition.Instance);
        RegisterCommandType(AudioControlDefinition.Instance);
    }

    public static JsonNode ToJson(ActionCommand action)
    {
        var n = new JsonObject();
        n.Add("actionType", JsonValue.Create(action.Definition.ActionName.ToLowerCamelCase()));
        action.WriteJson(n);
        return n;
    }

    public static ActionCommand? FromJson(JsonObject o)
    {
        var actionType = String.Empty;
        o.TryGet<string>("actionType", s => actionType = s);
        if (String.IsNullOrEmpty(actionType)) return null;

        var builder = builders.FirstOrDefault(a =>
            a.ActionName.Equals(actionType, StringComparison.InvariantCultureIgnoreCase)
            , null);

        var result = builder?.CreateFromJson(o);
        return result;
    }
}

public abstract class ActionCommandDefinition
{
    public abstract string ActionName { get; }
    public abstract string ActionDisplayName { get; }
    public abstract string ActionShortName { get; }

    public abstract ImageSource ActionImage { get; }

    public abstract ActionCommand Create();
    public abstract ActionCommand CreateFromJson(JsonObject o);
    public abstract FrameworkElement CreateConfigElement();
}

public abstract class ActionCommand : ICommand, INotifyPropertyChanged
{
    public abstract void WriteJson(JsonObject o);
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
    
    public void Execute(object? parameter)
    {
        var task = AsTask(parameter);
        if (task.Status == TaskStatus.Created) task.Start(TaskScheduler.FromCurrentSynchronizationContext());
        Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
    }

    public Task AsTask(object? parameter)
    {
        DebugLog.Log($"Executing action of type {Definition.ActionName} ({Definition.ActionDisplayName})");

        var cec = parameter as CommandExecutionContext;
        if (cec == null)
        {
            return ExecuteWithContext(new CommandExecutionContext());
        }
        else
        {
            return ExecuteWithContext(cec);
        }
    }

    public abstract Task ExecuteWithContext(CommandExecutionContext context);
}

