using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using PowerOverlay.Commands;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Threading;
using Windows.UI.Core;

namespace PowerOverlay;

public class ButtonViewModel : ICommand, INotifyPropertyChanged, IApplicationJson {
    private FrameworkElement? content;
    private string contentSource = String.Empty;
    private string? xamlErrorMessage = String.Empty;
    public enum ContentSourceType
    {
        PlainText,
        XamlFragment,
        Xaml
    };
    private ContentSourceType contentSourceType = ContentSourceType.PlainText;

    private Visibility visibility = Visibility.Visible;

    private static readonly Color DefaultBackgroundColour = Color.FromRgb(80, 80, 80);
    private static readonly Color DefaultForegroundColour = Color.FromRgb(200, 200, 200);
    private static readonly int DefaultFontSize = 16;

    private static readonly Color DefaultBackgroundHoverColour = Color.FromRgb(100, 100, 100);
    private static readonly Color DefaultForegroundHoverColour = Color.FromRgb(230, 230, 230);
    private static readonly int DefaultHoverFontSize = 18;
    
    private static readonly Color DefaultBackgroundPressedColour = Color.FromRgb(140, 140, 140);
    private static readonly Color DefaultForegroundPressedColour = Color.FromRgb(255, 40, 40);
    private static readonly int DefaultPressedFontSize = 20;

    private readonly DisplayStyleViewModel defaultStyle =
        new DisplayStyleViewModel(
            DefaultBackgroundColour,
            DefaultForegroundColour,
            DefaultFontSize,
            String.Empty,
            nameof(FontWeights.Normal),
            nameof(FontStyles.Normal));
    public readonly DisplayStyleViewModel hoverStyle = new DisplayStyleViewModel(
            DefaultBackgroundHoverColour,
            DefaultForegroundHoverColour,
            DefaultHoverFontSize,
            String.Empty,
            nameof(FontWeights.Normal),
            nameof(FontStyles.Normal));
    public readonly DisplayStyleViewModel pressedStyle = new DisplayStyleViewModel(
            DefaultBackgroundPressedColour,
            DefaultForegroundPressedColour,
            DefaultPressedFontSize,
            String.Empty,
            nameof(FontWeights.Normal),
            nameof(FontStyles.Normal));

    public DisplayStyleViewModel DefaultStyle => defaultStyle;
    public DisplayStyleViewModel HoverStyle => hoverStyle;
    public DisplayStyleViewModel PressedStyle => pressedStyle;

    public FrameworkElement? Content {
        get { return content; }
        set {
            content = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
        }
    }

    public string Text { set { SetContent(value); } }
    public string Xaml { set { SetContent(value, true, false); } }
    public string XamlFragment { set { SetContent(value, true, true); } }

    public string XamlErrorMessage { get { return xamlErrorMessage ?? String.Empty; } }
    public bool HasXamlError { get { return !String.IsNullOrEmpty(xamlErrorMessage); } }

    public string RawText
    {
        get { return contentSource; }
        set
        {
            bool isXaml = contentSourceType != ContentSourceType.PlainText;
            bool isFragment = isXaml && contentSourceType == ContentSourceType.XamlFragment;
            SetContent(value, isXaml, isFragment);
        }
    }
    public ContentSourceType ContentFormat
    {
        get { return contentSourceType; }
        set
        {
            bool isXaml = value != ContentSourceType.PlainText;
            bool isFragment = isXaml && value == ContentSourceType.XamlFragment;
            SetContent(contentSource, isXaml, isFragment);
        }
    }
    public bool IsPlainText { get { return contentSourceType == ContentSourceType.PlainText; } }
    public bool IsXaml { get { return contentSourceType == ContentSourceType.Xaml; } }
    public bool IsXamlFragment { get { return contentSourceType == ContentSourceType.XamlFragment; } }

    public bool IsPlainTextCheckboxBehaviour
    {
        get { return contentSourceType == ContentSourceType.PlainText; }
        set
        {
            if (value) ContentFormat = ContentSourceType.PlainText;
        }
    }
    public bool IsXamlCheckboxBehaviour
    {
        get { return contentSourceType == ContentSourceType.Xaml; }
        set
        {
            if (value) ContentFormat = ContentSourceType.Xaml;
        }
    }
    public bool IsXamlFragmentCheckboxBehaviour
    {
        get { return contentSourceType == ContentSourceType.XamlFragment; }
        set
        {
            if (value) ContentFormat = ContentSourceType.XamlFragment;
        }
    }


    private void Notify(params string[] names)
    {
        Array.ForEach(names, n => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n)));
    }
    private void SetAndNotify<T>(ref T field, T value, params string[] names)
    {
        field = value;
        Notify(names);
    }

    private void Set2AndNotify<T, U>(ref T field, T value, ref U field2, U value2, params string[] names)
    {
        field = value; field2 = value2;
        Notify(names);
    }

    public Visibility Visibility {
        get { return visibility; }
        set {
            SetAndNotify(ref visibility, value, nameof(Visibility), nameof(IsVisible));
        }
    }

    public bool IsVisible
    {
        get { return visibility == Visibility.Visible; }
        set
        {
            SetAndNotify(ref visibility, value ? Visibility.Visible : Visibility.Collapsed, nameof(IsVisible), nameof(Visibility));
        }
    }

    private ActionMode actionMode = ActionMode.NoAction;

    public ActionMode ActionMode { 
        get { return this.actionMode; } 
        set
        {
            SetActionMode(value);
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
    public IEnumerable<string> ActionModes => Enum.GetNames<ActionMode>();



    public Visibility MenuListVisibility => actionMode == ActionMode.SelectMenu ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ActionVisibility => actionMode == ActionMode.PerformTask ? Visibility.Visible : Visibility.Collapsed;
    public bool IsNoAction => actionMode == ActionMode.NoAction;
    public bool IsSelectMenu => actionMode == ActionMode.SelectMenu;
    public bool IsPerformTask => actionMode == ActionMode.PerformTask;

    public bool IsNoActionCheckboxBehaviour
    {
        get { return IsNoAction; }
        set { if (value) ActionMode = ActionMode.NoAction; }
    }
    public bool IsSelectMenuCheckboxBehaviour
    {
        get { return IsSelectMenu; }
        set { if (value) ActionMode = ActionMode.SelectMenu; }
    }
    public bool IsPerformTaskCheckboxBehaviour
    {
        get { return IsPerformTask; }
        set { if (value) ActionMode = ActionMode.PerformTask; }
    }

    private ActionCommand? action;
    public string targetMenu = String.Empty;

    public ActionCommand? Action
    {
        get { return action; }
        set
        {
            if (action != null)
            {
                action.CanExecuteChanged -= Action_CanExecuteChanged;
            }
            SetAndNotify(ref action, value, nameof(Action));
            CanExecuteChanged?.Invoke(this, new EventArgs());
            if (action != null)
            {
                action.CanExecuteChanged += Action_CanExecuteChanged;
            }
        }
    }

    private void Action_CanExecuteChanged(object? sender, EventArgs e)
    {
        RaiseCanExecuteChanged();
    }

    public string TargetMenu
    {
        get { return targetMenu; }
        set
        {
            SetAndNotify(ref targetMenu, value, nameof(TargetMenu));
            RaiseCanExecuteChanged();
        }
    }

    private void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, new EventArgs());
    }

    public void SetActionMode(ActionMode mode)
    {
        if (action == null && mode == ActionMode.PerformTask) action = SequenceCommandDefinition.Instance.Create();

        SetAndNotify(ref actionMode, mode, nameof(IsNoAction), nameof(IsSelectMenu), nameof(IsPerformTask), 
            nameof(MenuListVisibility), nameof(ActionVisibility), nameof(Action), nameof(ActionMode),
            nameof(IsNoActionCheckboxBehaviour),nameof(IsSelectMenuCheckboxBehaviour), nameof(IsPerformTaskCheckboxBehaviour));
        RaiseCanExecuteChanged();
    }

    public bool CanExecute(object? o) {
        switch (actionMode)
        {
            case ActionMode.NoAction: return false;
            case ActionMode.SelectMenu: return ! String.IsNullOrEmpty(targetMenu);
        }
        return action?.CanExecute(null) ?? false;
    }

    public void Execute(object? o) {
        AppViewModel appdata = ((AppViewModel)App.Current.MainWindow.DataContext);

        switch (actionMode)
        {
            case ActionMode.NoAction:
                DebugLog.Log($"Button pressed. No action configured");
                return;
            case ActionMode.SelectMenu:
                DebugLog.Log($"Button pressed. Menu selection: '{targetMenu}'");
                if (targetMenu == String.Empty) return;
                foreach (var m in appdata.AllMenus)
                {
                    if (m.Name.Equals(targetMenu, StringComparison.InvariantCultureIgnoreCase))
                    {
                        appdata.CurrentMenu = m;
                        return;
                    }
                }
                DebugLog.Log($"Requested menu not found");
                return;
            case ActionMode.PerformTask:
                break;
            default:
                throw new NotImplementedException($"Action type {actionMode} is not implemented");
        }
        if (Action == null)
        {
            DebugLog.Log($"Button pressed. No action configured");
            return;
        }
        DebugLog.Log($"Button pressed. Executing configured actions");

        Application.Current.MainWindow.Hide();

        try
        {
            var task = Action.AsTask(new CommandExecutionContext()
            {
                MouseCursorPositionX = appdata.MouseX,
                MouseCursorPositionY = appdata.MouseY,
                OriginalActiveWindowHwnd = appdata.ApplicationHwnd,
            });

            if (task != null)
            {
                if (task.Status == TaskStatus.Created) task.Start(TaskScheduler.FromCurrentSynchronizationContext());
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        DebugLog.Log($"Actions completed with error: {t.Exception?.Message ?? "No error details available"}");
                    }
                    else
                    {
                        DebugLog.Log($"Actions completed without error");
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
        catch (Exception e)
        {
            DebugLog.Log($"Actions completed with error: {e.Message}");
        }
    }

    public void SetContent(string text, bool asXaml = false, bool includeBoilerplate = true) {
        try
        {
            this.contentSource = text;
            this.xamlErrorMessage = String.Empty;

            if (!asXaml)
            {
                this.contentSourceType = ContentSourceType.PlainText;

                var tb = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                };
                var lines = text.Split('\n');
                var isFirstLine = true;
                foreach (var l in lines)
                {
                    if (!isFirstLine) tb.Inlines.Add(new LineBreak());
                    tb.Inlines.Add(new Run(l));
                    isFirstLine = false;
                }
                this.Content = tb;
                return;
            }
            var boilerplatePrefix = @"<ContentControl
        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" >
        ";

            var boilerplateSuffix = @"
</ContentControl>
        ";

            if (includeBoilerplate)
            {
                this.contentSourceType = ContentSourceType.XamlFragment;
                text = boilerplatePrefix + text + boilerplateSuffix;
            }
            else
            {
                this.contentSourceType = ContentSourceType.Xaml;
            }
            try
            {
                this.Content = System.Windows.Markup.XamlReader.Parse(text, true) as FrameworkElement;

            }
            catch (System.Windows.Markup.XamlParseException e)
            {
                xamlErrorMessage = e.Message ?? "Unspecified XAML error";
                this.Content = new TextBlock { Text = "Unable to render" };
            }
        }
        finally
        {
            Notify(nameof(Content), nameof(ContentFormat), nameof(RawText), 
                nameof(IsXaml), nameof(IsPlainText), nameof(IsXamlFragment),
                nameof(XamlErrorMessage), nameof(HasXamlError),
                nameof(IsXamlCheckboxBehaviour), nameof(IsPlainTextCheckboxBehaviour), nameof(IsXamlFragmentCheckboxBehaviour));
        }
    }

    internal ButtonViewModel Clone()
    {
        var result = new ButtonViewModel()
        { // Property accessors used to link events and other logic
            Visibility = Visibility,
            ActionMode = actionMode,
            TargetMenu = targetMenu,
            Action = action?.Clone(), 
        };
        DefaultStyle.CopyTo(result.defaultStyle);
        HoverStyle.CopyTo(result.hoverStyle);
        PressedStyle.CopyTo(result.pressedStyle);


        switch (this.contentSourceType)
        {
            case ContentSourceType.PlainText:
                result.SetContent(this.contentSource);
                break;
            case ContentSourceType.XamlFragment:
                result.SetContent(this.contentSource, true);
                break;
            case ContentSourceType.Xaml:
                result.SetContent(this.contentSource, true, false);
                break;
        }

        return result;
    }

    public void PasteStyleFrom(ButtonViewModel source)
    {
        source.DefaultStyle.CopyTo(this.DefaultStyle);
        source.HoverStyle.CopyTo(this.HoverStyle);
        source.PressedStyle.CopyTo(this.PressedStyle);
    }

    public event EventHandler? CanExecuteChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public JsonNode ToJson()
    {
        var n = new JsonObject();
        n.AddLowerCamel(nameof(IsVisible), JsonValue.Create(IsVisible));
        n.AddLowerCamel(nameof(ContentFormat), JsonValue.Create(ContentFormat.ToString()));
        n.AddLowerCamel(nameof(Content), JsonValue.Create(RawText));
        n.AddLowerCamel(nameof(ActionMode), JsonValue.Create(ActionMode.ToString()));

        n.AddLowerCamel(nameof(DefaultStyle), DefaultStyle.ToJson());
        n.AddLowerCamel(nameof(HoverStyle), HoverStyle.ToJson());
        n.AddLowerCamel(nameof(PressedStyle), PressedStyle.ToJson());

        if (ActionMode == ActionMode.SelectMenu)
        {
            n.AddLowerCamel(nameof(TargetMenu), JsonValue.Create(TargetMenu));
        }
        if (ActionMode == ActionMode.PerformTask)
        {
            if (Action != null)
            {
                n.Add(nameof(Action), CommandFactory.ToJson(Action));
            }
        }

        return n;
    }

    public static ButtonViewModel CreateFrom(JsonObject o)
    {
        var result = new ButtonViewModel();
        bool isXaml = false;
        bool isFragment = false;
        o.TryGetValue<bool>(nameof(IsVisible), b => result.IsVisible = b);
        o.TryGet<string>(nameof(ContentFormat), s =>
        {
            switch (Enum.Parse<ContentSourceType>(s, true))
            {
                case ContentSourceType.PlainText: break;
                case ContentSourceType.XamlFragment: isXaml = true; isFragment = true; break;
                case ContentSourceType.Xaml: isXaml = true; break;
            }
        });
        if (o.ContainsKey(nameof(Content)))
        {
            var contentData = o[nameof(Content)]!.GetValue<string>();
            result.SetContent(contentData, isXaml, isFragment);
        }
        if (o.ContainsKey(nameof(ActionMode)))
        {
            result.ActionMode = Enum.Parse<ActionMode>(o[nameof(ActionMode)]!.GetValue<string>(), true);
        }

        o.TryGet<JsonObject>(nameof(DefaultStyle), o => result.DefaultStyle.ReadJson(o));
        o.TryGet<JsonObject>(nameof(HoverStyle), o => result.HoverStyle.ReadJson(o));
        o.TryGet<JsonObject>(nameof(PressedStyle), o => result.PressedStyle.ReadJson(o));
        o.TryGet<string>(nameof(ActionMode), s =>
        {
            var actionMode = Enum.Parse<ActionMode>(s, true);
            result.ActionMode = actionMode;
            switch (actionMode)
            {
                case ActionMode.SelectMenu:
                    o.TryGet<string>(nameof(TargetMenu), s => result.TargetMenu = s);
                    break;
                case ActionMode.PerformTask:
                    o.TryGet<JsonObject>(nameof(Action), o => result.Action = CommandFactory.FromJson(o));
                    break;
            }
        });

        return result;
    }


}