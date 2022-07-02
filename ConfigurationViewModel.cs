using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using overlay_popup.Commands;
using System;

namespace overlay_popup;

public class ConfigurationViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    ButtonViewModel? clipboardButton;
    public ButtonViewModel? ClipboardButton {
        get { return clipboardButton; }
        set
        {
            this.clipboardButton = value?.Clone();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipboardButton)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasClipboardButton)));
        }
    }
    public bool HasClipboardButton { get { return clipboardButton != null; } }

    class CommandType { public string Name { get; set; } public string DisplayName { get; set; }
        public CommandType()
        {
            Name = String.Empty;
            DisplayName = String.Empty;
        }
    }
    public List<ActionCommandDefinition> CommandTypes { get; private set; }

    public ObservableCollection<ConfigurationButtonMenuViewModel> Menus { get; private set; }

    public ConfigurationViewModel(AppViewModel source)
    {
        Menus = new ObservableCollection<ConfigurationButtonMenuViewModel>(
            source.AllMenus.Select(m => new ConfigurationButtonMenuViewModel(m)));

        CommandTypes = new(CommandFactory.GetCommandTypes());
    }
}
