using System.Windows;
using System.IO;
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

        StoredSettings stored = _settingsStore.Load(out IReadOnlyList<string> problems);
        if (problems.Count > 0)
        {
            System.Windows.MessageBox.Show(string.Join(Environment.NewLine, problems), "Eikonelle");
        }

        // The theme is in place before any window is built.
        _ui = new AppUiShell(Resources, Dispatcher);
        _ui.Apply(stored.UiMode);

        var model = new PreviewModel();
        _preview = new PreviewWindow(model);

        try
        {
            _hotkey = new HotkeyListener(_preview, stored.Hotkey);
        }
        catch (InvalidOperationException)
        {
            System.Windows.MessageBox.Show("The screenshot hotkey is unavailable. Close the application using that combination and restart Eikonelle.", "Eikonelle");
            Shutdown();
            return;
        }

        _settings = new Settings(
            _hotkey.TryChangeHotkey, stored.Hotkey, stored.CaptureMode, stored.UiMode, stored.SaveFolder);
        var command = new ScreenshotCommand(model, _settings, RegionSelectionWindow.Select);
        _hotkey.Pressed += (_, _) =>
        {
            if (command.Execute())
            {
                _preview.ShowCurrent();
                if (command.Message.Length > 0)
                {
                    System.Windows.MessageBox.Show(_preview, command.Message, "Eikonelle");
                }
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
