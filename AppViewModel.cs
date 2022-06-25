﻿using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Specialized;
using overlay_popup.Commands;

namespace overlay_popup;

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
            },
            new ButtonMenuViewModel()
            {
                Name = "Menu 3",
                CanChangeName = true,
            }
        };
        
        AllMenus[0][0, 0].DefaultStyle.BackgroundColour = "#FF000000";
        AllMenus[0][0, 0].HoverStyle.BackgroundColour = "#FF400000";
        var action = (ExecuteCommand) ExecuteCommandDefinition.Instance.Create();
        action.ExecutablePath = @"C:\windows\notepad.exe";
        action.WaitForInputIdle = true;
        action.WaitTimeoutMilliseconds = 300;
        action.Arguments.Add(@"C:\temp.txt");
        AllMenus[0][0, 0].Action = action;
        AllMenus[0][0, 0].SetActionMode(ButtonViewModel.ActionMode.PerformTask);
        AllMenus[0][0, 0].Text = "Notepad";

        AllMenus[0][1, 0].DefaultStyle.BackgroundColour = "#FF500000";
        AllMenus[0][1, 0].TargetMenu = "Alternate";
        AllMenus[0][1, 0].SetActionMode(ButtonViewModel.ActionMode.SelectMenu);
        AllMenus[0][1, 0].SetContent(@"<TextBlock>Menu -&gt;<LineBreak/>Alternate</TextBlock>",true,true);

        AllMenus[0][2, 0].DefaultStyle.BackgroundColour = "#FF500000";
        AllMenus[0][2, 0].TargetMenu = "Menu 3";
        AllMenus[0][2, 0].SetActionMode(ButtonViewModel.ActionMode.SelectMenu);
        AllMenus[0][2, 0].Text = "Menu:\nMenu 3";

        AllMenus[0][3, 0].DefaultStyle.BackgroundColour = "#FF404040";
        AllMenus[0][3, 0].HoverStyle.BackgroundColour = "#FFE0E0A0";
        AllMenus[0][3, 0].HoverStyle.ForegroundColour = "#FF404080";
        AllMenus[0][3, 0].SetActionMode(ButtonViewModel.ActionMode.PerformTask);
        AllMenus[0][3, 0].Action = SendCharactersDefinition.Instance.Create();
        AllMenus[0][3, 0].DefaultStyle.FontFamilyName = "Segoe UI Emoji";
        AllMenus[0][3, 0].HoverStyle.FontFamilyName = "Segoe UI Emoji";
        AllMenus[0][3, 0].PressedStyle.FontFamilyName = "Segoe UI Emoji";
        AllMenus[0][3, 0].Text = "🎁";

        AllMenus[1][0, 0].DefaultStyle.BackgroundColour = "#FF0000FF";
        AllMenus[1][0, 1].Text = "Go to Default";
        AllMenus[1][0, 1].TargetMenu = "Default";
        AllMenus[1][0, 1].SetActionMode(ButtonViewModel.ActionMode.SelectMenu);

        AllMenus[2][0, 0].Text = "Go to Alternate";
        AllMenus[2][0, 0].TargetMenu = "Alternate";
        AllMenus[2][0, 0].SetActionMode(ButtonViewModel.ActionMode.SelectMenu);
        AllMenus[2][0, 1].Text = "Go to Default";
        AllMenus[2][0, 1].TargetMenu = "Default";
        AllMenus[2][0, 1].SetActionMode(ButtonViewModel.ActionMode.SelectMenu);

        AllMenus[0][0, 1].Text = "Sequence test";
        AllMenus[0][0, 1].SetActionMode(ButtonViewModel.ActionMode.PerformTask);
        var action2 = (SequenceCommand)SequenceCommandDefinition.Instance.Create();
        AllMenus[0][0, 1].Action = action2;
        action2.Actions.Add(ExecuteCommandDefinition.Instance.Create());
        

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
        foreach (var x in config.Menus.Select(m => ButtonMenuViewModel.CreateFrom(m)))
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
