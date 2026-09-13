namespace Eikonelle;

public sealed class ScreenshotCommand
{
    private readonly PreviewModel _preview;

    public ScreenshotCommand(PreviewModel preview) => _preview = preview;

    public void Execute() => _preview.Show(ScreenshotCapture.Capture());
}
