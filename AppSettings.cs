using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;

namespace PowerOverlay;

public class AppSettings
{
    private const string OwnerName = "SteveHorsfield";
    private const string ApplicationName = "PowerOverlay";
    private const string SettingsFileName = "settings.json";
    private const string CacheFileName = "current.overlayconfig.json";

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

    }

    public void Save()
    {
        // save settings
        var settings = new JsonObject();
        settings.AddLowerCamel(nameof(FileAccessPath), JsonValue.Create<string>(FileAccessPath));
        
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
