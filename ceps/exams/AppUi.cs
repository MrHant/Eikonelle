using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `app_ui` (ceps/cases/app_ui.md).
public class AppUiExam
{
    [Theory]
    [InlineData(AppTheme.Light)]
    [InlineData(AppTheme.Dark)]
    public void The_light_mode_draws_the_ui_light_whatever_windows_uses(AppTheme systemTheme)
    {
        Assert.Equal(AppTheme.Light, AppUi.ThemeFor(UiMode.Light, systemTheme));
    }

    [Theory]
    [InlineData(AppTheme.Light)]
    [InlineData(AppTheme.Dark)]
    public void The_dark_mode_draws_the_ui_dark_whatever_windows_uses(AppTheme systemTheme)
    {
        Assert.Equal(AppTheme.Dark, AppUi.ThemeFor(UiMode.Dark, systemTheme));
    }

    [Theory]
    [InlineData(AppTheme.Light)]
    [InlineData(AppTheme.Dark)]
    public void The_system_mode_follows_windows(AppTheme systemTheme)
    {
        Assert.Equal(systemTheme, AppUi.ThemeFor(UiMode.System, systemTheme));
    }

    [Fact]
    public void Only_light_and_dark_are_drawn()
    {
        Assert.Equal(new[] { AppTheme.Light, AppTheme.Dark }, Enum.GetValues<AppTheme>());
    }

    [Fact]
    public void An_unsupported_ui_mode_falls_back_to_following_windows()
    {
        Assert.Equal(AppTheme.Dark, AppUi.ThemeFor((UiMode)42, AppTheme.Dark));
        Assert.Equal(AppTheme.Light, AppUi.ThemeFor((UiMode)42, AppTheme.Light));
    }

    [Theory]
    [InlineData(UiMode.Light, AppTheme.Light)]
    [InlineData(UiMode.Dark, AppTheme.Dark)]
    public void The_ui_mode_in_settings_decides_the_theme(UiMode mode, AppTheme expected)
    {
        var settings = new Settings(_ => true);
        var session = new SettingsSession(settings, _ => { });
        session.SelectedUiMode = mode;

        // Before Apply the theme still follows the mode in effect, which is System.
        Assert.Equal(AppTheme.Dark, AppUi.ThemeFor(settings.UiMode, AppTheme.Dark));

        Assert.True(session.Apply());

        Assert.Equal(expected, AppUi.ThemeFor(settings.UiMode, AppTheme.Dark));
        Assert.Equal(expected, AppUi.ThemeFor(settings.UiMode, AppTheme.Light));
    }

    [Fact]
    public void Switching_back_to_the_system_mode_follows_windows_again()
    {
        var settings = new Settings(_ => true, null, CaptureMode.FullScreen, UiMode.Light);
        var session = new SettingsSession(settings, _ => { });
        session.SelectedUiMode = UiMode.System;

        Assert.True(session.Apply());

        Assert.Equal(AppTheme.Dark, AppUi.ThemeFor(settings.UiMode, AppTheme.Dark));
        Assert.Equal(AppTheme.Light, AppUi.ThemeFor(settings.UiMode, AppTheme.Light));
    }
}
