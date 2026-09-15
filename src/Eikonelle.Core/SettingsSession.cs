namespace Eikonelle;

/// <summary>Pending settings changes and the result of applying them.</summary>
public sealed class SettingsSession
{
    private readonly Settings _settings;
    private readonly Action<StoredSettings> _save;

    public SettingsSession(Settings settings, Action<StoredSettings> save)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(save);
        _settings = settings;
        _save = save;
        SelectedHotkey = settings.ScreenshotHotkey;
        SelectedCaptureMode = settings.CaptureMode;
    }

    public Hotkey SelectedHotkey { get; set; }
    public CaptureMode SelectedCaptureMode { get; set; }
    public string Message { get; private set; } = "";

    public bool Apply()
    {
        if (!Settings.IsValid(SelectedCaptureMode))
        {
            Message = "This capture mode is not supported. Choose another; your current settings are still active.";
            return false;
        }

        if (!_settings.TryChangeHotkey(SelectedHotkey))
        {
            Message = "This combination is invalid or unavailable. Choose another; your current hotkey is still active.";
            return false;
        }

        _settings.TryChangeCaptureMode(SelectedCaptureMode);

        try
        {
            _save(new StoredSettings(_settings.ScreenshotHotkey, _settings.CaptureMode));
            Message = "Settings applied and saved.";
            return true;
        }
        catch (Exception error)
        {
            // Catch broadly at the persistence boundary to report failures through the UI.
            Message = "The settings are active for this session, but could not be saved. " + error.Message;
            return false;
        }
    }

    public void Cancel()
    {
        SelectedHotkey = _settings.ScreenshotHotkey;
        SelectedCaptureMode = _settings.CaptureMode;
        Message = "";
    }
}
