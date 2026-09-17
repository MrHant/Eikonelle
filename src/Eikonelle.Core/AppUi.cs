namespace Eikonelle;

/// <summary>The appearance the application UI is actually drawn with.</summary>
public enum AppTheme
{
    Light,
    Dark,
}

/// <summary>
/// The appearance of the application UI. Headless model; the WPF shell draws the
/// resolved <see cref="AppTheme"/>.
/// </summary>
public static class AppUi
{
    /// <summary>
    /// The theme for the UI mode chosen in settings. <see cref="UiMode.Light"/> and
    /// <see cref="UiMode.Dark"/> decide it; <see cref="UiMode.System"/> defers to
    /// <paramref name="systemTheme"/>, as does any value that is not a UI mode.
    /// </summary>
    public static AppTheme ThemeFor(UiMode mode, AppTheme systemTheme) => mode switch
    {
        UiMode.Light => AppTheme.Light,
        UiMode.Dark => AppTheme.Dark,
        _ => systemTheme,
    };
}
