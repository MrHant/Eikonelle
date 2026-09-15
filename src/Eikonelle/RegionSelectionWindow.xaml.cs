using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;

namespace Eikonelle;

/// <summary>
/// Covers the primary monitor with a frozen screenshot and lets the user drag out a region.
/// </summary>
public partial class RegionSelectionWindow : Window
{
    private readonly Screenshot _screenshot;
    private readonly RegionSelection _selection;
    private DrawingRectangle? _selected;

    private RegionSelectionWindow(Screenshot screenshot)
    {
        InitializeComponent();
        _screenshot = screenshot;
        _selection = new RegionSelection(new System.Drawing.Size(screenshot.Width, screenshot.Height));
        ScreenImage.Source = BitmapSourceFactory.Create(screenshot.Image);
        Left = 0;
        Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;
    }

    /// <summary>Show the selection overlay and return the selected region, or <c>null</c> if cancelled.</summary>
    public static DrawingRectangle? Select(Screenshot screenshot)
    {
        var window = new RegionSelectionWindow(screenshot);
        window.ShowDialog();
        return window._selected;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        Activate();
        Focus();
    }

    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _selection.Begin(ToPixels(e.GetPosition(this)));
        ((UIElement)sender).CaptureMouse();
        ShowRegion();
        e.Handled = true;
    }

    private void Overlay_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_selection.IsDragging)
        {
            return;
        }

        _selection.Drag(ToPixels(e.GetPosition(this)));
        ShowRegion();
    }

    private void Overlay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_selection.IsDragging)
        {
            return;
        }

        ((UIElement)sender).ReleaseMouseCapture();
        e.Handled = true;
        if (_selection.End(ToPixels(e.GetPosition(this))) is { } region)
        {
            _selected = region;
            Close();
            return;
        }

        SelectionRectangle.Visibility = Visibility.Collapsed;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            _selected = null;
            Close();
        }
    }

    private DrawingPoint ToPixels(System.Windows.Point position) => new(
        (int)Math.Round(position.X * _screenshot.Width / Math.Max(ActualWidth, 1)),
        (int)Math.Round(position.Y * _screenshot.Height / Math.Max(ActualHeight, 1)));

    private void ShowRegion()
    {
        DrawingRectangle region = _selection.Region;
        double scaleX = ActualWidth / _screenshot.Width;
        double scaleY = ActualHeight / _screenshot.Height;
        Canvas.SetLeft(SelectionRectangle, region.X * scaleX);
        Canvas.SetTop(SelectionRectangle, region.Y * scaleY);
        SelectionRectangle.Width = region.Width * scaleX;
        SelectionRectangle.Height = region.Height * scaleY;
        SelectionRectangle.Visibility = Visibility.Visible;
    }
}
