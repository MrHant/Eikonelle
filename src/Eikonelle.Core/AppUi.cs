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

/// <summary>
/// The theme the running application draws: the UI mode in effect combined with the
/// appearance Windows is configured with, reporting each change of the drawn theme.
/// </summary>
public sealed class UiAppearance(UiMode mode, AppTheme systemTheme)
{
    public UiMode Mode { get; private set; } = mode;

    public AppTheme SystemTheme { get; private set; } = systemTheme;

    public AppTheme Theme => AppUi.ThemeFor(Mode, SystemTheme);

    /// <summary>Raised when <see cref="Theme"/> changes.</summary>
    public event EventHandler? ThemeChanged;

    /// <summary>Put a newly applied UI mode in effect.</summary>
    public void Apply(UiMode mode) => Change(() => Mode = mode);

    /// <summary>Windows now uses <paramref name="theme"/> for apps.</summary>
    public void SystemThemeChanged(AppTheme theme) => Change(() => SystemTheme = theme);

    private void Change(Action change)
    {
        AppTheme before = Theme;
        change();
        if (Theme != before)
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
