using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Eikonelle;

/// <summary>
/// Draws the application UI in the theme of its <see cref="UiAppearance"/>, by swapping
/// the theme dictionary the shared control styles read their brushes from. Reports
/// Windows appearance changes to the model, so the UI follows Windows while the
/// chosen mode is <see cref="UiMode.System"/>.
/// </summary>
public sealed class AppUiShell : IDisposable
{
    private static readonly Uri LightTheme = new("Themes/Light.xaml", UriKind.Relative);
    private static readonly Uri DarkTheme = new("Themes/Dark.xaml", UriKind.Relative);

    private readonly Collection<ResourceDictionary> _dictionaries;
    private readonly Dispatcher _dispatcher;
    private readonly UiAppearance _appearance;
    private ResourceDictionary? _applied;
    private bool _disposed;

    public AppUiShell(ResourceDictionary applicationResources, Dispatcher dispatcher, UiMode mode)
    {
        ArgumentNullException.ThrowIfNull(applicationResources);
        ArgumentNullException.ThrowIfNull(dispatcher);
        _dictionaries = applicationResources.MergedDictionaries;
        _dispatcher = dispatcher;
        _appearance = new UiAppearance(mode, SystemTheme());
        Draw();
        _appearance.ThemeChanged += OnThemeChanged;
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    /// <summary>The theme currently drawn.</summary>
    public AppTheme Theme => _appearance.Theme;

    /// <summary>Raised after the UI has been redrawn in another theme.</summary>
    public event EventHandler? ThemeChanged;

    /// <summary>Draw the UI in the theme of the given mode.</summary>
    public void Apply(UiMode mode) => _appearance.Apply(mode);

    /// <summary>The appearance Windows is configured with; light when it is not recorded.</summary>
    public static AppTheme SystemTheme()
    {
        object? value = Registry.GetValue(
            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "AppsUseLightTheme",
            null);
        return value is int light && light == 0 ? AppTheme.Dark : AppTheme.Light;
    }

    private void Draw()
    {
        var replacement = new ResourceDictionary
        {
            Source = _appearance.Theme == AppTheme.Dark ? DarkTheme : LightTheme,
        };

        // Add before removing so the brushes stay resolvable throughout the swap.
        _dictionaries.Add(replacement);
        if (_applied is not null)
        {
            _dictionaries.Remove(_applied);
        }

        _applied = replacement;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Draw();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General)
        {
            return;
        }

        // Windows raises this on its own thread; the resources belong to the UI thread.
        _dispatcher.BeginInvoke(() =>
        {
            if (!_disposed)
            {
                _appearance.SystemThemeChanged(SystemTheme());
            }
        });
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        _appearance.ThemeChanged -= OnThemeChanged;
    }
}
