using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `settings` (ceps/cases/settings.md).
public class SettingsExam
{
    [Fact]
    public void The_tray_menu_opens_settings()
    {
        bool opened = false;
        TrayIcon tray = TrayIcon.CreateDefault(() => { }, () => opened = true);

        Assert.Single(tray.ContextMenu, item => item.Text == "Settings").Invoke();

        Assert.True(opened);
    }

    [Fact]
    public void Changing_the_screenshot_hotkey_registers_the_selected_combination()
    {
        Hotkey? registered = null;
        var settings = new Settings(hotkey => { registered = hotkey; return true; });
        var selected = new Hotkey(HotkeyModifiers.Control | HotkeyModifiers.Alt, 'P');

        Assert.True(settings.TryChangeHotkey(selected));

        Assert.Equal(selected, registered);
        Assert.Equal(selected, settings.ScreenshotHotkey);
    }

    [Fact]
    public void An_unavailable_combination_preserves_the_active_hotkey()
    {
        var settings = new Settings(_ => false);

        Assert.False(settings.TryChangeHotkey(new Hotkey(HotkeyModifiers.Control, 'P')));
        Assert.Equal(Hotkey.Capture, settings.ScreenshotHotkey);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(0x10u)]
    [InlineData(0x11u)]
    [InlineData(0x12u)]
    [InlineData(0x5Bu)]
    [InlineData(255u)]
    public void Invalid_or_modifier_only_keys_do_not_replace_the_active_hotkey(uint key)
    {
        var settings = new Settings(_ => throw new Exception("Must not register an invalid key."));

        Assert.False(settings.TryChangeHotkey(new Hotkey(HotkeyModifiers.Control, key)));
        Assert.Equal(Hotkey.Capture, settings.ScreenshotHotkey);
    }

    [Fact]
    public void Applying_the_current_combination_does_not_reregister_it()
    {
        var settings = new Settings(_ => throw new Exception("Already registered."));

        Assert.True(settings.TryChangeHotkey(Hotkey.Capture));
    }

    [Fact]
    public void The_hotkey_is_remembered_across_settings_instances_and_can_be_replaced()
    {
        string directory = Path.Combine(Path.GetTempPath(), "Eikonelle-exams-" + Guid.NewGuid());
        string path = Path.Combine(directory, "settings.json");
        try
        {
            var store = new SettingsStore(path);
            Assert.Equal(Hotkey.Capture, store.Load());
            var selected = new Hotkey(HotkeyModifiers.Alt, 'P');
            store.Save(selected);

            var settings = new Settings(_ => true, new SettingsStore(path).Load());
            Assert.Equal(selected, settings.ScreenshotHotkey);

            store.Save(Hotkey.Capture);
            Assert.Equal(Hotkey.Capture, new SettingsStore(path).Load());
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
