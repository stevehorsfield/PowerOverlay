using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;

namespace overlay_popup;

public class ButtonMenuViewModel : INotifyPropertyChanged {

    private readonly ButtonViewModel[] Buttons = new ButtonViewModel[25];

    public event PropertyChangedEventHandler? PropertyChanged;

    private string name = String.Empty;
    private bool canChangeName = true;

    private readonly List<ApplicationMatcherViewModel> menuSelectors = new();
    public List<ApplicationMatcherViewModel> MenuSelectors => menuSelectors;

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

    //internal ButtonMenuViewModel Clone()
    //{
    //    var other = new ButtonMenuViewModel();
    //    other.name = this.name;
    //    other.canChangeName = this.canChangeName;
    //    for (int i = 0; i < this.Buttons.Length; ++i)
    //    {
    //        other.Buttons[i] = this.Buttons[i]?.Clone();
    //    }

    //    return other;
    //}

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
                model[i, j] = config.Buttons[i * 5 + j].Clone();
            }
        }

        foreach (var m in model.MenuSelectors)
        {
            model.MenuSelectors.Add(m.Clone());
        }
        return model;
    }

}
