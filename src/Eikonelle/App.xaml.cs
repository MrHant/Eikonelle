using System.Windows;

namespace Eikonelle;

/// <summary>
/// Wires the fixed <see cref="Hotkey.Capture"/> combination to a screenshot of the
/// primary monitor shown in the <see cref="PreviewWindow"/>. The app runs without a
/// visible window until the first capture.
/// </summary>
public partial class App : Application
{
    private PreviewWindow? _preview;
    private HotkeyListener? _hotkey;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var model = new PreviewModel();
        var command = new ScreenshotCommand(model);
        _preview = new PreviewWindow(model);

        _hotkey = new HotkeyListener(_preview, Hotkey.Capture);
        _hotkey.Pressed += (_, _) =>
        {
            command.Execute();
            _preview.ShowCurrent();
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkey?.Dispose();
        base.OnExit(e);
    }
}
