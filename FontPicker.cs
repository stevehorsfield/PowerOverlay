using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using DrawingFontFamily = System.Drawing.FontFamily;

namespace overlay_popup;

public class FontPicker : ComboBox
{
    public FontPicker() : base()
    {
        DrawingFontFamily[] families = DrawingFontFamily.Families ?? Array.Empty<DrawingFontFamily>();

        var names = families.Select(x => x.Name).ToList();
        names.Add(String.Empty);
        names.Sort();

        ItemsSource = names;
    }
}
