using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using overlay_popup.Commands;
using System.Windows.Documents;

namespace overlay_popup;

public class ButtonViewModel : ICommand, INotifyPropertyChanged {
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
    private static readonly Color DefaultBackgroundHoverColour = Color.FromRgb(100, 100, 100);
    private static readonly Color DefaultBackgroundPressedColour = Color.FromRgb(140, 140, 140);
    private static readonly Color DefaultForegroundColour = Color.FromRgb(200, 200, 200);
    private static readonly Color DefaultForegroundHoverColour = Color.FromRgb(230, 230, 230);
    private static readonly Color DefaultForegroundPressedColour = Color.FromRgb(255, 40, 40);
    private static readonly int DefaultFontSize = 16;
    private static readonly int DefaultHoverFontSize = 18;
    private static readonly int DefaultPressedFontSize = 20;

    private string backgroundColour = DefaultBackgroundColour.ToString();
    private Brush? backgroundBrush;

    private string backgroundHoverColour = DefaultBackgroundHoverColour.ToString();
    private Brush? backgroundHoverBrush;

    private string backgroundPressedColour = DefaultBackgroundPressedColour.ToString();
    private Brush? backgroundPressedBrush;

    private string foregroundColour = DefaultForegroundColour.ToString();
    private Brush? foregroundBrush;

    private string foregroundHoverColour = DefaultForegroundHoverColour.ToString();
    private Brush? foregroundHoverBrush;

    private string foregroundPressedColour = DefaultForegroundPressedColour.ToString();
    private Brush? foregroundPressedBrush;

    private int fontSize = DefaultFontSize;
    private int hoverFontSize = DefaultHoverFontSize;
    private int pressedFontSize = DefaultPressedFontSize;

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

    public Brush BackgroundColourBrush
    {
        get
        {
            return XamlUtils.SetAndReturnSolidColourBrush(ref backgroundBrush, backgroundColour, DefaultBackgroundColour);
        }
    }
    public string BackgroundColour
    {
        get { return this.backgroundColour; }
        set
        {
            Set2AndNotify(
                ref this.backgroundColour, value,
                ref this.backgroundBrush, null,
                nameof(BackgroundColour), nameof(BackgroundColourBrush));
        }
    }

    public Brush BackgroundHoverColourBrush
    {
        get
        {
            return XamlUtils.SetAndReturnSolidColourBrush(ref backgroundHoverBrush, backgroundHoverColour, DefaultBackgroundHoverColour);
        }
    }
    public string BackgroundHoverColour
    {
        get { return this.backgroundHoverColour; }
        set
        {
            Set2AndNotify(
                ref this.backgroundHoverColour, value,
                ref this.backgroundHoverBrush, null,
                nameof(BackgroundHoverColour), nameof(BackgroundHoverColourBrush));
        }
    }

    public Brush BackgroundPressedColourBrush
    {
        get
        {
            return XamlUtils.SetAndReturnSolidColourBrush(ref backgroundPressedBrush, backgroundPressedColour, DefaultBackgroundPressedColour);
        }
    }
    public string BackgroundPressedColour
    {
        get { return this.backgroundPressedColour; }
        set
        {
            Set2AndNotify(
                ref this.backgroundPressedColour, value,
                ref this.backgroundPressedBrush, null,
                nameof(BackgroundPressedColour), nameof(BackgroundPressedColourBrush));
        }
    }

    public Brush ForegroundColourBrush
    {
        get
        {
            return XamlUtils.SetAndReturnSolidColourBrush(
                ref foregroundBrush, foregroundColour, DefaultForegroundColour);
        }
    }
    public string ForegroundColour
    {
        get { return this.foregroundColour; }
        set
        {
            Set2AndNotify(
                ref foregroundColour, value,
                ref foregroundBrush, null,
                nameof(ForegroundColour), nameof(ForegroundColourBrush));
        }
    }

    public Brush ForegroundHoverColourBrush
    {
        get
        {
            return XamlUtils.SetAndReturnSolidColourBrush(
                ref foregroundHoverBrush, foregroundHoverColour, DefaultForegroundHoverColour);
        }
    }
    public string ForegroundHoverColour
    {
        get { return this.foregroundHoverColour; }
        set
        {
            Set2AndNotify(
                ref foregroundHoverColour, value,
                ref foregroundHoverBrush, null,
                nameof(ForegroundHoverColour), nameof(ForegroundHoverColourBrush));
        }
    }

    public Brush ForegroundPressedColourBrush
    {
        get
        {
            return XamlUtils.SetAndReturnSolidColourBrush(
                ref foregroundPressedBrush, foregroundPressedColour, DefaultForegroundPressedColour);
        }
    }
    public string ForegroundPressedColour
    {
        get { return this.foregroundPressedColour; }
        set
        {
            Set2AndNotify(
                ref foregroundPressedColour, value,
                ref foregroundPressedBrush, null,
                nameof(ForegroundPressedColour), nameof(ForegroundPressedColourBrush));
        }
    }

    public int FontSize {
        get { return fontSize; }
        set
        {
            SetAndNotify(ref fontSize, value, nameof(FontSize));
        }
    }
    public int HoverFontSize
    {
        get { return hoverFontSize; }
        set
        {
            SetAndNotify(ref hoverFontSize, value, nameof(HoverFontSize));
        }
    }
    public int PressedFontSize
    {
        get { return pressedFontSize; }
        set
        {
            SetAndNotify(ref pressedFontSize, value, nameof(PressedFontSize));
        }
    }

    public enum ActionMode
    {
        NoAction,
        SelectMenu,
        PerformTask
    }
    private ActionMode actionMode = ActionMode.NoAction;

    public Visibility MenuListVisibility => actionMode == ActionMode.SelectMenu ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ActionVisibility => actionMode == ActionMode.PerformTask ? Visibility.Visible : Visibility.Collapsed;
    public bool IsNoAction => actionMode == ActionMode.NoAction;
    public bool IsSelectMenu => actionMode == ActionMode.SelectMenu;
    public bool IsPerformTask => actionMode == ActionMode.PerformTask;

    private ActionCommand? action;
    public string targetMenu = String.Empty;

    public ActionCommand? Action
    {
        get { return action; }
        set
        {
            SetAndNotify(ref action, value, nameof(Action));
        }
    }
    public string TargetMenu
    {
        get { return targetMenu; }
        set
        {
            SetAndNotify(ref targetMenu, value, nameof(TargetMenu));
        }
    }

    public void SetActionMode(ActionMode mode)
    {
        SetAndNotify(ref actionMode, mode, nameof(IsNoAction), nameof(IsSelectMenu), nameof(IsPerformTask), 
            nameof(MenuListVisibility), nameof(ActionVisibility));
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
                return;
            case ActionMode.SelectMenu:
                if (targetMenu == String.Empty) return;
                foreach (var m in appdata.AllMenus)
                {
                    if (m.Name.Equals(targetMenu, StringComparison.InvariantCultureIgnoreCase))
                    {
                        appdata.CurrentMenu = m;
                        return;
                    }
                }
                MessageBox.Show(Application.Current.MainWindow, $"Menu '{targetMenu}' is not found");
                return;
            case ActionMode.PerformTask:
                break;
            default:
                throw new NotImplementedException($"Action type {actionMode} is not implemented");
        }
        if (Action == null) return;

        Application.Current.MainWindow.Hide();
        Action.Execute(null);
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
                nameof(XamlErrorMessage), nameof(HasXamlError));
        }
    }

    internal ButtonViewModel Clone()
    {
        var result = new ButtonViewModel()
        {
            Visibility = Visibility,
            FontSize = FontSize,
            HoverFontSize = HoverFontSize,
            PressedFontSize = PressedFontSize,
            // Brushes created automatically, only colours need to be set
            BackgroundColour = BackgroundColour,
            BackgroundHoverColour = BackgroundHoverColour,
            BackgroundPressedColour = BackgroundPressedColour,
            ForegroundColour = ForegroundColour,
            ForegroundHoverColour = ForegroundHoverColour,
            ForegroundPressedColour = ForegroundPressedColour,

            actionMode = actionMode,
            targetMenu = targetMenu,
            action = action?.Clone(),
        };

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
        this.BackgroundColour = source.BackgroundColour;
        this.BackgroundHoverColour = source.BackgroundHoverColour;
        this.BackgroundPressedColour = source.BackgroundPressedColour;
        this.ForegroundColour = source.ForegroundColour;
        this.ForegroundHoverColour = source.ForegroundHoverColour;
        this.ForegroundPressedColour = source.ForegroundPressedColour;
        this.FontSize = source.FontSize;
        this.HoverFontSize = source.HoverFontSize;
        this.PressedFontSize = source.PressedFontSize;
    }

    public event EventHandler? CanExecuteChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
}