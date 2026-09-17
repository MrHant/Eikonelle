using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eikonelle;

/// <summary>
/// The settings as they are kept between application runs. A stored file that predates
/// a setting leaves it absent, which reads back as that setting's default.
/// </summary>
public readonly record struct StoredSettings(
    Hotkey Hotkey,
    CaptureMode CaptureMode,
    UiMode UiMode = UiMode.System)
{
    /// <summary>The settings used when none are configured.</summary>
    public static StoredSettings Default { get; } =
        new(Hotkey.Capture, CaptureMode.FullScreen, UiMode.System);
}

/// <summary>Stores the user's settings between application runs.</summary>
public sealed class SettingsStore(string path)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public StoredSettings Load()
    {
        if (!File.Exists(path))
        {
            return StoredSettings.Default;
        }

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        if (document.RootElement.ValueKind == JsonValueKind.Object &&
            !document.RootElement.TryGetProperty(nameof(StoredSettings.Hotkey), out _))
        {
            // Files written before capture modes existed hold only the hotkey.
            return StoredSettings.Default with { Hotkey = document.Deserialize<Hotkey>(Options) };
        }

        return document.Deserialize<StoredSettings>(Options);
    }

    public void Save(StoredSettings settings)
    {
        string fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        string temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, Options));
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
