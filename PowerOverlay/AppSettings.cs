using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;

namespace PowerOverlay;

public class AppSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void Notify(params string[] names)
    {
        if (PropertyChanged == null) return;
        foreach (var n in names) PropertyChanged!.Invoke(this, new PropertyChangedEventArgs(n));
    }

    private const string OwnerName = "SteveHorsfield";
    private const string ApplicationName = "PowerOverlay";
    private const string SettingsFileName = "settings.json";
    private const string CacheFileName = "current.overlayconfig.json";

    public const int DefaultMainWindowWidth = 800;
    public const int DefaultMainWindowHeight = 450;

    private static readonly string SettingsFilePath = String.Empty;
    private static readonly string CacheFilePath = String.Empty;

    private string fileAccessPath = String.Empty;
    public string FileAccessPath {
        get
        {
            return fileAccessPath;
        }
        set
        {
            fileAccessPath = value;
            Save();
            Notify(nameof(FileAccessPath));
        }
    }

    private int mainWindowWidth, mainWindowHeight;
    public int MainWindowWidth
    {
        get
        {
            return mainWindowWidth;
        }
        set
        {
            mainWindowWidth = value;
            Save();
            Notify(nameof(MainWindowWidth));
        }
    }
    public int MainWindowHeight
    {
        get { return mainWindowHeight; }
        set
        {
            mainWindowHeight = value;
            Save();
            Notify(nameof(MainWindowHeight));
        }
    }

    private double displayZoom;
    public double DisplayZoom
    {
        get
        {
            return displayZoom;
        }
        set
        {
            if (value < 0.25) throw new ArgumentException("Zoom must be between 0.25 and 4.0");
            if (value > 4) throw new ArgumentException("Zoom must be between 0.25 and 4.0");
            displayZoom = value;
            Save();
            Notify(nameof(DisplayZoom), nameof(DisplayZoomLogValue));
        }
    }

    public double DisplayZoomLogValue
    {
        get
        {
            return Math.Log2(DisplayZoom);
        }
        set
        {
            DisplayZoom = Math.Pow(2.0, value);
        }
    }

    private int? showOnScreenNumber;

    public int? ShowOnScreenNumber
    {
        get { return showOnScreenNumber; }
        set {
            showOnScreenNumber = value;
            Save();
            Notify(
                nameof(ShowOnScreenNumber),
                nameof(ShowOnCursorScreen),
                nameof(ShowOnScreen0), 
                nameof(ShowOnScreen1), 
                nameof(ShowOnScreen2), 
                nameof(ShowOnScreen3), 
                nameof(ShowOnScreen4));
        }
    }
    public bool ShowOnCursorScreen
    {
        get { return !showOnScreenNumber.HasValue; }
        set { if (value) ShowOnScreenNumber = null; }
    }

    public bool ShowOnScreen0
    {
        get { return (showOnScreenNumber ?? -1) == 0; }
        set { if (value) ShowOnScreenNumber = 0; }
    }

    public bool ShowOnScreen1
    {
        get { return (showOnScreenNumber ?? -1) == 1; }
        set { if (value) ShowOnScreenNumber = 1; }
    }

    public bool ShowOnScreen2
    {
        get { return (showOnScreenNumber ?? -1) == 2; }
        set { if (value) ShowOnScreenNumber = 2; }
    }
    public bool ShowOnScreen3
    {
        get { return (showOnScreenNumber ?? -1) == 3; }
        set { if (value) ShowOnScreenNumber = 3; }
    }
    public bool ShowOnScreen4
    {
        get { return (showOnScreenNumber ?? -1) == 4; }
        set { if (value) ShowOnScreenNumber = 4; }
    }

    private int sizeToScreenPercent;
    public int SizeToScreenPercent
    {
        get { return sizeToScreenPercent; }
        set
        {
            if (value < 0) sizeToScreenPercent = 0;
            else if (value > 100) sizeToScreenPercent = 100;
            else sizeToScreenPercent = value;
            
        }
    }

    private AppViewModel appViewModel = new();
    public AppViewModel AppViewModel {
        get {
            return appViewModel;
        }
        set
        {
            appViewModel = value;
            Save();
            Notify(nameof(AppViewModel));
        }
    }

    static AppSettings()
    {
        var folder = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), OwnerName, ApplicationName);
        Directory.CreateDirectory(folder);

        SettingsFilePath = Path.Join(folder, SettingsFileName);
        CacheFilePath = Path.Join(folder, CacheFileName);
    }

    private AppSettings()
    {
        // Do not use property accessors as it will force save
        mainWindowWidth = DefaultMainWindowWidth;
        mainWindowHeight = DefaultMainWindowHeight;
        displayZoom = 1.0;
        showOnScreenNumber = null;
        sizeToScreenPercent = 0;
    }

    public void Save()
    {
        // save settings
        var settings = new JsonObject();
        settings.AddLowerCamel(nameof(FileAccessPath), JsonValue.Create<string>(FileAccessPath));
        settings.AddLowerCamelValue(nameof(DisplayZoom), DisplayZoom);
        if (ShowOnScreenNumber.HasValue) settings.AddLowerCamel(nameof(ShowOnScreenNumber), ShowOnScreenNumber.Value);
        settings.AddLowerCamelValue(nameof(SizeToScreenPercent), SizeToScreenPercent);
        
        using var fs = new FileStream(SettingsFilePath, FileMode.Create);
        using var writer = new Utf8JsonWriter(fs, new JsonWriterOptions() { Indented = true });
        settings.WriteTo(writer);
        writer.Flush();

        // save menu data
        AppViewModel.SaveToFile(CacheFilePath);
    }

    private void Load()
    {
        // read settings
        if (File.Exists(SettingsFilePath))
        {
            var data = File.ReadAllBytes(SettingsFilePath);
            var reader = new Utf8JsonReader(data.AsSpan());
            var obj = JsonNode.Parse(ref reader, new JsonNodeOptions() { PropertyNameCaseInsensitive = true }) as JsonObject;

            obj?.TryGet<string>(nameof(FileAccessPath), s => fileAccessPath = s); // do not invoke property method
            obj?.TryGetValue<int>(nameof(MainWindowWidth), w => mainWindowWidth = w); // do not invoke property method
            obj?.TryGetValue<int>(nameof(MainWindowHeight), h => mainWindowHeight = h); // do not invoke property method
            obj?.TryGetValue<double>(nameof(DisplayZoom), d =>
            {
                if (d >= 0.1 && d <= 10.0) displayZoom = d;
            });
            obj?.TryGetValue<int>(nameof(ShowOnScreenNumber), d => showOnScreenNumber = d);
            obj?.TryGetValue<int>(nameof(SizeToScreenPercent), d => sizeToScreenPercent = d);
        }
        // read menu data
        if (File.Exists(CacheFilePath))
        {
            appViewModel = appViewModel.LoadFromFile(CacheFilePath) ?? appViewModel;
        }
    }

    public static AppSettings Get()
    {
        var settings = new AppSettings();
        settings.Load();
        return settings;
    }
}
