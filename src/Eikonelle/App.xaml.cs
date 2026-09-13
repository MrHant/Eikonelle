using System.Windows;
using System.IO;
using System.Text.Json;
using Application = System.Windows.Application;

namespace Eikonelle;

/// <summary>
/// Wires the configured hotkey to a screenshot of the
/// primary monitor shown in the <see cref="PreviewWindow"/>. The app runs without a
/// visible window until the first capture.
/// </summary>
public partial class App : Application
{
    private PreviewWindow? _preview;
    private HotkeyListener? _hotkey;
    private TrayIconShell? _tray;
    private Settings? _settings;
    private SettingsWindow? _settingsWindow;
    private readonly SettingsStore _settingsStore = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Eikonelle", "settings.json"));

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var model = new PreviewModel();
        var command = new ScreenshotCommand(model);
        _preview = new PreviewWindow(model);

        Hotkey initialHotkey = Hotkey.Capture;
        try
        {
            initialHotkey = _settingsStore.Load();
            if (!Settings.IsValid(initialHotkey))
            {
                initialHotkey = Hotkey.Capture;
                System.Windows.MessageBox.Show("The saved screenshot hotkey is invalid. The default Ctrl+Shift+S will be used.", "Eikonelle");
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        {
            System.Windows.MessageBox.Show("Settings could not be loaded. The default Ctrl+Shift+S will be used. " + error.Message, "Eikonelle");
        }
        try
        {
            _hotkey = new HotkeyListener(_preview, initialHotkey);
        }
        catch (InvalidOperationException)
        {
            System.Windows.MessageBox.Show("The screenshot hotkey is unavailable. Close the application using that combination and restart Eikonelle.", "Eikonelle");
            Shutdown();
            return;
        }

        _settings = new Settings(_hotkey.TryChangeHotkey, initialHotkey);
        _hotkey.Pressed += (_, _) =>
        {
            command.Execute();
            _preview.ShowCurrent();
        };

        _tray = new TrayIconShell(TrayIcon.CreateDefault(exit: Shutdown, openSettings: ShowSettings));
        _tray.Show();
    }

    public void ShowSettings()
    {
        if (_settings is null)
        {
            return;
        }

        if (_settingsWindow is null)
        {
            _settingsWindow = new SettingsWindow(_settings, _settingsStore);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }

        _settingsWindow.Show();
        _settingsWindow.WindowState = WindowState.Normal;
        _settingsWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _hotkey?.Dispose();
        base.OnExit(e);
    }
}
