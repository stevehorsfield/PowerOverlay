using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Specialized;
using overlay_popup.Commands;
using System.Text.RegularExpressions;
using System.Windows;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.IO;
using System.Windows.Input;

namespace overlay_popup;

public class AppViewModel : INotifyPropertyChanged {

    private string appWindowTitle = String.Empty;
    private string appProcessName = String.Empty;
    private string appProcessExecutable = String.Empty;
    private IntPtr applicationHwnd = IntPtr.Zero;
    private ButtonMenuViewModel? currentMenu;

    private bool lockMenu;
    public bool LockMenu { 
        get { return lockMenu; } 
        set { 
            lockMenu = value; 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LockMenu))); 
        }
    }

    public ObservableCollection<ButtonMenuViewModel> AllMenus { get; private set; }


    public event PropertyChangedEventHandler? PropertyChanged;

    internal void AddTestData()
    {
        AllMenus.Add(new ButtonMenuViewModel()
        {
            Name = "Alternate",
            CanChangeName = true,
        });
        AllMenus.Add(new ButtonMenuViewModel()
        {
            Name = "Menu 3",
            CanChangeName = true,
        });
        AllMenus.Add(new ButtonMenuViewModel()
        {
            Name = "Notepad menu",
        });
        AllMenus[0][0, 0].DefaultStyle.BackgroundColour = "#FF000000";
        AllMenus[0][0, 0].HoverStyle.BackgroundColour = "#FF400000";
        var action = (ExecuteCommand)ExecuteCommandDefinition.Instance.Create();
        action.ExecutablePath = @"C:\windows\notepad.exe";
        action.WaitForInputIdle = true;
        action.WaitTimeoutMilliseconds = 300;
        action.Arguments.Add(@"C:\temp.txt");
        AllMenus[0][0, 0].Action = action;
        AllMenus[0][0, 0].SetActionMode(ActionMode.PerformTask);
        AllMenus[0][0, 0].Text = "Notepad";

        AllMenus[0][0, 4].Text = "Switch to notepad";
        AllMenus[0][0, 4].SetActionMode(ActionMode.PerformTask);
        AllMenus[0][0, 4].Action = SwitchToApplicationDefinition.Instance.Create();
        ((SwitchToApplication)AllMenus[0][0, 4].Action!).ApplicationTargets.Add(new ApplicationMatcherViewModel());
        ((SwitchToApplication)AllMenus[0][0, 4].Action!).ApplicationTargets[0].UseRegexForExecutable = true;
        ((SwitchToApplication)AllMenus[0][0, 4].Action!).ApplicationTargets[0].ExecutablePattern = ".*notepad.exe$";


        AllMenus[0][1, 0].DefaultStyle.BackgroundColour = "#FF500000";
        AllMenus[0][1, 0].TargetMenu = "Alternate";
        AllMenus[0][1, 0].SetActionMode(ActionMode.SelectMenu);
        AllMenus[0][1, 0].SetContent(@"<TextBlock>Menu -&gt;<LineBreak/>Alternate</TextBlock>", true, true);

        AllMenus[0][2, 0].DefaultStyle.BackgroundColour = "#FF500000";
        AllMenus[0][2, 0].TargetMenu = "Menu 3";
        AllMenus[0][2, 0].SetActionMode(ActionMode.SelectMenu);
        AllMenus[0][2, 0].Text = "Menu:\nMenu 3";

        AllMenus[0][3, 0].DefaultStyle.BackgroundColour = "#FF404040";
        AllMenus[0][3, 0].HoverStyle.BackgroundColour = "#FFE0E0A0";
        AllMenus[0][3, 0].HoverStyle.ForegroundColour = "#FF404080";
        AllMenus[0][3, 0].SetActionMode(ActionMode.PerformTask);
        AllMenus[0][3, 0].Action = SendCharactersDefinition.Instance.Create();
        ((SendCharacters)AllMenus[0][3, 0].Action!).ApplicationTargets.Add(new ApplicationMatcherViewModel());
        ((SendCharacters)AllMenus[0][3, 0].Action!).ApplicationTargets[0].UseRegexForExecutable = true;
        ((SendCharacters)AllMenus[0][3, 0].Action!).ApplicationTargets[0].ExecutablePattern = ".*notepad.exe$";
        ((SendCharacters)AllMenus[0][3, 0].Action!).Text = "Hello 🎁";
        AllMenus[0][3, 0].DefaultStyle.FontFamilyName = "Segoe UI Emoji";
        AllMenus[0][3, 0].HoverStyle.FontFamilyName = "Segoe UI Emoji";
        AllMenus[0][3, 0].PressedStyle.FontFamilyName = "Segoe UI Emoji";
        AllMenus[0][3, 0].Text = "🎁";

        AllMenus[1][0, 0].DefaultStyle.BackgroundColour = "#FF0000FF";
        AllMenus[1][0, 1].Text = "Go to Default";
        AllMenus[1][0, 1].TargetMenu = "Default";
        AllMenus[1][0, 1].SetActionMode(ActionMode.SelectMenu);

        AllMenus[2][0, 0].Text = "Go to Alternate";
        AllMenus[2][0, 0].TargetMenu = "Alternate";
        AllMenus[2][0, 0].SetActionMode(ActionMode.SelectMenu);
        AllMenus[2][0, 1].Text = "Go to Default";
        AllMenus[2][0, 1].TargetMenu = "Default";
        AllMenus[2][0, 1].SetActionMode(ActionMode.SelectMenu);

        AllMenus[0][0, 1].Text = "Sequence test";
        AllMenus[0][0, 1].SetActionMode(ActionMode.PerformTask);
        var action2 = (SequenceCommand)SequenceCommandDefinition.Instance.Create();
        AllMenus[0][0, 1].Action = action2;
        action2.Actions.Add(ExecuteCommandDefinition.Instance.Create());

        AllMenus[0][4, 4].Text = "Send Keys Test";
        AllMenus[0][4, 4].SetActionMode(ActionMode.PerformTask);
        var action3 = (SendKeys)SendKeysDefinition.Instance.Create();
        AllMenus[0][4, 4].Action = action3;
        action3.ApplicationTargets.Add(new ApplicationMatcherViewModel() { ExecutablePattern = ".*notepad.exe$", UseRegexForExecutable = true });
        action3.KeySequence.Add(new SendKeyValue() { IsSpecialKey = true, SpecialKey = "HOME" });
        action3.KeySequence.Add(new SendKeyValue() { IsSpecialKey = true, SpecialKey = "END", Modifiers = SendKeyModifierFlags.LeftShift });


        AllMenus[3].MenuSelectors.Add(new ApplicationMatcherViewModel
        {
            ExecutablePattern = @"C:\Windows\notepad.exe"
        });
        AllMenus[3].MenuSelectors.Add(new ApplicationMatcherViewModel
        {
            ExecutablePattern = @"C:\Windows\system32\notepad.exe"
        });
        AllMenus[3][0, 0].Text = "Switch to devenv";
        AllMenus[3][0, 0].SetActionMode(ActionMode.PerformTask);
        AllMenus[3][0, 0].Action = SwitchToApplicationDefinition.Instance.Create();
        ((SwitchToApplication)AllMenus[3][0, 0].Action!).ApplicationTargets.Add(new ApplicationMatcherViewModel());
        ((SwitchToApplication)AllMenus[3][0, 0].Action!).ApplicationTargets[0].UseRegexForExecutable = true;
        ((SwitchToApplication)AllMenus[3][0, 0].Action!).ApplicationTargets[0].ExecutablePattern = ".*devenv.exe$";

        AllMenus[3][0, 1].Text = "Default menu";
        AllMenus[3][0, 1].SetActionMode(ActionMode.SelectMenu);
        AllMenus[3][0, 1].TargetMenu = "Default";
    }

    public string ApplicationWindowTitle
    {
        get { return this.appWindowTitle; }
        set
        {
            this.appWindowTitle = value;
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(ApplicationWindowTitle)));
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
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(ApplicationProcessName)));
            }
        }
    }

    public string ApplicationProcessExecutable
    {
        get { return this.appProcessExecutable; }
        set
        {
            this.appProcessExecutable = value;
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(ApplicationProcessExecutable)));
            }
        }
    }

    public ButtonMenuViewModel? CurrentMenu
    {
        get { return this.currentMenu; }
        set
        {
            this.currentMenu = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentMenu)));
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
        };

        CurrentMenu = this.AllMenus[0];
    }

    internal void SelectMenuFromApp()
    {
        if (CurrentMenu?.MenuSelectors.Any(x => x.Matches(ApplicationWindowTitle, ApplicationProcessExecutable)) ?? false)
            return; // already matches

        if (CurrentMenu != null && LockMenu) return;

        foreach (var menu in this.AllMenus)
        {
            foreach (ApplicationMatcherViewModel selector in menu.MenuSelectors)
            {
                bool matched = selector.Matches(ApplicationWindowTitle, ApplicationProcessExecutable);
                if (matched)
                {
                    CurrentMenu = menu;
                    return;
                }
            }
        }
    }

    internal void RefreshCurrentDesktopState()
    {
        this.applicationHwnd = NativeUtils.GetActiveAppHwnd();
        this.ApplicationWindowTitle = NativeUtils.GetWindowTitle(applicationHwnd);
        this.ApplicationProcessExecutable = NativeUtils.GetWindowProcessMainFilename(applicationHwnd);
        this.ApplicationProcessName = NativeUtils.GetWindowProcessName(applicationHwnd);
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

    public AppViewModel NewFromThis()
    {
        return new AppViewModel()
        {
            ApplicationProcessName = ApplicationProcessName ?? String.Empty,
            ApplicationProcessExecutable = ApplicationProcessExecutable ?? String.Empty,
            ApplicationWindowTitle = ApplicationWindowTitle ?? String.Empty,
            LockMenu = LockMenu,
        };
    }
    public AppViewModel? LoadFromFile(string path)
    {
        var result = new AppViewModel();
        result.ApplicationProcessName = ApplicationProcessName ?? String.Empty;
        result.ApplicationProcessExecutable = ApplicationProcessExecutable ?? String.Empty;
        result.ApplicationWindowTitle = ApplicationWindowTitle ?? String.Empty;
        result.LockMenu = LockMenu;

        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = File.ReadAllBytes(path);
            var reader = new Utf8JsonReader(data.AsSpan());
            var obj = JsonNode.Parse(ref reader, new JsonNodeOptions() { PropertyNameCaseInsensitive = true }) as JsonObject;
            if (obj == null) throw new Exception("No data found");
            MergeFromJson(obj);
        }
        catch (Exception e)
        {
            MessageBox.Show(Application.Current.MainWindow, $"Error reading '{path}'.\nException: {e.Message}", $"Failed to read configuration", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        return result;
    }
    public void SaveToFile(string path)
    {
        JsonNode json = ToJson();

        using var fs = new FileStream(path, FileMode.Create);
        using var writer = new Utf8JsonWriter(fs, new JsonWriterOptions() { Indented = true });
        json.WriteTo(writer);
        writer.Flush();
    }


    public JsonNode ToJson()
    {
        var n = new JsonObject();
        n.AddLowerCamel("menus", AllMenus.ToJson());
        return n;
    }

    private void MergeFromJson(JsonObject root)
    {
        throw new NotImplementedException();
    }
}


