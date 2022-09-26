using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PowerOverlay.Commands;

public class SendCharacters : ActionCommand
{

    public override ActionCommandDefinition Definition { get { return SendCharactersDefinition.Instance; } }

    private readonly Lazy<FrameworkElement> configElement = new(SendCharactersDefinition.Instance.CreateConfigElement);
    public override FrameworkElement ConfigElement => configElement.Value;

    private ObservableCollection<ApplicationMatcherViewModel> appTargets = new();
    public ObservableCollection<ApplicationMatcherViewModel> ApplicationTargets => appTargets;

    private bool sendToDesktop;
    public bool SendToDesktop
    {
        get { return sendToDesktop; }
        set
        {
            sendToDesktop = value;
            RaisePropertyChanged(nameof(SendToDesktop));
        }
    }

    private bool sendToShell;
    public bool SendToShell
    {
        get { return sendToShell; }
        set
        {
            sendToShell = value;
            RaisePropertyChanged(nameof(SendToShell));
        }
    }

    private bool sendToActiveApplication;
    public bool SendToActiveApplication
    {
        get { return sendToActiveApplication; }
        set
        {
            sendToActiveApplication = value;
            RaisePropertyChanged(nameof(SendToActiveApplication));
        }
    }

    private string text = String.Empty;
    public string Text
    {
        get { return text; }
        set
        {
            text = value;
            RaisePropertyChanged(nameof(Text));
        }
    }

    private bool sendToAllMatches;
    public bool SendToAllMatches
    {
        get { return sendToAllMatches; }
        set
        {
            sendToAllMatches = value;
            RaisePropertyChanged(nameof(SendToAllMatches));
        }
    }

    private string dateTimeFormatString = String.Empty;
    public string DateTimeFormatString
    {
        get { return dateTimeFormatString; }
        set
        {
            dateTimeFormatString = value;
            RaisePropertyChanged(nameof(DateTimeFormatString));
        }
    }

    private bool isDateTime;
    public bool IsDateTime
    {
        get { return isDateTime; }
        set
        {
            isDateTime = value;
            RaisePropertyChanged(nameof(IsDateTime));
            RaisePropertyChanged(nameof(IsDateTimeCheckboxBehaviour));
            RaisePropertyChanged(nameof(IsFixedStringCheckboxBehaviour));
        }
    }

    private bool isUTC;
    public bool IsUTC
    {
        get { return isUTC; }
        set
        {
            isUTC = value;
            RaisePropertyChanged(nameof(IsUTC));
        }
    }

    public bool IsDateTimeCheckboxBehaviour
    {
        get { return isDateTime; }
        set
        {
            if (value) isDateTime = value;
            RaisePropertyChanged(nameof(IsDateTime));
            RaisePropertyChanged(nameof(IsDateTimeCheckboxBehaviour));
            RaisePropertyChanged(nameof(IsFixedStringCheckboxBehaviour));
        }
    }

    public bool IsFixedStringCheckboxBehaviour
    {
        get { return ! isDateTime; }
        set
        {
            if (value) isDateTime = ! value;
            RaisePropertyChanged(nameof(IsDateTime));
            RaisePropertyChanged(nameof(IsDateTimeCheckboxBehaviour));
            RaisePropertyChanged(nameof(IsFixedStringCheckboxBehaviour));
        }
    }

    public override bool CanExecute(object? parameter)
    {
        return (
            ( ((!IsDateTime) && (!String.IsNullOrEmpty(Text)))
              ||
              (IsDateTime && (!String.IsNullOrEmpty(DateTimeFormatString)))
            ) && (
              (appTargets.Count > 0)
              || sendToDesktop || sendToShell || sendToActiveApplication
            ));
    }

    public override ActionCommand Clone()
    {
        var result = new SendCharacters()
        {
            Text = Text,
            SendToAllMatches = SendToAllMatches,
            SendToActiveApplication = SendToActiveApplication,
            SendToDesktop = SendToDesktop,
            SendToShell = SendToShell,
            DateTimeFormatString = DateTimeFormatString,
            IsUTC = IsUTC,
            IsDateTime = IsDateTime,
        };

        foreach (var x in ApplicationTargets)
        {
            result.ApplicationTargets.Add(x.Clone());
        }
        return result;
    }

    public override Task ExecuteWithContext(CommandExecutionContext context)
    {
        var active = NativeUtils.GetActiveAppHwnd();
        var shell = NativeUtils.GetShellWindow();
        var desktop = NativeUtils.GetDesktopWindow();

        var targets = new List<IntPtr>();

        if (this.SendToActiveApplication)
        {
            targets.Add(active);
        }
        if (this.SendToShell)
        {
            targets.Add(shell);
        }
        if (this.SendToDesktop)
        {
            targets.Add(desktop);
        }

        foreach (var hwnd in ApplicationTargets.EnumerateMatchedWindows(false, true))
        {
            if (hwnd == desktop || hwnd == shell) continue;
            targets.Add(hwnd);
            if (!SendToAllMatches) break;
        }
        var uniqueTargets = targets.Where(x => x != IntPtr.Zero).Distinct().ToList();

        string sourceText;
        if (IsDateTime)
        {
            if (IsUTC)
            {
                sourceText = DateTimeOffset.UtcNow.ToString(DateTimeFormatString);
            }
            else
            {
                sourceText = DateTimeOffset.Now.ToString(DateTimeFormatString);
            }
        }
        else
        {
            sourceText = Text;
        }

        var textUTF16 = MemoryMarshal.Cast<byte, Int16>(Encoding.Unicode.GetBytes(sourceText).AsSpan());

        foreach (var target in uniqueTargets)
        {
            NativeUtils.SendText(target, textUTF16);
        }

        return Task.CompletedTask;
    }

    public override void WriteJson(JsonObject o)
    {
        o.AddLowerCamelValue(nameof(IsDateTime), IsDateTime);
        if (IsDateTime)
        {
            o.AddLowerCamel(nameof(DateTimeFormatString), DateTimeFormatString);
            o.AddLowerCamelValue(nameof(IsUTC), IsUTC);
        } else
        {
            o.AddLowerCamel(nameof(Text), JsonValue.Create(Text));
        }

        if (ApplicationTargets.Count > 0)
        {
            o.AddLowerCamel(nameof(ApplicationTargets), ApplicationTargets.ToJson());
        }
        o.AddLowerCamel(nameof(SendToActiveApplication), JsonValue.Create(SendToActiveApplication));
        o.AddLowerCamel(nameof(SendToDesktop), JsonValue.Create(SendToDesktop));
        o.AddLowerCamel(nameof(SendToShell), JsonValue.Create(SendToShell));
        o.AddLowerCamel(nameof(SendToAllMatches), JsonValue.Create(SendToAllMatches));
    }

    public static SendCharacters CreateFromJson(JsonObject o)
    {
        var result = new SendCharacters();

        o.TryGetValue<bool>(nameof(IsDateTime), b => result.IsDateTime = b);
        o.TryGetValue<bool>(nameof(IsUTC), b => result.IsUTC = b);
        o.TryGet<string>(nameof(DateTimeFormatString), s => result.DateTimeFormatString = s);
        o.TryGet<string>(nameof(Text), s => result.Text = s);
        o.TryGet<JsonArray>(nameof(ApplicationTargets), xs => {
            foreach (var x in xs)
            {
                result.ApplicationTargets.Add(
                    ApplicationMatcherViewModel.FromJson(x!)
                );
            }
        });

        o.TryGetValue<bool>(nameof(SendToActiveApplication), b => result.SendToActiveApplication = b);
        o.TryGetValue<bool>(nameof(SendToDesktop), b => result.SendToDesktop = b);
        o.TryGetValue<bool>(nameof(SendToShell), b => result.SendToShell = b);
        o.TryGetValue<bool>(nameof(SendToAllMatches), b => result.SendToAllMatches = b);
        
        return result;
    }
}

public class SendCharactersDefinition : ActionCommandDefinition
{
    public static SendCharactersDefinition Instance = new();

    public override string ActionName => "SendCharacters";
    public override string ActionDisplayName => "Send text to window";
    public override string ActionShortName => "Text";

    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/SendCharacters.png"));

    public override ActionCommand Create()
    {
        return new SendCharacters();
    }
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return SendCharacters.CreateFromJson(o);
    }


    public override FrameworkElement CreateConfigElement()
    {
        return new SendCharactersConfigControl();
    }
}
