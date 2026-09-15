using System.Drawing;
using System.Drawing.Imaging;
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

    [Fact]
    public void Full_Screen_mode_shows_the_whole_primary_monitor_without_selecting_a_region()
    {
        var preview = new PreviewModel();
        var settings = new Settings(_ => true, null, CaptureMode.FullScreen);
        var takeScreenshot = new ScreenshotCommand(preview, settings,
            _ => throw new Exception("Full Screen mode must not ask for a region."));

        Assert.True(takeScreenshot.Execute());

        Assert.True(preview.IsVisible);
        Assert.Equal(PrimaryScreen.Bounds.Width, preview.Current!.Width);
        Assert.Equal(PrimaryScreen.Bounds.Height, preview.Current!.Height);
    }

    [Fact]
    public void Region_mode_shows_only_the_dragged_region_of_the_primary_monitor()
    {
        var preview = new PreviewModel();
        var settings = new Settings(_ => true, null, CaptureMode.Region);
        var region = new Rectangle(10, 20, 40, 30);
        Color topLeft = default, bottomRight = default;
        var takeScreenshot = new ScreenshotCommand(preview, settings, fullScreen =>
        {
            Assert.Equal(PrimaryScreen.Bounds.Width, fullScreen.Width);
            Assert.Equal(PrimaryScreen.Bounds.Height, fullScreen.Height);
            topLeft = fullScreen.Image.GetPixel(region.Left, region.Top);
            bottomRight = fullScreen.Image.GetPixel(region.Right - 1, region.Bottom - 1);
            return region;
        });

        Assert.True(takeScreenshot.Execute());

        Assert.True(preview.IsVisible);
        Screenshot shown = preview.Current!;
        Assert.Equal(region.Size, new Size(shown.Width, shown.Height));
        Assert.Equal(topLeft.ToArgb(), shown.Image.GetPixel(0, 0).ToArgb());
        Assert.Equal(bottomRight.ToArgb(), shown.Image.GetPixel(region.Width - 1, region.Height - 1).ToArgb());
    }

    [Fact]
    public void The_capture_mode_applied_in_settings_is_used_for_the_next_screenshot()
    {
        var preview = new PreviewModel();
        var settings = new Settings(_ => true);
        bool regionRequested = false;
        var takeScreenshot = new ScreenshotCommand(preview, settings, _ =>
        {
            regionRequested = true;
            return new Rectangle(0, 0, 5, 5);
        });

        takeScreenshot.Execute();
        Assert.False(regionRequested);

        var session = new SettingsSession(settings, _ => { }) { SelectedCaptureMode = CaptureMode.Region };
        Assert.True(session.Apply());
        takeScreenshot.Execute();

        Assert.True(regionRequested);
        Assert.Equal(5, preview.Current!.Width);
    }

    [Fact]
    public void Dragging_displays_the_rectangle_that_would_be_captured()
    {
        var selection = new RegionSelection(new Size(200, 100));

        selection.Begin(new Point(10, 20));
        Assert.True(selection.IsDragging);

        selection.Drag(new Point(60, 50));
        Assert.Equal(new Rectangle(10, 20, 50, 30), selection.Region);

        selection.Drag(new Point(80, 70));
        Assert.Equal(new Rectangle(10, 20, 70, 50), selection.Region);

        Assert.Equal(new Rectangle(10, 20, 70, 50), selection.End(new Point(80, 70)));
        Assert.False(selection.IsDragging);
    }

    [Fact]
    public void A_region_can_be_dragged_in_any_direction()
    {
        var selection = new RegionSelection(new Size(200, 100));

        selection.Begin(new Point(60, 50));
        selection.Drag(new Point(10, 20));

        Assert.Equal(new Rectangle(10, 20, 50, 30), selection.Region);
        Assert.Equal(new Rectangle(10, 20, 50, 30), selection.End(new Point(10, 20)));
    }

    [Fact]
    public void A_captured_region_contains_the_pixels_of_that_area()
    {
        var source = new Bitmap(4, 3, PixelFormat.Format32bppArgb);
        source.SetPixel(1, 1, Color.Red);
        source.SetPixel(2, 2, Color.Blue);
        using var screenshot = new Screenshot(source);

        using Screenshot region = screenshot.Crop(new Rectangle(1, 1, 2, 2));

        Assert.Equal(2, region.Width);
        Assert.Equal(2, region.Height);
        Assert.Equal(Color.Red.ToArgb(), region.Image.GetPixel(0, 0).ToArgb());
        Assert.Equal(Color.Blue.ToArgb(), region.Image.GetPixel(1, 1).ToArgb());
    }
}
