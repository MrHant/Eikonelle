using System.Drawing;

namespace Eikonelle;

/// <summary>
/// Takes a screenshot using the capture mode in effect, saves it into the configured
/// save folder, and shows it in the preview.
/// </summary>
public sealed class ScreenshotCommand
{
    private readonly PreviewModel _preview;
    private readonly Func<CaptureMode> _captureMode;
    private readonly Func<string> _saveFolder;
    private readonly Func<Screenshot, Rectangle?> _selectRegion;
    private bool _capturing;

    public ScreenshotCommand(PreviewModel preview, string saveFolder)
        : this(preview, () => CaptureMode.FullScreen, () => saveFolder, _ => null)
    {
    }

    /// <param name="selectRegion">
    /// Lets the user select a region of the full-screen screenshot it is given, returning
    /// <c>null</c> when nothing is selected. The screenshot remains owned by the command.
    /// </param>
    public ScreenshotCommand(PreviewModel preview, Settings settings, Func<Screenshot, Rectangle?> selectRegion)
        : this(preview, () => settings.CaptureMode, () => settings.SaveFolder, selectRegion)
    {
        ArgumentNullException.ThrowIfNull(settings);
    }

    private ScreenshotCommand(
        PreviewModel preview,
        Func<CaptureMode> captureMode,
        Func<string> saveFolder,
        Func<Screenshot, Rectangle?> selectRegion)
    {
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(selectRegion);
        _preview = preview;
        _captureMode = captureMode;
        _saveFolder = saveFolder;
        _selectRegion = selectRegion;
    }

    /// <summary>The file the last shown screenshot was saved to, or <c>null</c> if it could not be saved.</summary>
    public string? SavedPath { get; private set; }

    /// <summary>Why the last shown screenshot could not be saved; empty when it was saved.</summary>
    public string Message { get; private set; } = "";

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
            DateTime takenAt = DateTime.Now;
            Screenshot fullScreen = ScreenshotCapture.Capture();
            if (_captureMode() != CaptureMode.Region)
            {
                SaveAndShow(fullScreen, takenAt);
                return true;
            }

            using (fullScreen)
            {
                if (_selectRegion(fullScreen) is not { } region)
                {
                    return false;
                }

                SaveAndShow(fullScreen.Crop(region), takenAt);
                return true;
            }
        }
        finally
        {
            _capturing = false;
        }
    }

    private void SaveAndShow(Screenshot screenshot, DateTime takenAt)
    {
        string folder = _saveFolder();
        try
        {
            SavedPath = ScreenshotFolder.Save(screenshot, folder, takenAt);
            Message = "";
        }
        catch (Exception error)
        {
            // Catch broadly at the file boundary: a failed save must not lose the screenshot.
            SavedPath = null;
            Message = $"The screenshot could not be saved into \"{folder}\". " + error.Message;
        }

        _preview.Show(screenshot);
    }
}
