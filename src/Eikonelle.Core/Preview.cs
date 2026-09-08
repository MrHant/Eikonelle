using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Eikonelle;

/// <summary>
/// State of the preview window: the screenshot currently on show, if any.
/// The window itself observes this model and follows it.
/// </summary>
public sealed class PreviewModel : INotifyPropertyChanged
{
    private Screenshot? _current;
    private bool _isVisible;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The screenshot being shown, or <c>null</c> before the first capture.</summary>
    public Screenshot? Current
    {
        get => _current;
        private set => Set(ref _current, value);
    }

    /// <summary>Whether the preview window is showing a screenshot.</summary>
    public bool IsVisible
    {
        get => _isVisible;
        private set => Set(ref _isVisible, value);
    }

    /// <summary>Show <paramref name="screenshot"/> in the preview, replacing whatever was there.</summary>
    public void Show(Screenshot screenshot)
    {
        ArgumentNullException.ThrowIfNull(screenshot);
        Current = screenshot;
        IsVisible = true;
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
