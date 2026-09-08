using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Eikonelle;

/// <summary>
/// Shows the screenshot currently held by a <see cref="PreviewModel"/>.
/// </summary>
public partial class PreviewWindow : Window
{
    private readonly PreviewModel _model;

    public PreviewWindow(PreviewModel model)
    {
        InitializeComponent();
        _model = model;
        _model.PropertyChanged += OnModelChanged;
    }

    private void OnModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PreviewModel.Current))
        {
            PreviewImage.Source = _model.Current is { } screenshot ? ToImageSource(screenshot) : null;
        }
    }

    /// <summary>Bring the preview to the foreground, showing it if it is hidden.</summary>
    public void ShowCurrent()
    {
        if (!IsVisible)
        {
            Show();
        }

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // The app owns the lifetime; hide instead of destroying the window.
        e.Cancel = true;
        Hide();
    }

    private static ImageSource ToImageSource(Screenshot screenshot)
    {
        IntPtr hBitmap = screenshot.Image.GetHbitmap();
        try
        {
            ImageSource source = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);
}
