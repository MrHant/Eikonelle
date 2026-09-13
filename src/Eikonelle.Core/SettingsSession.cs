namespace Eikonelle;

/// <summary>Pending settings changes and the result of applying them.</summary>
public sealed class SettingsSession
{
    private readonly Settings _settings;
    private readonly Action<Hotkey> _save;

    public SettingsSession(Settings settings, Action<Hotkey> save)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(save);
        _settings = settings;
        _save = save;
        SelectedHotkey = settings.ScreenshotHotkey;
    }

    public Hotkey SelectedHotkey { get; set; }
    public string Message { get; private set; } = "";

    public bool Apply()
    {
        if (!_settings.TryChangeHotkey(SelectedHotkey))
        {
            Message = "This combination is invalid or unavailable. Choose another; your current hotkey is still active.";
            return false;
        }

        try
        {
            _save(_settings.ScreenshotHotkey);
            Message = "Hotkey applied and saved.";
            return true;
        }
        catch (Exception error)
        {
            // Catch broadly at the persistence boundary to report failures through the UI.
            Message = "The hotkey is active for this session, but could not be saved. " + error.Message;
            return false;
        }
    }

    public void Cancel()
    {
        SelectedHotkey = _settings.ScreenshotHotkey;
        Message = "";
    }
}
