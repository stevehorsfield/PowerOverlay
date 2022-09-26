using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PowerOverlay.Commands;


public enum CloseMode
{
    SendCloseMessage,
    SendQuitMessage,
}

public class CloseCommand : ActionCommand
{

    public override ActionCommandDefinition Definition { get { return CloseCommandDefinition.Instance; } }

    private readonly Lazy<FrameworkElement> configElement = new(CloseCommandDefinition.Instance.CreateConfigElement);
    public override FrameworkElement ConfigElement => configElement.Value;

    private ObservableCollection<ApplicationMatcherViewModel> appTargets = new();
    public ObservableCollection<ApplicationMatcherViewModel> ApplicationTargets => appTargets;

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

    private CloseMode closeMode;
    public CloseMode CloseMode
    {
        get { return closeMode; }
        set
        {
            closeMode = value;
            RaisePropertyChanged(nameof(CloseMode));
            RaisePropertyChanged(nameof(IsSendCloseMessageCheckboxBehaviour));
            RaisePropertyChanged(nameof(IsSendQuitMessageCheckboxBehaviour));
        }
    }
    public bool IsSendCloseMessageCheckboxBehaviour
    {
        get { return closeMode == CloseMode.SendCloseMessage; }
        set
        {
            if (value) CloseMode = CloseMode.SendCloseMessage;
        }
    }
    public bool IsSendQuitMessageCheckboxBehaviour
    {
        get { return closeMode == CloseMode.SendQuitMessage; }
        set
        {
            if (value) CloseMode = CloseMode.SendQuitMessage;
        }
    }


    public override bool CanExecute(object? parameter)
    {
        return (appTargets.Count > 0) || sendToActiveApplication;
    }

    public override ActionCommand Clone()
    {
        var result = new CloseCommand()
        {
            SendToAllMatches = SendToAllMatches,
            SendToActiveApplication = SendToActiveApplication,
            CloseMode = CloseMode,
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

        var targets = new List<IntPtr>();

        if (this.SendToActiveApplication)
        {
            targets.Add(active);
        }

        foreach (var hwnd in ApplicationTargets.EnumerateMatchedWindows(false, true))
        {
            targets.Add(hwnd);
            if (!SendToAllMatches) break;
        }
        var uniqueTargets = targets.Where(x => x != IntPtr.Zero).Distinct().ToList();

        foreach (var target in uniqueTargets)
        {
            switch (CloseMode)
            {
                case CloseMode.SendCloseMessage:
                    DebugLog.Log($"Sending WM_CLOSE to window 0x{target.ToString("X16")}");
                    NativeUtils.SendCloseMessage(target);
                    break;
                case CloseMode.SendQuitMessage:
                    DebugLog.Log($"Sending WM_QUIT to thread associated with window 0x{target.ToString("X16")}");
                    NativeUtils.SendQuitMessage(target);
                    break;
            }
        }

        return Task.CompletedTask;
    }

    public override void WriteJson(JsonObject o)
    {
        o.AddLowerCamelValue(nameof(CloseMode), CloseMode.ToString());
        if (ApplicationTargets.Count > 0)
        {
            o.AddLowerCamel(nameof(ApplicationTargets), ApplicationTargets.ToJson());
        }
        o.AddLowerCamel(nameof(SendToActiveApplication), JsonValue.Create(SendToActiveApplication));
        o.AddLowerCamel(nameof(SendToAllMatches), JsonValue.Create(SendToAllMatches));
    }

    public static CloseCommand CreateFromJson(JsonObject o)
    {
        var result = new CloseCommand();

        o.TryGet<string>(nameof(CloseMode), n => result.CloseMode = Enum.Parse<CloseMode>(n, true));
        o.TryGet<JsonArray>(nameof(ApplicationTargets), xs => {
            foreach (var x in xs)
            {
                result.ApplicationTargets.Add(
                    ApplicationMatcherViewModel.FromJson(x!)
                );
            }
        });

        o.TryGetValue<bool>(nameof(SendToActiveApplication), b => result.SendToActiveApplication = b);
        o.TryGetValue<bool>(nameof(SendToAllMatches), b => result.SendToAllMatches = b);

        return result;
    }
}

public class CloseCommandDefinition : ActionCommandDefinition
{
    public static CloseCommandDefinition Instance = new();

    public override string ActionName => "Close";
    public override string ActionDisplayName => "Close window or application";
    public override string ActionShortName => "Close window";

    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/CloseCommand.png"));

    public override ActionCommand Create()
    {
        return new CloseCommand();
    }
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return CloseCommand.CreateFromJson(o);
    }


    public override FrameworkElement CreateConfigElement()
    {
        return new CloseCommandConfigControl();
    }
}
