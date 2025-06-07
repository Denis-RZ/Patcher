using System.Text.Json;
using System.IO;

namespace UniversalCodePatcher.Avalonia.Models;

public class AppSettings
{
    public bool ShowHiddenFiles { get; set; }
    public string ThemeVariant { get; set; } = "Default";

    public static AppSettings Load(string file)
    {
        try
        {
            if (File.Exists(file))
            {
                var json = File.ReadAllText(file);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                    return settings;
            }
        }
        catch { }
        return new AppSettings();
    }

    public void Save(string file)
    {
        try
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(file, json);
        }
        catch { }
    }
}
