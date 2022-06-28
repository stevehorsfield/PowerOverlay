using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace overlay_popup;

public class ConfigurationButtonMenuViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ButtonViewModel> Buttons { get; private set; }

    public ObservableCollection<ApplicationMatcherViewModel> MenuSelectors { get; private set; }

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
        MenuSelectors = new ObservableCollection<ApplicationMatcherViewModel>();

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
                Buttons.Add(source[i, j].Clone());
            }
        }

        foreach (var selector in source.MenuSelectors)
        {
            MenuSelectors.Add(selector.Clone());
        }
    }
}
