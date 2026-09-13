using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `settings` (ceps/cases/settings.md).
public class SettingsExam
{
    [Fact]
    public void Pending_changes_are_discarded_on_cancel_without_registering_or_saving()
    {
        var settings = new Settings(_ => throw new Exception("Must not register before Apply."));
        var session = new SettingsSession(settings, _ => throw new Exception("Must not save before Apply."));
        session.SelectedHotkey = new Hotkey(HotkeyModifiers.Alt, 'P');

        Assert.Equal(Hotkey.Capture, settings.ScreenshotHotkey);
        session.Cancel();

        Assert.Equal(Hotkey.Capture, session.SelectedHotkey);
        Assert.Equal(Hotkey.Capture, new SettingsSession(settings, _ => { }).SelectedHotkey);
    }

    [Fact]
    public void Cancel_discards_only_changes_made_since_the_last_apply()
    {
        Hotkey? saved = null;
        var settings = new Settings(_ => true);
        var session = new SettingsSession(settings, hotkey => saved = hotkey);
        var applied = new Hotkey(HotkeyModifiers.Alt, 'P');
        session.SelectedHotkey = applied;

        Assert.True(session.Apply());
        session.SelectedHotkey = new Hotkey(HotkeyModifiers.Alt, 'Q');
        session.Cancel();

        Assert.Equal(applied, settings.ScreenshotHotkey);
        Assert.Equal(applied, session.SelectedHotkey);
        Assert.Equal(applied, saved);
    }

    [Theory]
    [InlineData("io")]
    [InlineData("access")]
    [InlineData("unexpected")]
    public void Persistence_errors_are_reported_and_the_same_session_can_retry(string failure)
    {
        Exception error = failure switch
        {
            "io" => new IOException("Disk is full."),
            "access" => new UnauthorizedAccessException("Access denied."),
            _ => new InvalidOperationException("Unexpected persistence failure."),
        };
        bool shouldFail = true;
        Hotkey? saved = null;
        var session = new SettingsSession(new Settings(_ => true), hotkey =>
        {
            if (shouldFail) throw error;
            saved = hotkey;
        });
        var selected = new Hotkey(HotkeyModifiers.Alt, 'P');
        session.SelectedHotkey = selected;

        Assert.False(session.Apply());
        Assert.Contains(error.Message, session.Message);
        Assert.Equal(selected, session.SelectedHotkey);
        Assert.Null(saved);

        shouldFail = false;
        Assert.True(session.Apply());
        Assert.Equal(selected, saved);
        Assert.DoesNotContain(error.Message, session.Message);
    }

    [Fact]
    public void An_unavailable_hotkey_is_not_persisted()
    {
        var session = new SettingsSession(new Settings(_ => false), _ => throw new Exception("Must not save."));
        session.SelectedHotkey = new Hotkey(HotkeyModifiers.Alt, 'P');

        Assert.False(session.Apply());
        Assert.Contains("unavailable", session.Message);
    }

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
            var session = new SettingsSession(new Settings(_ => true), store.Save)
            {
                SelectedHotkey = selected,
            };
            Assert.True(session.Apply());

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
