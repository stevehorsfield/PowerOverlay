using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Collections.Specialized;

namespace overlay_popup;

public class ConfigurationViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ConfigurationButtonMenuViewModel> Menus { get; private set; }

    public ConfigurationViewModel(AppViewModel source)
    {
        Menus = new ObservableCollection<ConfigurationButtonMenuViewModel>(
            source.AllMenus.Select(m => new ConfigurationButtonMenuViewModel(m)));
    }
}

public class ConfigurationButtonMenuViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ButtonViewModel> Buttons { get; private set; }

    private string name = String.Empty;
    public string Name
    {
        get { return this.name; }
        set
        {
            this.name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    private bool canChangeName = true;
    public bool CanChangeName
    {
        get
        {
            return canChangeName;
        }
        set
        {
            canChangeName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanChangeName)));
        }
    }
    public bool IsReadOnly { get { return !canChangeName; } }

    public ConfigurationButtonMenuViewModel(ButtonMenuViewModel source)
    {
        name = source.Name;
        canChangeName = source.CanChangeName;
        Buttons = new ObservableCollection<ButtonViewModel>();
        for (int i = 0; i < 5; ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                if (i == 2 && j == 2)
                {
                    Buttons.Add(new ButtonViewModel()
                    {
                        Visibility = Visibility.Hidden,
                    });
                    continue;
                }
                Buttons.Add(source[j, i].Clone());
            }
        }
    }
}

public class AppViewModel : INotifyPropertyChanged {

    private string appWindowTitle = String.Empty;
    private string appProcessName = String.Empty;
    private ButtonMenuViewModel? currentMenu;

    public ObservableCollection<ButtonMenuViewModel> AllMenus { get; private set; }


    public event PropertyChangedEventHandler? PropertyChanged;

    public string ApplicationWindowTitle
    {
        get { return this.appWindowTitle; }
        set
        {
            this.appWindowTitle = value;
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("ApplicationWindowTitle"));
            }
        }
    }

    public string ApplicationProcessName
    {
        get { return this.appProcessName; }
        set
        {
            this.appProcessName = value;
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("ApplicationProcessName"));
            }
        }
    }

    public ButtonMenuViewModel? CurrentMenu
    {
        get { return this.currentMenu; }
        set
        {
            this.currentMenu = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentMenu"));
        }
    }

    public AppViewModel()
    {
        this.AllMenus = new()
        {
            new ButtonMenuViewModel()
            {
                Name = "Default",
                CanChangeName = false,
            },
            new ButtonMenuViewModel()
            {
                Name = "Alternate",
                CanChangeName = true,
            }
        };
        AllMenus[0][0, 0].BackgroundColour = "#FF000000";
        AllMenus[1][0, 0].BackgroundColour = "#FF0000FF";

        CurrentMenu = this.AllMenus[0];
    }

    internal void RefreshCurrentApp()
    {
        IntPtr hwndApp = NativeUtils.GetActiveAppHwnd();
        this.ApplicationWindowTitle = NativeUtils.GetWindowTitle(hwndApp);
        
        this.ApplicationProcessName = NativeUtils.GetWindowProcessName(hwndApp);

    }

    public void ApplyFrom(ConfigurationViewModel config)
    {
        string currentMenu = this.CurrentMenu?.Name ?? String.Empty;
        this.AllMenus.Clear();
        foreach (var x in config.Menus.Select(m => ButtonViewModel.CreateFrom(m)))
        {
            this.AllMenus.Add(x);
        }

        var finder = (string v) => {
            for (var i = 0; i < AllMenus.Count; ++i)
            {
                if (String.Equals(AllMenus[i].Name, v, StringComparison.InvariantCultureIgnoreCase)) return i;
            }
            return -1;
        };

        var indexCurrent = finder(currentMenu);
        if (indexCurrent == -1) indexCurrent = finder("Default");
        if (indexCurrent == -1 && this.AllMenus.Count > 0) indexCurrent = 0;

        this.CurrentMenu = (indexCurrent >= 0) ? this.AllMenus[indexCurrent] : null;
    }
}

public class ButtonMenuViewModel : INotifyPropertyChanged {

    private readonly ButtonViewModel[] Buttons = new ButtonViewModel[25];

    public event PropertyChangedEventHandler? PropertyChanged;

    private string name = String.Empty;
    private bool canChangeName = true;

    public string Name
    {
        get { return this.name; }
        set
        {
            this.name = value;
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }
    }
    public bool CanChangeName
    {
        get { return this.canChangeName; }
        set
        {
            this.canChangeName = value;
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("CanChangeName"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("IsReadOnly"));
            }
        }
    }

    public bool IsReadOnly { get { return !canChangeName; } }

    public ButtonViewModel this[int x, int y]
    {
        get {  
            if (x == 2 && y == 2) throw new NotSupportedException("Middle position is not used");
            if (x > 4 || x < 0 || y > 4 || y < 0) throw new IndexOutOfRangeException("Position out of bounds");
            return Buttons[y*5+x];
        }
        set {  
            if (x == 2 && y == 2) throw new NotSupportedException("Middle position is not used");
            if (x > 4 || x < 0 || y > 4 || y < 0) throw new IndexOutOfRangeException("Position out of bounds");
            Buttons[y*5+x] = value;
        }
    }

    public ButtonMenuViewModel() {
        for (int i = 0; i < 5; ++i) {
            for (int j = 0; j < 5; ++j) {
                if (i == 2 && j == 2) continue;
                this[i,j] = new ButtonViewModel {
                    Text = "untitled",
                    Visibility = Visibility.Visible,
                };
            }
        }

        this[2,1].XamlFragment = @"
        <TextBlock>abc</TextBlock>";
        this[2, 4].Text = @"🤣🤣🤣";
    }

    internal ButtonMenuViewModel Clone()
    {
        var other = new ButtonMenuViewModel();
        other.name = this.name;
        other.canChangeName = this.canChangeName;
        for (int i = 0; i < this.Buttons.Length; ++i)
        {
            other.Buttons[i] = this.Buttons[i]?.Clone();
        }

        return other;
    }

}

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
    public bool IsPlainText { get { return contentSourceType == ContentSourceType.PlainText;  } }
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

    private void Set2AndNotify<T,U>(ref T field, T value, ref U field2, U value2, params string[] names)
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

    public bool CanExecute(object? o) {
        return true;
    }

    public void Execute(object? o) {

    }

    public void SetContent(string text, bool asXaml = false, bool includeBoilerplate = true) {
        try
        {
            this.contentSource = text;
            this.xamlErrorMessage = String.Empty;

            if (!asXaml)
            {
                this.contentSourceType = ContentSourceType.PlainText;
                this.Content = new TextBlock { Text = text };
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

    public static ButtonMenuViewModel CreateFrom(ConfigurationButtonMenuViewModel config)
    {
        var model = new ButtonMenuViewModel()
        {
            Name = config.Name,
            CanChangeName = config.CanChangeName
        };
        for (int i = 0; i < 5; ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                if (i == 2 && j == 2) continue;
                model[i, j] = config.Buttons[j * 5 + i].Clone();
            }
        }
        return model;
    }


    public event EventHandler? CanExecuteChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
}