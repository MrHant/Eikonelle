using System.Runtime.InteropServices;

namespace Eikonelle;

/// <summary>Pixel bounds of a monitor.</summary>
public readonly record struct ScreenBounds(int X, int Y, int Width, int Height)
{
    public System.Drawing.Size Size => new(Width, Height);
}

/// <summary>
/// The monitor a screenshot is taken from. Eikonelle captures the primary monitor only.
/// </summary>
public static class PrimaryScreen
{
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    /// <summary>Bounds of the primary monitor, in physical pixels, with its origin at (0, 0).</summary>
    public static ScreenBounds Bounds =>
        new(0, 0, GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN));
}
