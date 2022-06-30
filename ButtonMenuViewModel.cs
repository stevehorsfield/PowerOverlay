using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Linq;

namespace overlay_popup;

public class ButtonMenuViewModel : INotifyPropertyChanged, IApplicationJson {

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
                model[i, j] = config.Buttons[i * 5 + j].Clone();
            }
        }

        foreach (var m in config.MenuSelectors)
        {
            model.MenuSelectors.Add(m.Clone());
        }
        return model;
    }

    public JsonNode ToJson()
    {
        var n = new JsonObject();
        n.AddLowerCamel(nameof(Name), JsonValue.Create(Name));
        n.AddLowerCamel(nameof(MenuSelectors), MenuSelectors.ToJson());

        for (int row = 0; row < 5; ++row)
        {
            for (int column = 0; column < 5; ++column)
            {
                if (row == 2 && column == 2) continue;
                n[$"button_row{row}_column{column}"] = this[column, row].ToJson();
            }
        }

        return n;
    }

}
