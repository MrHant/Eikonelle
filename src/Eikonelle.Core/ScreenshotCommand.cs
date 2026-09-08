namespace Eikonelle;

/// <summary>
/// The action bound to the <see cref="Hotkey.Capture"/> combination: take a screenshot
/// of the primary monitor and show it in the preview window.
/// </summary>
public sealed class ScreenshotCommand
{
    private readonly PreviewModel _preview;

    public ScreenshotCommand(PreviewModel preview) => _preview = preview;

    public void Execute() => _preview.Show(ScreenshotCapture.Capture());
}
