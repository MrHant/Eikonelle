using System.Drawing;

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

    public void Dispose() => Image.Dispose();
}
