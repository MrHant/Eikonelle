using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eikonelle;

/// <summary>
/// A captured image. Owns the underlying <see cref="Bitmap"/>; dispose when done.
/// </summary>
public sealed class Screenshot : IDisposable
{
    public Screenshot(Bitmap image) => Image = image;

    /// <summary>The captured pixels.</summary>
    public Bitmap Image { get; }

    public int Width => Image.Width;

    public int Height => Image.Height;

    /// <summary>Return an independent screenshot holding only the pixels inside <paramref name="region"/>.</summary>
    public Screenshot Crop(Rectangle region)
    {
        if (region.Width <= 0 || region.Height <= 0 || !new Rectangle(0, 0, Width, Height).Contains(region))
        {
            throw new ArgumentOutOfRangeException(nameof(region), region, "The region must be a non-empty area inside the screenshot.");
        }

        var bitmap = new Bitmap(region.Width, region.Height, Image.PixelFormat);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImage(Image, new Rectangle(0, 0, region.Width, region.Height), region, GraphicsUnit.Pixel);
        }

        return new Screenshot(bitmap);
    }

    public void Dispose() => Image.Dispose();
}
