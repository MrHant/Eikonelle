using System.Text.Json;

namespace Eikonelle;

/// <summary>Stores the user's screenshot hotkey between application runs.</summary>
public sealed class SettingsStore(string path)
{
    public Hotkey Load()
    {
        if (!File.Exists(path))
        {
            return Hotkey.Capture;
        }

        return JsonSerializer.Deserialize<Hotkey>(File.ReadAllText(path));
    }

    public void Save(Hotkey hotkey)
    {
        string fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        string temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(hotkey));
            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
