using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `settings-defaults` (ceps/cases/settings-defaults.md).
public class SettingsDefaultsExam
{
    [Fact]
    public void Unconfigured_settings_use_Ctrl_Shift_S()
    {
        var settings = new Settings(_ => true);
        var session = new SettingsSession(settings, _ => { });

        var expected = new Hotkey(HotkeyModifiers.Control | HotkeyModifiers.Shift, 'S');
        Assert.Equal(expected, settings.ScreenshotHotkey);
        Assert.Equal(expected, session.SelectedHotkey);
    }

    [Fact]
    public void Missing_configuration_loads_defaults_without_creating_a_file()
    {
        string path = Path.Combine(Path.GetTempPath(), "Eikonelle-defaults-" + Guid.NewGuid(), "settings.json");

        Hotkey loaded = new SettingsStore(path).Load();

        Assert.Equal(new Hotkey(HotkeyModifiers.Control | HotkeyModifiers.Shift, 'S'), loaded);
        Assert.False(File.Exists(path));
        Assert.False(Directory.Exists(Path.GetDirectoryName(path)));
    }

    [Fact]
    public void A_configured_hotkey_is_not_replaced_by_the_default()
    {
        var configured = new Hotkey(HotkeyModifiers.Control | HotkeyModifiers.Alt, 'P');
        var settings = new Settings(_ => true, configured);

        Assert.Equal(configured, settings.ScreenshotHotkey);
        Assert.Equal(configured, new SettingsSession(settings, _ => { }).SelectedHotkey);
    }
}
