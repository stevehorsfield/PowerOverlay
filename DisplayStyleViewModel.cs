using System;
using System.Reflection;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;

namespace overlay_popup;

public class DisplayStyleViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private Color defaultBackgroundColour;
    private Color defaultForegroundColour;

    private string backgroundColour;
    private Brush? backgroundBrush;
    private string foregroundColour;
    private Brush? foregroundBrush;
    private int fontSize;
    private FontFamily? fontFamily;
    private string fontFamilyName;
    private string fontWeightName;
    private string fontStyleName;

    public Brush BackgroundColourBrush
    {
        get
        {
            return XamlUtils.SetAndReturnSolidColourBrush(ref backgroundBrush, backgroundColour, defaultBackgroundColour);
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

    public Brush ForegroundColourBrush
    {
        get
        {
            return XamlUtils.SetAndReturnSolidColourBrush(
                ref foregroundBrush, foregroundColour, defaultForegroundColour);
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

    public int FontSize
    {
        get { return fontSize; }
        set
        {
            SetAndNotify(ref fontSize, value, nameof(FontSize));
        }
    }

    public string FontFamilyName
    {
        get { return fontFamilyName; }
        set
        {
            Set2AndNotify(ref fontFamilyName, value, ref fontFamily, null, nameof(FontFamilyName), nameof(FontFamily));
        }
    }
    public FontFamily? FontFamily {
        get
        {
            if (this.fontFamily == null)
            {
                if (String.IsNullOrEmpty(FontFamilyName))
                {
                    return new FontFamily();

                }
                fontFamily = new FontFamily(fontFamilyName);
            }
            return fontFamily;
        }
    }
    public FontWeight FontWeight
    {
        get {
            if (fontWeightName == null) return System.Windows.FontWeights.Normal;
            var getMethod = 
                typeof(System.Windows.FontWeights)
                .GetProperty(fontWeightName, BindingFlags.Static | BindingFlags.Public)
                ?.GetGetMethod();

            var result = getMethod?.Invoke(null, null);
            if (result != null) return (System.Windows.FontWeight)result;
            return System.Windows.FontWeights.Normal;
        }
    }

    public string FontWeightName
    {
        get { return fontWeightName; }
        set
        {
            SetAndNotify(ref fontWeightName, value, nameof(FontWeightName), nameof(FontWeight));
        }
    }


    public string FontStyleName
    {
        get { return fontStyleName; }
        set
        {
            SetAndNotify(ref fontStyleName, value, nameof(FontStyleName), nameof(DisplayStyleViewModel.FontStyle));
        }
    }
    public FontStyle FontStyle
    {
        get
        {
            if (fontStyleName == null) return System.Windows.FontStyles.Normal;
            var getMethod =
                typeof(System.Windows.FontStyles)
                .GetProperty(fontStyleName, BindingFlags.Static | BindingFlags.Public)
                ?.GetGetMethod();

            var result = getMethod?.Invoke(null, null);
            if (result != null) return (System.Windows.FontStyle)result;
            return System.Windows.FontStyles.Normal;
        }
    }
    public IEnumerable<string> FontWeights => GetFontWeights();
    private IEnumerable<string> GetFontWeights() {
        return 
            typeof(System.Windows.FontWeights)
                .GetProperties(BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .Where(x => x.GetGetMethod()?.ReturnType.IsAssignableTo(typeof(FontWeight)) ?? false)
                .Select(x => x.Name);
    }


    public IEnumerable<string> FontStyles => GetFontStyles();
    private IEnumerable<string> GetFontStyles()
    {
        return typeof(System.Windows.FontStyles)
            .GetProperties(BindingFlags.Static | System.Reflection.BindingFlags.Public)
            .Where(x => x.GetGetMethod()?.ReturnType.IsAssignableTo(typeof(FontStyle)) ?? false)
            .Select(x => x.Name);
    }

    public DisplayStyleViewModel(
        Color defaultBackgroundColour,
        Color defaultForegroundColour,
        int defaultFontSize,
        string defaultFontFamilyName,
        string defaultFontWeightName,
        string defaultFontStyleName)
    {
        this.defaultBackgroundColour = defaultBackgroundColour;
        this.defaultForegroundColour = defaultForegroundColour;

        backgroundColour = defaultBackgroundColour.ToString();
        foregroundColour = defaultForegroundColour.ToString();
        fontSize = defaultFontSize;
        fontFamilyName = defaultFontFamilyName;
        fontWeightName = defaultFontWeightName;
        fontStyleName = defaultFontStyleName;
    }

    public void CopyTo(DisplayStyleViewModel other)
    {
        other.defaultBackgroundColour = this.defaultBackgroundColour;
        other.defaultForegroundColour = this.defaultForegroundColour;
        other.backgroundBrush = null;
        other.backgroundColour = this.backgroundColour;
        other.foregroundBrush = null;
        other.foregroundColour = this.foregroundColour;
        other.fontSize = this.fontSize;
        other.fontFamilyName = this.fontFamilyName;
        other.fontWeightName = this.fontWeightName;
        other.fontStyleName = this.fontStyleName;
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

}
