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
    UiMode UiMode = UiMode.System,
    string SaveFolder = "")
{
    /// <summary>The save folder; an absent one reads back as <see cref="ScreenshotFolder.Default"/>.</summary>
    public string SaveFolder { get; init; } =
        string.IsNullOrEmpty(SaveFolder) ? ScreenshotFolder.Default : SaveFolder;

    /// <summary>The settings used when none are configured.</summary>
    public static StoredSettings Default { get; } =
        new(Hotkey.Capture, CaptureMode.FullScreen, UiMode.System, ScreenshotFolder.Default);
}

/// <summary>Stores the user's settings between application runs.</summary>
public sealed class SettingsStore(string path)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public StoredSettings Load() => Load(out _);

    /// <summary>
    /// Load the stored settings. Each setting missing from the file takes its default; each
    /// invalid one takes its default too and is described in <paramref name="problems"/>.
    /// Any error while loading leaves all settings at their defaults and is described too.
    /// </summary>
    public StoredSettings Load(out IReadOnlyList<string> problems)
    {
        var found = new List<string>();
        problems = found;
        try
        {
            return Load(found);
        }
        catch (Exception error)
        {
            found.Clear();
            found.Add("Settings could not be loaded. The defaults will be used. " + error.Message);
            return StoredSettings.Default;
        }
    }

    private StoredSettings Load(List<string> found)
    {
        if (!File.Exists(path))
        {
            return StoredSettings.Default;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(File.ReadAllText(path));
        }
        catch (JsonException)
        {
            found.Add("The settings file is not valid. The defaults will be used.");
            return StoredSettings.Default;
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                found.Add("The settings file is not valid. The defaults will be used.");
                return StoredSettings.Default;
            }

            StoredSettings defaults = StoredSettings.Default;
            if (root.TryGetProperty(nameof(Hotkey.VirtualKey), out _))
            {
                // Files written before capture modes existed hold only the hotkey.
                return defaults with
                {
                    Hotkey = Read(root, Settings.IsValid, defaults.Hotkey, found,
                        "The saved screenshot hotkey is invalid. The default Ctrl+Shift+S will be used."),
                };
            }

            return new StoredSettings(
                ReadProperty(root, nameof(StoredSettings.Hotkey), Settings.IsValid, defaults.Hotkey, found,
                    "The saved screenshot hotkey is invalid. The default Ctrl+Shift+S will be used."),
                ReadProperty(root, nameof(StoredSettings.CaptureMode), Settings.IsValid, defaults.CaptureMode, found,
                    "The saved capture mode is invalid. The default Full Screen will be used."),
                ReadProperty(root, nameof(StoredSettings.UiMode), Settings.IsValid, defaults.UiMode, found,
                    "The saved UI mode is invalid. The default System will be used."),
                ReadProperty<string?>(root, nameof(StoredSettings.SaveFolder), ScreenshotFolder.IsValid, defaults.SaveFolder, found,
                    $"The saved save folder is invalid. The default {defaults.SaveFolder} will be used.")!);
        }
    }

    private static T ReadProperty<T>(
        JsonElement root, string name, Func<T, bool> isValid, T fallback, List<string> problems, string problem) =>
        root.TryGetProperty(name, out JsonElement value)
            ? Read(value, isValid, fallback, problems, problem)
            : fallback;

    private static T Read<T>(JsonElement value, Func<T, bool> isValid, T fallback, List<string> problems, string problem)
    {
        try
        {
            if (value.Deserialize<T>(Options) is { } read && isValid(read))
            {
                return read;
            }
        }
        catch (JsonException)
        {
        }

        problems.Add(problem);
        return fallback;
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
