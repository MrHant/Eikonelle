using System.Drawing;

namespace Eikonelle;

/// <summary>
/// A region of a screenshot chosen by clicking, dragging, and releasing.
/// Positions are in screenshot pixels and are kept inside the screenshot.
/// </summary>
public sealed class RegionSelection
{
    private readonly Size _bounds;
    private Point _start;

    public RegionSelection(Size bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bounds), bounds, "The selection area must not be empty.");
        }

        _bounds = bounds;
    }

    /// <summary>Whether a drag is in progress.</summary>
    public bool IsDragging { get; private set; }

    /// <summary>The area that would be captured if the drag ended at the latest position.</summary>
    public Rectangle Region { get; private set; }

    public void Begin(Point start)
    {
        _start = Clamp(start);
        Region = new Rectangle(_start, Size.Empty);
        IsDragging = true;
    }

    public void Drag(Point current)
    {
        if (!IsDragging)
        {
            throw new InvalidOperationException("Begin must be called before dragging.");
        }

        Point end = Clamp(current);
        Region = Rectangle.FromLTRB(
            Math.Min(_start.X, end.X),
            Math.Min(_start.Y, end.Y),
            Math.Max(_start.X, end.X),
            Math.Max(_start.Y, end.Y));
    }

    /// <summary>Finish the drag, returning the selected region, or <c>null</c> if it is empty.</summary>
    public Rectangle? End(Point end)
    {
        Drag(end);
        IsDragging = false;
        return Region.Width > 0 && Region.Height > 0 ? Region : null;
    }

    private Point Clamp(Point point) => new(
        Math.Clamp(point.X, 0, _bounds.Width),
        Math.Clamp(point.Y, 0, _bounds.Height));
}
