using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Eikonelle;

/// <summary>
/// Draws the application UI in the theme <see cref="AppUi"/> resolves for the chosen
/// <see cref="UiMode"/>, by swapping the theme dictionary the shared control styles
/// read their brushes from. Follows Windows while the chosen mode is
/// <see cref="UiMode.System"/>.
/// </summary>
public sealed class AppUiShell : IDisposable
{
    private static readonly Uri LightTheme = new("Themes/Light.xaml", UriKind.Relative);
    private static readonly Uri DarkTheme = new("Themes/Dark.xaml", UriKind.Relative);

    private readonly Collection<ResourceDictionary> _dictionaries;
    private readonly Dispatcher _dispatcher;
    private ResourceDictionary? _applied;
    private UiMode _mode = UiMode.System;
    private bool _disposed;

    public AppUiShell(ResourceDictionary applicationResources, Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(applicationResources);
        ArgumentNullException.ThrowIfNull(dispatcher);
        _dictionaries = applicationResources.MergedDictionaries;
        _dispatcher = dispatcher;
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    /// <summary>The theme currently drawn.</summary>
    public AppTheme Theme { get; private set; } = AppTheme.Light;

    /// <summary>Draw the UI in the theme of the given mode.</summary>
    public void Apply(UiMode mode)
    {
        _mode = mode;
        Draw();
    }

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
        AppTheme theme = AppUi.ThemeFor(_mode, SystemTheme());
        var replacement = new ResourceDictionary
        {
            Source = theme == AppTheme.Dark ? DarkTheme : LightTheme,
        };

        // Add before removing so the brushes stay resolvable throughout the swap.
        _dictionaries.Add(replacement);
        if (_applied is not null)
        {
            _dictionaries.Remove(_applied);
        }

        _applied = replacement;
        Theme = theme;
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General || _mode != UiMode.System)
        {
            return;
        }

        // Windows raises this on its own thread; the resources belong to the UI thread.
        _dispatcher.BeginInvoke(() =>
        {
            if (!_disposed)
            {
                Draw();
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
    }
}
