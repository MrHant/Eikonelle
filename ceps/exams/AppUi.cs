using System.Text.RegularExpressions;
using System.Xml.Linq;
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

    [Fact]
    public void While_the_system_mode_is_in_effect_a_windows_appearance_change_redraws_the_ui()
    {
        var appearance = new UiAppearance(UiMode.System, AppTheme.Light);
        int redrawn = 0;
        appearance.ThemeChanged += (_, _) => redrawn++;

        appearance.SystemThemeChanged(AppTheme.Dark);

        Assert.Equal(AppTheme.Dark, appearance.Theme);
        Assert.Equal(1, redrawn);

        appearance.SystemThemeChanged(AppTheme.Light);

        Assert.Equal(AppTheme.Light, appearance.Theme);
        Assert.Equal(2, redrawn);
    }

    [Theory]
    [InlineData(UiMode.Light)]
    [InlineData(UiMode.Dark)]
    public void A_windows_appearance_change_does_not_affect_the_light_or_dark_mode(UiMode mode)
    {
        var appearance = new UiAppearance(mode, AppTheme.Light);
        AppTheme drawn = appearance.Theme;
        int redrawn = 0;
        appearance.ThemeChanged += (_, _) => redrawn++;

        appearance.SystemThemeChanged(AppTheme.Dark);
        appearance.SystemThemeChanged(AppTheme.Light);

        Assert.Equal(drawn, appearance.Theme);
        Assert.Equal(0, redrawn);
    }

    [Fact]
    public void Switching_to_the_system_mode_draws_what_windows_uses_now()
    {
        var appearance = new UiAppearance(UiMode.Light, AppTheme.Light);
        appearance.SystemThemeChanged(AppTheme.Dark);
        int redrawn = 0;
        appearance.ThemeChanged += (_, _) => redrawn++;

        appearance.Apply(UiMode.System);

        Assert.Equal(AppTheme.Dark, appearance.Theme);
        Assert.Equal(1, redrawn);
    }

    [Theory]
    [InlineData("PreviewWindow")]
    [InlineData("EditorWindow")]
    [InlineData("SettingsWindow")]
    [InlineData("RegionSelectionWindow")]
    public void Screens_the_app_draws_take_their_colours_from_the_ui_mode(string window)
    {
        XDocument markup = Markup(window + ".xaml");

        Assert.Contains("{DynamicResource", (string?)markup.Root!.Attribute("Background"));
        foreach (string key in ThemeKeysUsedBy(markup))
        {
            Assert.Contains(key, ThemeKeys("Light"));
            Assert.Contains(key, ThemeKeys("Dark"));
        }
    }

    [Fact]
    public void The_region_selection_overlay_draws_its_selection_in_the_ui_mode()
    {
        XElement selection = Assert.Single(Markup("RegionSelectionWindow.xaml").Descendants(), element =>
            element.Name.LocalName == "Rectangle");

        Assert.Contains("{DynamicResource", (string?)selection.Attribute("Stroke"));
        Assert.Contains("{DynamicResource", (string?)selection.Attribute("Fill"));
    }

    [Fact]
    public void The_screenshot_shown_in_the_overlay_is_not_recoloured()
    {
        XElement image = Assert.Single(Markup("RegionSelectionWindow.xaml").Descendants(), element =>
            element.Name.LocalName == "Image");

        Assert.DoesNotContain(image.Attributes(), attribute =>
            attribute.Name.LocalName is "Opacity" or "OpacityMask" or "Effect" ||
            attribute.Value.Contains("DynamicResource"));
        Assert.DoesNotContain(image.Elements(), element => element.Name.LocalName.StartsWith("Image."));
    }

    [Fact]
    public void Light_and_dark_define_the_same_colours()
    {
        Assert.NotEmpty(ThemeKeys("Light"));
        Assert.Equal(ThemeKeys("Light").Order(), ThemeKeys("Dark").Order());
    }

    private static XDocument Markup(string file) =>
        XDocument.Load(Path.Combine(AppContext.BaseDirectory, "MenuExamMarkup", file));

    private static IEnumerable<string> ThemeKeysUsedBy(XDocument markup) =>
        markup.Descendants().SelectMany(element => element.Attributes())
            .Select(attribute => Regex.Match(attribute.Value, @"\{DynamicResource\s+(\w+)\}"))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value);

    private static string[] ThemeKeys(string theme)
    {
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        return Markup(Path.Combine("Themes", theme + ".xaml")).Root!.Elements()
            .Select(element => (string?)element.Attribute(x + "Key"))
            .OfType<string>()
            .ToArray();
    }
}
