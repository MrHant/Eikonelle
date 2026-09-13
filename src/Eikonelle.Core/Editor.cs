using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eikonelle;

/// <summary>The drawing tools available in the screenshot editor.</summary>
public enum EditorTool
{
    Pen,
    Highlighter,
    Line,
    Rectangle,
    Ellipse,
}

/// <summary>A position in screenshot pixels.</summary>
public readonly record struct EditorPoint(float X, float Y);

/// <summary>
/// An isolated, in-memory editing session. Gestures change a private copy of the
/// screenshot; only <see cref="Apply"/> produces a screenshot for the preview.
/// </summary>
public sealed class EditorSession : IDisposable
{
    private readonly Bitmap _working;
    private Bitmap? _gestureStart;
    private readonly List<EditorPoint> _points = [];
    private EditorTool _tool;
    private bool _disposed;

    public EditorSession(Screenshot screenshot)
    {
        ArgumentNullException.ThrowIfNull(screenshot);
        _working = new Bitmap(screenshot.Image);
    }

    /// <summary>The edited pixels currently displayed by the editor.</summary>
    public Bitmap Image
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _working;
        }
    }

    /// <summary>Raised when the editor image changes during a gesture.</summary>
    public event EventHandler? Changed;

    public void BeginGesture(EditorTool tool, EditorPoint start)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        CancelGesture();
        _tool = tool;
        _gestureStart = new Bitmap(_working);
        _points.Add(start);
    }

    public void UpdateGesture(EditorPoint point)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_gestureStart is null)
        {
            throw new InvalidOperationException("BeginGesture must be called before updating a gesture.");
        }

        _points.Add(point);
        RestoreGestureStart();
        RenderGesture();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void EndGesture(EditorPoint end)
    {
        UpdateGesture(end);
        _gestureStart!.Dispose();
        _gestureStart = null;
        _points.Clear();
    }

    public void CancelGesture()
    {
        if (_gestureStart is null)
        {
            return;
        }

        RestoreGestureStart();
        _gestureStart.Dispose();
        _gestureStart = null;
        _points.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Return an independent screenshot containing all edits.</summary>
    public Screenshot Apply()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new Screenshot(new Bitmap(_working));
    }

    private void RestoreGestureStart()
    {
        using Graphics graphics = Graphics.FromImage(_working);
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.DrawImageUnscaled(_gestureStart!, 0, 0);
    }

    private void RenderGesture()
    {
        using Graphics graphics = Graphics.FromImage(_working);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.CompositingMode = CompositingMode.SourceOver;

        if (_tool is EditorTool.Pen or EditorTool.Highlighter)
        {
            DrawFreehand(graphics);
            return;
        }

        EditorPoint start = _points[0];
        EditorPoint end = _points[^1];
        using var pen = CreatePen(Color.Red, 4f);

        switch (_tool)
        {
            case EditorTool.Line:
                graphics.DrawLine(pen, start.X, start.Y, end.X, end.Y);
                break;
            case EditorTool.Rectangle:
                graphics.DrawRectangle(pen, Bounds(start, end));
                break;
            case EditorTool.Ellipse:
                graphics.DrawEllipse(pen, Bounds(start, end));
                break;
        }
    }

    private void DrawFreehand(Graphics graphics)
    {
        Color color = _tool == EditorTool.Highlighter
            ? Color.FromArgb(90, 255, 235, 59)
            : Color.Red;
        float width = _tool == EditorTool.Highlighter ? 18f : 4f;
        using Pen pen = CreatePen(color, width);

        if (_points.Count == 1)
        {
            graphics.DrawEllipse(pen, _points[0].X, _points[0].Y, 0.1f, 0.1f);
            return;
        }

        graphics.DrawLines(pen, _points.Select(point => new PointF(point.X, point.Y)).ToArray());
    }

    private static Pen CreatePen(Color color, float width) => new(color, width)
    {
        StartCap = LineCap.Round,
        EndCap = LineCap.Round,
        LineJoin = LineJoin.Round,
    };

    private static RectangleF Bounds(EditorPoint first, EditorPoint second) => new(
        Math.Min(first.X, second.X),
        Math.Min(first.Y, second.Y),
        Math.Abs(first.X - second.X),
        Math.Abs(first.Y - second.Y));

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _gestureStart?.Dispose();
        _working.Dispose();
        _disposed = true;
    }
}
