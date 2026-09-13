using System.ComponentModel;
using System.Windows;

namespace Eikonelle;

/// <summary>
/// Shows the screenshot currently held by a <see cref="PreviewModel"/>.
/// </summary>
public partial class PreviewWindow : Window
{
    private readonly PreviewModel _model;
    private EditorWindow? _editor;

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
            PreviewImage.Source = _model.Current is { } screenshot
                ? BitmapSourceFactory.Create(screenshot.Image)
                : null;
            OpenEditorButton.IsEnabled = _model.Current is not null;
        }
    }

    private void OpenEditor_Click(object sender, RoutedEventArgs e)
    {
        if (_model.Current is not { } screenshot)
        {
            return;
        }

        if (_editor is not null)
        {
            _editor.Activate();
            return;
        }

        _editor = new EditorWindow(screenshot, edited => _model.Show(edited))
        {
            Owner = this,
        };
        _editor.Closed += (_, _) => _editor = null;
        _editor.Show();
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

}
