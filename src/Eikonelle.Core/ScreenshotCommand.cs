using System.Drawing;

namespace Eikonelle;

/// <summary>Takes a screenshot using the capture mode in effect and shows it in the preview.</summary>
public sealed class ScreenshotCommand
{
    private readonly PreviewModel _preview;
    private readonly Func<CaptureMode> _captureMode;
    private readonly Func<Screenshot, Rectangle?> _selectRegion;
    private bool _capturing;

    public ScreenshotCommand(PreviewModel preview)
        : this(preview, () => CaptureMode.FullScreen, _ => null)
    {
    }

    /// <param name="selectRegion">
    /// Lets the user select a region of the full-screen screenshot it is given, returning
    /// <c>null</c> when nothing is selected. The screenshot remains owned by the command.
    /// </param>
    public ScreenshotCommand(PreviewModel preview, Settings settings, Func<Screenshot, Rectangle?> selectRegion)
        : this(preview, () => settings.CaptureMode, selectRegion)
    {
        ArgumentNullException.ThrowIfNull(settings);
    }

    private ScreenshotCommand(PreviewModel preview, Func<CaptureMode> captureMode, Func<Screenshot, Rectangle?> selectRegion)
    {
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(selectRegion);
        _preview = preview;
        _captureMode = captureMode;
        _selectRegion = selectRegion;
    }

    /// <summary>Take a screenshot; returns whether one was shown in the preview.</summary>
    public bool Execute()
    {
        // A region selection runs a nested message loop, so the hotkey can re-enter here.
        if (_capturing)
        {
            return false;
        }

        _capturing = true;
        try
        {
            Screenshot fullScreen = ScreenshotCapture.Capture();
            if (_captureMode() != CaptureMode.Region)
            {
                _preview.Show(fullScreen);
                return true;
            }

            using (fullScreen)
            {
                if (_selectRegion(fullScreen) is not { } region)
                {
                    return false;
                }

                _preview.Show(fullScreen.Crop(region));
                return true;
            }
        }
        finally
        {
            _capturing = false;
        }
    }
}
