using System.Windows;
using System.IO;
using System.Text.Json;
using Application = System.Windows.Application;

namespace Eikonelle;

/// <summary>Owns the application windows, hotkey listener, and tray shell.</summary>
public partial class App : Application
{
    private PreviewWindow? _preview;
    private HotkeyListener? _hotkey;
    private TrayIconShell? _tray;
    private Settings? _settings;
    private SettingsWindow? _settingsWindow;
    private AppUiShell? _ui;
    private readonly SettingsStore _settingsStore = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Eikonelle", "settings.json"));

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Hotkey initialHotkey = Hotkey.Capture;
        CaptureMode initialCaptureMode = CaptureMode.FullScreen;
        UiMode initialUiMode = UiMode.System;
        try
        {
            StoredSettings stored = _settingsStore.Load();
            initialHotkey = stored.Hotkey;
            initialCaptureMode = stored.CaptureMode;
            initialUiMode = stored.UiMode;
            if (!Settings.IsValid(initialHotkey))
            {
                initialHotkey = Hotkey.Capture;
                System.Windows.MessageBox.Show("The saved screenshot hotkey is invalid. The default Ctrl+Shift+S will be used.", "Eikonelle");
            }

            if (!Settings.IsValid(initialCaptureMode))
            {
                initialCaptureMode = CaptureMode.FullScreen;
                System.Windows.MessageBox.Show("The saved capture mode is invalid. The default Full Screen will be used.", "Eikonelle");
            }

            if (!Settings.IsValid(initialUiMode))
            {
                initialUiMode = UiMode.System;
                System.Windows.MessageBox.Show("The saved UI mode is invalid. The default System will be used.", "Eikonelle");
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        {
            System.Windows.MessageBox.Show("Settings could not be loaded. The defaults will be used. " + error.Message, "Eikonelle");
        }

        // The theme is in place before any window is built.
        _ui = new AppUiShell(Resources, Dispatcher);
        _ui.Apply(initialUiMode);

        var model = new PreviewModel();
        _preview = new PreviewWindow(model);

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

        _settings = new Settings(_hotkey.TryChangeHotkey, initialHotkey, initialCaptureMode, initialUiMode);
        var command = new ScreenshotCommand(model, _settings, RegionSelectionWindow.Select);
        _hotkey.Pressed += (_, _) =>
        {
            if (command.Execute())
            {
                _preview.ShowCurrent();
            }
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
            _settingsWindow = new SettingsWindow(_settings, _settingsStore, ApplyUiMode);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }

        _settingsWindow.Show();
        _settingsWindow.WindowState = WindowState.Normal;
        _settingsWindow.Activate();
    }

    /// <summary>Redraw the running UI in the theme of the given mode.</summary>
    public void ApplyUiMode(UiMode mode) => _ui?.Apply(mode);

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _hotkey?.Dispose();
        _ui?.Dispose();
        base.OnExit(e);
    }
}
