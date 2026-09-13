using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `taking-screenshot` (ceps/cases/taking-screenshot.md).
public class TakingScreenshot
{
    [Fact]
    public void The_default_trigger_is_Ctrl_Shift_S()
    {
        Hotkey trigger = Hotkey.Capture;

        Assert.Equal(HotkeyModifiers.Control | HotkeyModifiers.Shift, trigger.Modifiers);
        Assert.Equal((uint)'S', trigger.VirtualKey);
    }

    [Fact]
    public void A_capture_covers_the_whole_primary_monitor()
    {
        ScreenBounds primary = PrimaryScreen.Bounds;
        Assert.True(primary.Width > 0 && primary.Height > 0, "primary monitor bounds should be known");

        using Screenshot shot = ScreenshotCapture.Capture();

        Assert.Equal(primary.Width, shot.Width);
        Assert.Equal(primary.Height, shot.Height);
    }

    [Fact]
    public void Pressing_the_trigger_shows_the_screenshot_in_the_preview_window()
    {
        var preview = new PreviewModel();
        var takeScreenshot = new ScreenshotCommand(preview);

        Assert.False(preview.IsVisible);
        Assert.Null(preview.Current);

        takeScreenshot.Execute();

        Assert.True(preview.IsVisible);
        Assert.NotNull(preview.Current);
        Assert.Equal(PrimaryScreen.Bounds.Width, preview.Current!.Width);
        Assert.Equal(PrimaryScreen.Bounds.Height, preview.Current!.Height);
    }
}
