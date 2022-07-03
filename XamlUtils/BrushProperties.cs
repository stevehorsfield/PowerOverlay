namespace PowerOverlay;

using System.Windows.Controls;
using System.Windows.Media;

public partial class XamlUtils
{
    static public Color ColorOrDefault(string? value, Color defaultColour)
    {
        if (value == null) return defaultColour;
        return (Color) (new ColorConverter().ConvertFromInvariantString(value) ?? defaultColour);
    }
    static public Brush SolidColourBrush(string? value, Color defaultColour)
    {
        return new SolidColorBrush(ColorOrDefault(value, defaultColour));
    }
    static public Brush SetAndReturnSolidColourBrush(ref Brush? result, string? value, Color defaultColor)
    {
        result ??= SolidColourBrush(value, defaultColor);
        return result;
    }
}