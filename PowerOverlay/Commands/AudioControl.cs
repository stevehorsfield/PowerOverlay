using PowerOverlay.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Security.AccessControl;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PowerOverlay.Commands;

public enum AudioKind
{
    Speaker,
    Microphone
}

public enum AudioTargetRole
{
    Console,
    Multimedia,
    Communications
}

public enum MuteSetting
{
    Mute,
    Unmute,
    ToggleMute,
}

public enum VolumeAdjustmentMode
{
    Set,
    Increase,
    Decrease,
}

public class DiscoveredAudioDevice : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string id = String.Empty;
    private AudioKind kind;
    private bool isDefaultForConsoleTarget;
    private bool isDefaultForMultimediaTarget;
    private bool isDefaultForCommunicationsTarget;
    private bool isMuted;
    private int volume;
    private string friendlyName = String.Empty;
    private string deviceName = String.Empty;

    private void RaisePropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string ID
    {
        get { return id; }
        set
        {
            id = value;
            RaisePropertyChanged(nameof(ID));
        }
    }

    public AudioKind Kind
    {
        get { return kind; }
        set
        {
            this.kind = value;
            RaisePropertyChanged(nameof(Kind));
            RaisePropertyChanged(nameof(IsSpeaker));
            RaisePropertyChanged(nameof(IsMicrophone));
        }
    }

    public string MicrophoneOrSpeakerIndicatorText
    {
        get
        {
            if (IsSpeaker) return "Speaker";
            if (IsMicrophone) return "Microphone";
            return String.Empty;
        }
    }
    public bool IsSpeaker
    {
        get { return this.Kind == AudioKind.Speaker; }
        set { this.Kind = value ? AudioKind.Speaker : AudioKind.Microphone; }
    }
    public bool IsMicrophone
    {
        get { return this.Kind == AudioKind.Microphone; }
        set { this.Kind = value ? AudioKind.Microphone : AudioKind.Speaker; }
    }

    public bool IsDefaultForConsole
    {
        get { return isDefaultForConsoleTarget; }
        set
        {
            this.isDefaultForConsoleTarget = value;
            RaisePropertyChanged(nameof(IsDefaultForConsole));
        }
    }
    public bool IsDefaultForMultimedia
    {
        get { return isDefaultForMultimediaTarget; }
        set
        {
            this.isDefaultForMultimediaTarget = value;
            RaisePropertyChanged(nameof(IsDefaultForMultimedia));
        }
    }
    public bool IsDefaultForCommunications
    {
        get { return isDefaultForCommunicationsTarget; }
        set
        {
            this.isDefaultForCommunicationsTarget = value;
            RaisePropertyChanged(nameof(IsDefaultForCommunications));
        }
    }
    public bool IsMuted
    {
        get { return isMuted; }
        set { 
            this.isMuted = value;
            RaisePropertyChanged(nameof(IsMuted));
        }
    }
    public int Volume
    {
        get { return volume; }
        set
        {
            volume = value;
            RaisePropertyChanged(nameof(Volume));
        }
    }

    public string FriendlyName
    {
        get { return friendlyName; }
        set { 
            this.friendlyName = value;
            RaisePropertyChanged(nameof(FriendlyName));
        }
    }

    public string DeviceName
    {
        get { return deviceName; }
        set
        {
            this.deviceName = value;
            RaisePropertyChanged(nameof(DeviceName));
        }
    }
}

public class AudioControl : ActionCommand
{
    public override ActionCommandDefinition Definition { get { return AudioControlDefinition.Instance; } }

    private readonly Lazy<FrameworkElement> configElement = new (AudioControlDefinition.Instance.CreateConfigElement);
    public override FrameworkElement ConfigElement => configElement.Value;

    private bool setMute, setVolume;
    bool useDefaultDevice = true;

    private AudioKind deviceTargetKind = AudioKind.Speaker;
    private AudioTargetRole deviceTargetRole = AudioTargetRole.Console;
    private string deviceEndpointID = String.Empty;
    private VolumeAdjustmentMode volumeAdjustmentMode = VolumeAdjustmentMode.Set;
    private int volume;
    private MuteSetting muteSetting = MuteSetting.ToggleMute;
    private readonly ObservableCollection<DiscoveredAudioDevice> devices = new ObservableCollection<DiscoveredAudioDevice>();
    private DiscoveredAudioDevice? selectedDevice;
    public ObservableCollection<DiscoveredAudioDevice> Devices => devices;

    public bool UseDefaultDevice
    {
        get { return useDefaultDevice; }
        set
        {
            useDefaultDevice = value;
            RaisePropertyChanged(nameof(UseDefaultDevice));
            RaisePropertyChanged(nameof(UseSpecificDevice));
            RaisePropertyChanged(nameof(UseDefaultDeviceCheckboxBehaviour));
            RaisePropertyChanged(nameof(UseSpecificDeviceCheckboxBehaviour));
            UpdateSelectedDevice();
        }
    }

    public bool UseSpecificDevice
    {
        get { return !useDefaultDevice; }
        set
        {
            UseDefaultDevice = !value;
        }
    }

    public bool UseDefaultDeviceCheckboxBehaviour
    {
        get { return UseDefaultDevice; }
        set
        {
            if (value) UseDefaultDevice = value;
        }
    }
    public bool UseSpecificDeviceCheckboxBehaviour
    {
        get { return !UseDefaultDevice; }
        set
        {
            if (value) UseDefaultDevice = !value;
        }
    }

    public string DeviceEndpointID
    {
        get { return deviceEndpointID; }
        set
        {
            deviceEndpointID = value;
            RaisePropertyChanged(nameof(DeviceEndpointID));
            UpdateSelectedDevice();
        }
    }

    public bool SetMute
    {
        get { return setMute; }
        set
        {
            setMute = value;
            RaisePropertyChanged(nameof(SetMute));
        }
    }

    public bool SetVolume
    {
        get { return setVolume; }
        set
        {
            setVolume = value;
            RaisePropertyChanged(nameof(SetVolume));
        }
    }

    public int Volume
    {
        get { return volume; }
        set
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            volume = value;
            RaisePropertyChanged(nameof(Volume));
        }
    }

    public AudioKind DeviceTargetKind
    {
        get { return deviceTargetKind; }
        set
        {
            deviceTargetKind = value;
            RaisePropertyChanged(nameof(DeviceTargetKind));
            RaisePropertyChanged(nameof(IsSpeakerCheckboxBehaviour));
            RaisePropertyChanged(nameof(IsMicrophoneCheckboxBehaviour));
            UpdateSelectedDevice();
        }
    }

    public VolumeAdjustmentMode VolumeAdjustmentMode
    {
        get { return volumeAdjustmentMode; }
        set
        {
            volumeAdjustmentMode = value;
            RaisePropertyChanged(nameof(VolumeAdjustmentMode));
            RaisePropertyChanged(nameof(VolumeAdjustmentModeIsSetCheckboxBehaviour));
            RaisePropertyChanged(nameof(VolumeAdjustmentModeIsIncreaseCheckboxBehaviour));
            RaisePropertyChanged(nameof(VolumeAdjustmentModeIsDecreaseCheckboxBehaviour));
        }
    }

    public bool VolumeAdjustmentModeIsSetCheckboxBehaviour
    {
        get { return VolumeAdjustmentMode == VolumeAdjustmentMode.Set; }
        set
        {
            if (value) VolumeAdjustmentMode = VolumeAdjustmentMode.Set;
        }
    }
    public bool VolumeAdjustmentModeIsIncreaseCheckboxBehaviour
    {
        get { return VolumeAdjustmentMode == VolumeAdjustmentMode.Increase; }
        set
        {
            if (value) VolumeAdjustmentMode = VolumeAdjustmentMode.Increase;
        }
    }
    public bool VolumeAdjustmentModeIsDecreaseCheckboxBehaviour
    {
        get { return VolumeAdjustmentMode == VolumeAdjustmentMode.Decrease; }
        set
        {
            if (value) VolumeAdjustmentMode = VolumeAdjustmentMode.Decrease;
        }
    }

    public bool IsSpeakerCheckboxBehaviour
    {
        get { return deviceTargetKind == AudioKind.Speaker; }
        set
        {
            if (value) DeviceTargetKind = AudioKind.Speaker;
        }
    }

    public bool IsMicrophoneCheckboxBehaviour
    {
        get { return deviceTargetKind == AudioKind.Microphone; }
        set
        {
            if (value) DeviceTargetKind = AudioKind.Microphone;
        }
    }

    public AudioTargetRole DeviceTargetRole
    {
        get { return deviceTargetRole; }
        set
        {
            deviceTargetRole = value;
            RaisePropertyChanged(nameof(DeviceTargetRole));
            RaisePropertyChanged(nameof(IsConsoleCheckboxBehaviour));
            RaisePropertyChanged(nameof(IsMultimediaCheckboxBehaviour));
            RaisePropertyChanged(nameof(IsCommunicationsCheckboxBehaviour));
            UpdateSelectedDevice();
        }
    }

    public bool IsConsoleCheckboxBehaviour
    {
        get { return deviceTargetRole == AudioTargetRole.Console; }
        set
        {
            if (value) DeviceTargetRole = AudioTargetRole.Console;
        }
    }
    public bool IsMultimediaCheckboxBehaviour
    {
        get { return deviceTargetRole == AudioTargetRole.Multimedia; }
        set
        {
            if (value) DeviceTargetRole = AudioTargetRole.Multimedia;
        }
    }
    public bool IsCommunicationsCheckboxBehaviour
    {
        get { return deviceTargetRole == AudioTargetRole.Communications; }
        set
        {
            if (value) DeviceTargetRole = AudioTargetRole.Communications;
        }
    }

    public MuteSetting MuteSetting
    {
        get { return muteSetting; }
        set
        {
            muteSetting = value;
            RaisePropertyChanged(nameof(MuteSetting));
            RaisePropertyChanged(nameof(IsMuteCheckboxBehaviour));
            RaisePropertyChanged(nameof(IsUnmuteCheckboxBehaviour));
            RaisePropertyChanged(nameof(IsToggleMuteCheckboxBehaviour));
        }
    }

    public bool IsMuteCheckboxBehaviour
    {
        get { return MuteSetting == MuteSetting.Mute; }
        set
        {
            if (value) MuteSetting = MuteSetting.Mute;
        }
    }
    public bool IsUnmuteCheckboxBehaviour
    {
        get { return MuteSetting == MuteSetting.Unmute; }
        set
        {
            if (value) MuteSetting = MuteSetting.Unmute;
        }
    }
    public bool IsToggleMuteCheckboxBehaviour
    {
        get { return MuteSetting == MuteSetting.ToggleMute; }
        set
        {
            if (value) MuteSetting = MuteSetting.ToggleMute;
        }
    }

    public DiscoveredAudioDevice? SelectedDevice
    {
        get { return selectedDevice; }
        set
        {
            selectedDevice = value;
            RaisePropertyChanged(nameof(SelectedDevice));
        }
    }

    public override ActionCommand Clone()
    {
        return new AudioControl()
        {
            UseDefaultDevice = UseDefaultDevice,
            DeviceTargetKind = DeviceTargetKind,
            DeviceTargetRole = DeviceTargetRole,
            DeviceEndpointID = DeviceEndpointID,
            SetMute = SetMute,
            SetVolume = SetVolume,
            MuteSetting = MuteSetting,
            Volume = Volume,
            VolumeAdjustmentMode = VolumeAdjustmentMode,
        };
    }

    public override void WriteJson(JsonObject o)
    {
        o.AddLowerCamelValue(nameof(UseDefaultDevice), UseDefaultDevice);
        o.AddLowerCamelValue(nameof(SetMute), SetMute);
        if (SetMute) o.AddLowerCamelValue(nameof(MuteSetting), this.MuteSetting.ToString());
        o.AddLowerCamelValue(nameof(SetVolume), SetVolume);
        if (SetVolume)
        {
            o.AddLowerCamelValue(nameof(Volume), Volume);
            o.AddLowerCamelValue(nameof(VolumeAdjustmentMode), this.VolumeAdjustmentMode.ToString());
        }
        if (UseDefaultDevice)
        {
            o.AddLowerCamelValue(nameof(DeviceTargetKind), DeviceTargetKind.ToString());
            o.AddLowerCamelValue(nameof(DeviceTargetRole), DeviceTargetRole.ToString());
        }
        else
        {
            o.AddLowerCamelValue(nameof(DeviceEndpointID), DeviceEndpointID);
        }
    }

    public static AudioControl CreateFromJson(JsonObject o)
    {
        var result = new AudioControl();
        o.TryGetValue<bool>(nameof(UseDefaultDevice), b => result.UseDefaultDevice = b);
        if (result.UseDefaultDevice)
        {
            o.TryGet<string>(nameof(DeviceTargetKind), s =>
            {
                if (Enum.TryParse<AudioKind>(s, true, out AudioKind k)) result.DeviceTargetKind = k;
                if (Enum.TryParse<AudioTargetRole>(s, true, out AudioTargetRole r)) result.DeviceTargetRole = r;
            });
        }
        else
        {
            o.TryGet<string>(nameof(DeviceEndpointID), s => result.DeviceEndpointID = s);
        }
        o.TryGetValue<bool>(nameof(SetMute), b => result.SetMute = b);
        if (result.SetMute)
        {
            o.TryGet<string>(nameof(MuteSetting), s =>
            {
                if (Enum.TryParse<MuteSetting>(s, true, out MuteSetting v)) result.MuteSetting = v;
            });
        }
        o.TryGetValue<bool>(nameof(SetVolume), b => result.SetVolume = b);
        if (result.SetVolume)
        {
            o.TryGetValue<int>(nameof(Volume), d => result.Volume = d);
            o.TryGet<string>(nameof(VolumeAdjustmentMode), s =>
            {
                if (Enum.TryParse<VolumeAdjustmentMode>(s, true, out VolumeAdjustmentMode m)) result.VolumeAdjustmentMode = m;
            });
        }

        return result;
    }

    public void PopulateDiscoveredDevices()
    {
        Devices.Clear();
        var interop = new PowerOverlay.Interop.MultimediaAudioInterop();
        try
        {
            var endpoints = interop.EnumerateAudioEndpoints();

            foreach (var e in endpoints)
            {
                if (e.IsDisabled || e.IsNotPresent) continue;

                var device = new DiscoveredAudioDevice()
                {
                    ID = e.EndpointID,
                    DeviceName = e.InterfaceFriendlyName,
                    FriendlyName = e.EndpointFriendlyName,
                    Volume = (int)Math.Round(e.NormalizedVolumeLevel * 100.0, MidpointRounding.AwayFromZero),
                    IsMuted = e.IsMuted,
                    IsSpeaker = e.IsAudioOutput, // can only be speaker or microphone, not both
                };
                this.Devices.Add(device);
            }

            var defaults = interop.EnumerateDefaultAudioEndpoints();
            foreach (var d in defaults)
            {
                var match = Devices.FirstOrDefault(e => e.ID.Equals(d.EndpointID, StringComparison.Ordinal));
                if (match != null)
                {
                    switch (d.Role)
                    {
                        case Interop.AudioEndpointRole.Console:
                            match.IsDefaultForConsole = true;
                            break;
                        case Interop.AudioEndpointRole.Multimedia:
                            match.IsDefaultForMultimedia = true;
                            break;
                        case Interop.AudioEndpointRole.Communications:
                            match.IsDefaultForCommunications = true;
                            break;
                    }
                }
            }
        }
        catch (Win32Exception e)
        {
            DebugLog.Log($"Unable to interrogate audio devices: HRESULT {e.ErrorCode.ToString("X8")} - {e.Message}");
        }
        UpdateSelectedDevice();
    }
    private void UpdateSelectedDevice()
    {
        DiscoveredAudioDevice? newValue = null;
        if (UseDefaultDevice)
        {
            newValue = Devices.FirstOrDefault(x =>
                    DeviceTargetRole switch
                    {
                        AudioTargetRole.Multimedia => x.IsDefaultForMultimedia,
                        AudioTargetRole.Console => x.IsDefaultForConsole,
                        AudioTargetRole.Communications => x.IsDefaultForCommunications,
                        _ => false
                    }
                    &&
                    DeviceTargetKind switch
                    {
                        AudioKind.Speaker => x.IsSpeaker,
                        AudioKind.Microphone => x.IsMicrophone,
                        _ => false
                    }

                );
        }
        else
        {
            newValue = Devices.FirstOrDefault(x => x.ID.Equals(DeviceEndpointID, StringComparison.Ordinal));
        }
        SelectedDevice = newValue;
    }

    public override bool CanExecute(object? parameter)
    {
        return (useDefaultDevice || (!String.IsNullOrEmpty(deviceEndpointID)))
            && (setMute || setVolume);
    }

    public override Task ExecuteWithContext(CommandExecutionContext context)
    {
        var interop = new PowerOverlay.Interop.MultimediaAudioInterop();
        var targetID = String.Empty;
        if (UseDefaultDevice)
        {
            DebugLog.Log($"Discovering audio device for {DeviceTargetRole} {DeviceTargetKind}");
            var endpoints = interop.EnumerateDefaultAudioEndpoints();
            targetID = endpoints.FirstOrDefault(x =>
                (DeviceTargetKind switch
                {
                    AudioKind.Speaker => x.Direction == Interop.AudioEndpointDirection.Speaker,
                    AudioKind.Microphone => x.Direction == Interop.AudioEndpointDirection.Microphone,
                    _ => false,
                })
                &&
                (DeviceTargetRole switch
                {
                    AudioTargetRole.Console => x.Role == Interop.AudioEndpointRole.Console,
                    AudioTargetRole.Multimedia => x.Role == Interop.AudioEndpointRole.Multimedia,
                    AudioTargetRole.Communications => x.Role == Interop.AudioEndpointRole.Communications,
                    _ => false,
                }))
                ?.EndpointID ?? String.Empty;
        }
        else
        {
            targetID = DeviceEndpointID;
        }
        DebugLog.Log(
            $"Setting audio properties: device: {targetID}, " +
            (SetMute ? $"mute: {MuteSetting}," : "mute: not changed,") +
            (SetVolume ? $"volume: {VolumeAdjustmentMode} {Volume}" : "volume: not changed"));

        var needsInteropDevice =
            (SetMute && MuteSetting == MuteSetting.ToggleMute)
            ||
            (SetVolume && VolumeAdjustmentMode == VolumeAdjustmentMode.Increase)
            ||
            (SetVolume && VolumeAdjustmentMode == VolumeAdjustmentMode.Decrease);
        var interopDevice = needsInteropDevice ?
            interop.EnumerateAudioEndpoints()
                            .FirstOrDefault(x => x.EndpointID.Equals(targetID, StringComparison.Ordinal))
            : null;

        if (SetVolume)
        {
            switch (VolumeAdjustmentMode)
            {
                case VolumeAdjustmentMode.Set:
                    interop.SetVolume(targetID, ((double)Volume) / 100.0);
                    break;
                case VolumeAdjustmentMode.Increase:
                    if (interopDevice != null)
                    {
                        var targetVolume = interopDevice.NormalizedVolumeLevel + (((double)Volume) / 100.0);
                        if (targetVolume > 1.0) targetVolume = 1.0;
                        interop.SetVolume(targetID, targetVolume);
                    }
                    break;
                case VolumeAdjustmentMode.Decrease:
                    if (interopDevice != null)
                    {
                        var targetVolume = interopDevice.NormalizedVolumeLevel - (((double)Volume) / 100.0);
                        if (targetVolume < 0.0) targetVolume = 0.0;
                        interop.SetVolume(targetID, targetVolume);
                    }
                    break;
            }
        }
        if (SetMute)
        {
            switch (MuteSetting)
            {
                case MuteSetting.Mute:
                    interop.SetMute(targetID, true);
                    break;
                case MuteSetting.Unmute:
                    interop.SetMute(targetID, false);
                    break;
                case MuteSetting.ToggleMute:
                    {
                        if (interopDevice != null)
                        {
                            if (interopDevice.IsActive) // otherwise mute state is unavailable
                            {
                                interop.SetMute(targetID, !interopDevice.IsMuted);
                            }
                        }
                    }
                    break;
            }
        }
        return Task.CompletedTask;
    }
}


public class AudioControlDefinition : ActionCommandDefinition
{
    public static AudioControlDefinition Instance = new();

    public override string ActionName => "AudioControl";
    public override string ActionDisplayName => "Audio controls";

    public override string ActionShortName => "Audio";

    public override ImageSource ActionImage => new BitmapImage(new Uri("pack://application:,,,/Commands/AudioControl.png"));
    public override ActionCommand Create()
    {
        return new AudioControl();
    }
    public override ActionCommand CreateFromJson(JsonObject o)
    {
        return AudioControl.CreateFromJson(o);
    }

    public override FrameworkElement CreateConfigElement()
    {
        return new AudioControlConfigControl();
    }
}

