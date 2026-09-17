using System.Windows;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace Eikonelle;

/// <summary>Interactive WPF shell for an in-memory <see cref="EditorSession"/>.</summary>
public partial class EditorWindow : Window
{
    private readonly EditorSession _session;
    private readonly Action<Screenshot> _apply;
    private EditorTool _tool = EditorTool.Pen;
    private bool _drawing;

    public EditorWindow(Screenshot screenshot, Action<Screenshot> apply)
    {
        ArgumentNullException.ThrowIfNull(screenshot);
        ArgumentNullException.ThrowIfNull(apply);
        InitializeComponent();
        _session = new EditorSession(screenshot);
        _apply = apply;
        _session.Changed += Session_Changed;
        Menu.Show(AppMenu.ForEditor(ApplyEdits, CancelEdits));
        RefreshImage();
    }

    private void SelectTool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string name } && Enum.TryParse(name, out EditorTool tool))
        {
            _tool = tool;
            SelectedToolText.Text = tool.ToString();
        }
    }

    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!TryGetImagePoint(e.GetPosition(EditorImage), out EditorPoint point))
        {
            return;
        }

        _session.BeginGesture(_tool, point);
        _drawing = true;
        EditorImage.CaptureMouse();
        e.Handled = true;
    }

    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_drawing || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        _session.UpdateGesture(GetClampedImagePoint(e.GetPosition(EditorImage)));
    }

    private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_drawing)
        {
            return;
        }

        _session.EndGesture(GetClampedImagePoint(e.GetPosition(EditorImage)));
        _drawing = false;
        EditorImage.ReleaseMouseCapture();
        e.Handled = true;
    }

    private void Apply_Click(object sender, RoutedEventArgs e) => ApplyEdits();

    private void Cancel_Click(object sender, RoutedEventArgs e) => CancelEdits();

    private void ApplyEdits()
    {
        if (_drawing)
        {
            _session.CancelGesture();
            _drawing = false;
        }

        _apply(_session.Apply());
        Close();
    }

    private void CancelEdits() => Close();

    private void Session_Changed(object? sender, EventArgs e) => RefreshImage();

    private void RefreshImage() => EditorImage.Source = BitmapSourceFactory.Create(_session.Image);

    private bool TryGetImagePoint(Point position, out EditorPoint point)
    {
        (double scale, double offsetX, double offsetY) = ImageLayout();
        double x = (position.X - offsetX) / scale;
        double y = (position.Y - offsetY) / scale;
        bool inside = x >= 0 && y >= 0 && x < _session.Image.Width && y < _session.Image.Height;
        point = new EditorPoint((float)x, (float)y);
        return inside;
    }

    private EditorPoint GetClampedImagePoint(Point position)
    {
        (double scale, double offsetX, double offsetY) = ImageLayout();
        float x = (float)Math.Clamp((position.X - offsetX) / scale, 0, _session.Image.Width - 1);
        float y = (float)Math.Clamp((position.Y - offsetY) / scale, 0, _session.Image.Height - 1);
        return new EditorPoint(x, y);
    }

    private (double Scale, double OffsetX, double OffsetY) ImageLayout()
    {
        double scale = Math.Min(
            EditorImage.ActualWidth / _session.Image.Width,
            EditorImage.ActualHeight / _session.Image.Height);
        double width = _session.Image.Width * scale;
        double height = _session.Image.Height * scale;
        return (scale, (EditorImage.ActualWidth - width) / 2, (EditorImage.ActualHeight - height) / 2);
    }

    protected override void OnClosed(EventArgs e)
    {
        _session.Changed -= Session_Changed;
        _session.Dispose();
        base.OnClosed(e);
    }
}
