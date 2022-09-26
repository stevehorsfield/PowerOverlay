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

public enum WinAppCommand
{
    BASS_BOOST = 20, // Toggle the bass boost on and off.
    BASS_DOWN = 19, // Decrease the bass.
    BASS_UP = 21, // Increase the bass.
    BROWSER_BACKWARD = 1, // Navigate backward.
    BROWSER_FAVORITES = 6, // Open favorites.
    BROWSER_FORWARD = 2, // Navigate forward.
    BROWSER_HOME = 7, // Navigate home.
    BROWSER_REFRESH = 3, // Refresh page.
    BROWSER_SEARCH = 5, // Open search.
    BROWSER_STOP = 4, // Stop download.
    CLOSE = 31, // Close the window (not the application).
    COPY = 36, // Copy the selection.
    CORRECTION_LIST = 45, // Brings up the correction list when a word is incorrectly identified during speech input.
    CUT = 37, // Cut the selection.
    DICTATE_OR_COMMAND_CONTROL_TOGGLE = 43, // Toggles between two modes of speech input: dictation and command/control (giving commands to an application or accessing menus).
    FIND = 28, // Open the Find dialog.
    FORWARD_MAIL = 40, // Forward a mail message.
    HELP = 27, // Open the Help dialog.
    LAUNCH_APP1 = 17, // Start App1.
    LAUNCH_APP2 = 18, // Start App2.
    LAUNCH_MAIL = 15, // Open mail.
    LAUNCH_MEDIA_SELECT = 16, // Go to Media Select mode.
    MEDIA_CHANNEL_DOWN = 52, // Decrement the channel value, for example, for a TV or radio tuner.
    MEDIA_CHANNEL_UP = 51, // Increment the channel value, for example, for a TV or radio tuner.
    MEDIA_FAST_FORWARD = 49, // Increase the speed of stream playback. This can be implemented in many ways, for example, using a fixed speed or toggling through a series of increasing speeds.
    MEDIA_NEXTTRACK = 11, // Go to next track.
    MEDIA_PAUSE = 47, // Pause.If already paused, take no further action.This is a direct PAUSE command that has no state.If there are discrete Play and Pause buttons, applications should take action on this command as well as APPCOMMAND_MEDIA_PLAY_PAUSE.
    MEDIA_PLAY = 46, // Begin playing at the current position.If already paused, it will resume.This is a direct PLAY command that has no state.If there are discrete Play and Pause buttons, applications should take action on this command as well as APPCOMMAND_MEDIA_PLAY_PAUSE.
    MEDIA_PLAY_PAUSE = 14, // Play or pause playback.If there are discrete Play and Pause buttons, applications should take action on this command as well as APPCOMMAND_MEDIA_PLAY and APPCOMMAND_MEDIA_PAUSE.
    MEDIA_PREVIOUSTRACK = 12, // Go to previous track.
    MEDIA_RECORD = 48, // Begin recording the current stream.
    MEDIA_REWIND = 50, // Go backward in a stream at a higher rate of speed.This can be implemented in many ways, for example, using a fixed speed or toggling through a series of increasing speeds.
    MEDIA_STOP = 13, // Stop playback.
    MIC_ON_OFF_TOGGLE = 44, // Toggle the microphone.
    MICROPHONE_VOLUME_DOWN = 25, // Decrease microphone volume.
    MICROPHONE_VOLUME_MUTE = 24, // Mute the microphone.
    MICROPHONE_VOLUME_UP = 26, // Increase microphone volume.
    NEW = 29, // Create a new window.
    OPEN = 30, // Open a window.
    PASTE = 38, // Paste
    PRINT = 33, // Print current document.
    REDO = 35, // Redo last action.
    REPLY_TO_MAIL = 39, // Reply to a mail message.
    SAVE = 32, // Save current document.
    SEND_MAIL = 41, // Send a mail message.
    SPELL_CHECK = 42, // Initiate a spell check.
    TREBLE_DOWN = 22, // Decrease the treble.
    TREBLE_UP = 23, // Increase the treble.
    UNDO = 34, // Undo last action.
    VOLUME_DOWN = 9, // Lower the volume.
    VOLUME_MUTE = 8, // Mute the volume.
    VOLUME_UP = 10, // Raise the volume.
}

public class SendAppCommand : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return SendAppCommandDefinition.Instance; } }

    private readonly Lazy<FrameworkElement> configElement = new(SendAppCommandDefinition.Instance.CreateConfigElement);
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

    private readonly Dictionary<string,WinAppCommand> appCommandMappings = 
        Enum.GetValues<WinAppCommand>().ToDictionary(getCommandName, c => c);

    private static string getCommandName(WinAppCommand c)
    {
        return c switch
        {
            WinAppCommand.CLOSE => "Close",
            WinAppCommand.OPEN => "Open",
            WinAppCommand.NEW => "New",
            WinAppCommand.SAVE => "Save",
            WinAppCommand.LAUNCH_APP1 => "Launch App 1",
            WinAppCommand.LAUNCH_APP2 => "Launch App 2",
            WinAppCommand.BASS_DOWN => "Bass down",
            WinAppCommand.BASS_UP => "Bass up",
            WinAppCommand.BASS_BOOST => "Bass boost",
            WinAppCommand.BROWSER_BACKWARD => "Browser back",
            WinAppCommand.BROWSER_FORWARD => "Browser forward",
            WinAppCommand.BROWSER_FAVORITES => "Browser favourites",
            WinAppCommand.BROWSER_HOME => "Browser home",
            WinAppCommand.BROWSER_REFRESH => "Browser refresh",
            WinAppCommand.BROWSER_SEARCH => "Browser search",
            WinAppCommand.BROWSER_STOP => "Browser stop",
            WinAppCommand.COPY => "Copy",
            WinAppCommand.CUT => "Cut",
            WinAppCommand.PASTE => "Paste",
            WinAppCommand.UNDO => "Undo",
            WinAppCommand.REDO => "Redo",
            WinAppCommand.CORRECTION_LIST => "Show dictation correction list",
            WinAppCommand.DICTATE_OR_COMMAND_CONTROL_TOGGLE => "Toggle dictation/command speech input mode",
            WinAppCommand.FIND => "Open find dialog",
            WinAppCommand.FORWARD_MAIL => "Forward mail message",
            WinAppCommand.HELP => "Open help dialog",
            WinAppCommand.LAUNCH_MAIL => "Open mail app",
            WinAppCommand.LAUNCH_MEDIA_SELECT => "Go to media selection mode",
            WinAppCommand.MEDIA_CHANNEL_DOWN => "Decrement media channel (radio/TV/etc.)",
            WinAppCommand.MEDIA_CHANNEL_UP => "Increment media channel (radio/TV/etc.)",
            WinAppCommand.MEDIA_FAST_FORWARD => "Increase stream playback speed",
            WinAppCommand.MEDIA_NEXTTRACK => "Go to next track",
            WinAppCommand.MEDIA_PAUSE => "Pause playback",
            WinAppCommand.MEDIA_PLAY => "Start/resume playback",
            WinAppCommand.MEDIA_PLAY_PAUSE => "Toggle pause",
            WinAppCommand.MEDIA_PREVIOUSTRACK => "Go to previous track",
            WinAppCommand.MEDIA_RECORD => "Begin recording the current stream",
            WinAppCommand.MEDIA_REWIND => "Go backward in a stream at a higher speed",
            WinAppCommand.MEDIA_STOP => "Stop playback",
            WinAppCommand.MIC_ON_OFF_TOGGLE => "Toggle microphone",
            WinAppCommand.MICROPHONE_VOLUME_DOWN => "Decrease microphone gain",
            WinAppCommand.MICROPHONE_VOLUME_UP => "Increase microphone gain",
            WinAppCommand.MICROPHONE_VOLUME_MUTE => "Mute microphone",
            WinAppCommand.PRINT => "Print current document",
            WinAppCommand.REPLY_TO_MAIL => "Reply to a mail message",
            WinAppCommand.SEND_MAIL => "Send a mail message",
            WinAppCommand.SPELL_CHECK => "Initiate a spell check",
            WinAppCommand.TREBLE_DOWN => "Treble down",
            WinAppCommand.TREBLE_UP => "Treble up",
            WinAppCommand.VOLUME_DOWN => "Lower output volume",
            WinAppCommand.VOLUME_UP => "Increase output volume",
            WinAppCommand.VOLUME_MUTE => "Mute output",
            _ => Enum.GetName(c)!
        };
    }

    public IEnumerable<string> CommandItems =>Enum.GetValues<WinAppCommand>().Select(getCommandName);

    private WinAppCommand command = WinAppCommand.BROWSER_BACKWARD; // avoid invalid case
    public WinAppCommand Command
    {
        get
        {
            return command;
        }
        set
        {
            command = value;
            RaisePropertyChanged(nameof(Command));
            RaisePropertyChanged(nameof(CommandName));
            RaisePropertyChanged(nameof(CommandItemIndex));
        }
    }
    public string CommandName
    {
        get {
            return getCommandName(Command);
        }
        set
        {
            WinAppCommand result;
            if (appCommandMappings.TryGetValue(value, out result))
            {
                Command = result;
            }
        }
    }

    public int CommandItemIndex
    {
        get
        {
            var items = CommandItems.ToList();
            return items.IndexOf(CommandName);
        }
        set
        {
            var items = CommandItems.ToList();
            if (value < 0 || value >= items.Count)
            {
                Command = WinAppCommand.BROWSER_BACKWARD;
            }
            else
            {
                CommandName = items[value];
            }
        }
    }

    public IEnumerable<string> SourceItems => Enum.GetNames<NativeUtils.WmAppCommandSource>();

    private NativeUtils.WmAppCommandSource source = NativeUtils.WmAppCommandSource.Key;
    public NativeUtils.WmAppCommandSource Source
    {
        get { return source; }
        set
        {
            source = value;
            RaisePropertyChanged(nameof(Source));
            RaisePropertyChanged(nameof(SourceName));
            RaisePropertyChanged(nameof(SourceItemIndex));
        }
    }

    public string SourceName
    {
        get
        {
            return source.ToString();
        }
        set
        {
            Source = Enum.Parse<NativeUtils.WmAppCommandSource>(value, true);
        }
    }

    public int SourceItemIndex
    {
        get
        {
            var items = SourceItems.ToList();
            return items.IndexOf(SourceName);
        }
        set
        {
            var items = SourceItems.ToList();
            if (value < 0 || value >= items.Count)
            {
                Source = NativeUtils.WmAppCommandSource.Key;
            }
            else
            {
                SourceName = items[value];
            }
        }
    }

    private NativeUtils.WmAppCommandModifiers modifiers;
    public NativeUtils.WmAppCommandModifiers Modifiers
    {
        get { return modifiers; }
        set
        {
            modifiers = value;
            RaisePropertyChanged(nameof(Modifiers));
        }
    }

    private void UpdateModifier(bool isSet, NativeUtils.WmAppCommandModifiers mask)
    {
        var val = modifiers;
        val = val & (~mask);
        if (isSet) val |= mask;
        Modifiers = val;
    }

    public bool HasModififierShift
    {
        get { return Modifiers.HasFlag(NativeUtils.WmAppCommandModifiers.MK_SHIFT); }
        set { UpdateModifier(value, NativeUtils.WmAppCommandModifiers.MK_SHIFT); }
    }
    public bool HasModififierControl
    {
        get { return Modifiers.HasFlag(NativeUtils.WmAppCommandModifiers.MK_CONTROL); }
        set { UpdateModifier(value, NativeUtils.WmAppCommandModifiers.MK_CONTROL); }
    }
    public bool HasModififierMouseLeftMutton
    {
        get { return Modifiers.HasFlag(NativeUtils.WmAppCommandModifiers.MK_LBUTTON); }
        set { UpdateModifier(value, NativeUtils.WmAppCommandModifiers.MK_LBUTTON); }
    }
    public bool HasModififierMouseMiddleButton
    {
        get { return Modifiers.HasFlag(NativeUtils.WmAppCommandModifiers.MK_MBUTTON); }
        set { UpdateModifier(value, NativeUtils.WmAppCommandModifiers.MK_MBUTTON); }
    }
    public bool HasModififierMouseRightButton
    {
        get { return Modifiers.HasFlag(NativeUtils.WmAppCommandModifiers.MK_RBUTTON); }
        set { UpdateModifier(value, NativeUtils.WmAppCommandModifiers.MK_RBUTTON); }
    }
    public bool HasModififierMouseX1
    {
        get { return Modifiers.HasFlag(NativeUtils.WmAppCommandModifiers.MK_XBUTTON1); }
        set { UpdateModifier(value, NativeUtils.WmAppCommandModifiers.MK_XBUTTON1); }
    }
    public bool HasModififierMouseX2
    {
        get { return Modifiers.HasFlag(NativeUtils.WmAppCommandModifiers.MK_XBUTTON2); }
        set { UpdateModifier(value, NativeUtils.WmAppCommandModifiers.MK_XBUTTON2); }
    }

    private int repeatCount = 1;
    public int RepeatCount
    {
        get { return repeatCount; }
        set
        {
            repeatCount = value;
            if (value <= 0) repeatCount = 1;
            if (value > 1000) repeatCount = 1000;
            RaisePropertyChanged(nameof(RepeatCount));
        }
    }

    public override bool CanExecute(object? parameter)
    {
        return 
            (appTargets.Count > 0)
            || sendToDesktop || sendToShell || sendToActiveApplication
            ;
    }

    public override ActionCommand Clone()
    {
        var result = new SendAppCommand()
        {
            SendToAllMatches = SendToAllMatches,
            SendToActiveApplication = SendToActiveApplication,
            SendToDesktop = SendToDesktop,
            SendToShell = SendToShell,
            Command = Command,
            Source = Source,
            Modifiers = Modifiers,
            RepeatCount = RepeatCount,
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

        foreach (var target in uniqueTargets)
        {
            DebugLog.Log($"Sending WM_APPCOMMAND '{getCommandName(Command)}' to window 0x{target.ToString("X16")} (repeat: {RepeatCount})");
            for (int i = 0; i < RepeatCount; ++i)
            {
                NativeUtils.SendWmAppCommand(target, (int)Command, Source, Modifiers);
            }
        }

        return Task.CompletedTask;
    }

    public override void WriteJson(JsonObject o)
    {
        if (ApplicationTargets.Count > 0)
        {
            o.AddLowerCamel(nameof(ApplicationTargets), ApplicationTargets.ToJson());
        }
        o.AddLowerCamel(nameof(SendToActiveApplication), JsonValue.Create(SendToActiveApplication));
        o.AddLowerCamel(nameof(SendToDesktop), JsonValue.Create(SendToDesktop));
        o.AddLowerCamel(nameof(SendToShell), JsonValue.Create(SendToShell));
        o.AddLowerCamel(nameof(SendToAllMatches), JsonValue.Create(SendToAllMatches));
        o.AddLowerCamelValue(nameof(Command), CommandName);
        o.AddLowerCamelValue(nameof(Source), SourceName);
        o.AddLowerCamelValue(nameof(RepeatCount), RepeatCount);

        var flags = new List<JsonValue>();
        foreach (var flag in Enum.GetValues<NativeUtils.WmAppCommandModifiers>())
        {
            if (flag == 0) continue;
            if (Modifiers.HasFlag(flag))
                flags.Add(JsonValue.Create(Enum.GetName(flag)!.ToLowerCamelCase())!);
        }
        if (flags.Count > 0) o.AddLowerCamel(nameof(SendAppCommand.Modifiers), new JsonArray(flags.ToArray()));
    }

    public static SendAppCommand CreateFromJson(JsonObject o)
    {
        var result = new SendAppCommand();

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
        o.TryGet<string>(nameof(Command), s => result.CommandName = s);
        o.TryGet<string>(nameof(Source), s => result.SourceName = s);
        o.TryGetValue<int>(nameof(RepeatCount), d => result.RepeatCount = d);

        o.TryGet<JsonArray>(nameof(SendAppCommand.Modifiers), ms =>
        {
            NativeUtils.WmAppCommandModifiers flags = new NativeUtils.WmAppCommandModifiers();
            foreach (var m in ms)
            {
                var f = Enum.Parse<NativeUtils.WmAppCommandModifiers>(m.GetValue<string>(), true);
                flags |= f;
            }
            result.Modifiers = flags;
        });

        return result;
    }
}

public class SendAppCommandDefinition : ActionCommandDefinition
{
    public static SendAppCommandDefinition Instance = new();

    public override string ActionName => "SendAppCommand";
    public override string ActionDisplayName => "Send WM__APPCOMMAND";
    public override string ActionShortName => "App Command";
    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/SendAppCommand.png"));

    public override ActionCommand Create()
    {
        return new SendAppCommand();
    }

    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return SendAppCommand.CreateFromJson(o);
    }

    public override FrameworkElement CreateConfigElement()
    {
        return new SendAppCommandConfigControl();
    }
}