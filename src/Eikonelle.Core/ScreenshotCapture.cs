using System.Drawing;

namespace Eikonelle;

/// <summary>Takes a screenshot of the <see cref="PrimaryScreen"/>.</summary>
public static class ScreenshotCapture
{
    /// <summary>
    /// Captures the current contents of the primary monitor.
    /// </summary>
    public static Screenshot Capture()
    {
        ScreenBounds bounds = PrimaryScreen.Bounds;
        var bitmap = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
        }

        return new Screenshot(bitmap);
    }
}
