using System.Xml.Linq;
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
        var session = new SettingsSession(settings, values => saved = values.Hotkey);
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
        var session = new SettingsSession(new Settings(_ => true), values =>
        {
            if (shouldFail) throw error;
            saved = values.Hotkey;
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
    public void The_main_menu_opens_settings()
    {
        bool opened = false;
        AppMenu menu = AppMenu.ForPreview(openEditor: () => { }, openSettings: () => opened = true);

        Assert.Single(menu.Items, item => item.Text == "Settings").Invoke();

        Assert.True(opened);
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
            Assert.Equal(Hotkey.Capture, store.Load().Hotkey);
            var selected = new Hotkey(HotkeyModifiers.Alt, 'P');
            var session = new SettingsSession(new Settings(_ => true), store.Save)
            {
                SelectedHotkey = selected,
            };
            Assert.True(session.Apply());

            var settings = new Settings(_ => true, new SettingsStore(path).Load().Hotkey);
            Assert.Equal(selected, settings.ScreenshotHotkey);

            store.Save(StoredSettings.Default);
            Assert.Equal(Hotkey.Capture, new SettingsStore(path).Load().Hotkey);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(CaptureMode.Region)]
    [InlineData(CaptureMode.FullScreen)]
    public void Applying_makes_the_selected_capture_mode_active_and_persists_it(CaptureMode mode)
    {
        StoredSettings? saved = null;
        CaptureMode initial = mode == CaptureMode.Region ? CaptureMode.FullScreen : CaptureMode.Region;
        var settings = new Settings(_ => true, null, initial);
        var session = new SettingsSession(settings, values => saved = values);
        session.SelectedCaptureMode = mode;

        Assert.True(session.Apply());

        Assert.Equal(mode, settings.CaptureMode);
        Assert.Equal(new StoredSettings(Hotkey.Capture, mode), saved);
    }

    [Fact]
    public void A_pending_capture_mode_is_discarded_on_cancel_without_saving()
    {
        var settings = new Settings(_ => true);
        var session = new SettingsSession(settings, _ => throw new Exception("Must not save before Apply."));
        session.SelectedCaptureMode = CaptureMode.Region;

        Assert.Equal(CaptureMode.FullScreen, settings.CaptureMode);
        session.Cancel();

        Assert.Equal(CaptureMode.FullScreen, session.SelectedCaptureMode);
        Assert.Equal(CaptureMode.FullScreen, new SettingsSession(settings, _ => { }).SelectedCaptureMode);
    }

    [Fact]
    public void The_capture_mode_is_remembered_across_settings_instances()
    {
        string directory = Path.Combine(Path.GetTempPath(), "Eikonelle-exams-" + Guid.NewGuid());
        string path = Path.Combine(directory, "settings.json");
        try
        {
            var hotkey = new Hotkey(HotkeyModifiers.Alt, 'P');
            var session = new SettingsSession(new Settings(_ => true), new SettingsStore(path).Save)
            {
                SelectedHotkey = hotkey,
                SelectedCaptureMode = CaptureMode.Region,
            };
            Assert.True(session.Apply());

            StoredSettings loaded = new SettingsStore(path).Load();
            var settings = new Settings(_ => true, loaded.Hotkey, loaded.CaptureMode);

            Assert.Equal(CaptureMode.Region, settings.CaptureMode);
            Assert.Equal(hotkey, settings.ScreenshotHotkey);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(UiMode.Light)]
    [InlineData(UiMode.Dark)]
    [InlineData(UiMode.System)]
    public void Applying_makes_the_selected_ui_mode_active_and_persists_it(UiMode mode)
    {
        StoredSettings? saved = null;
        UiMode initial = mode == UiMode.Dark ? UiMode.Light : UiMode.Dark;
        var settings = new Settings(_ => true, null, CaptureMode.FullScreen, initial);
        var session = new SettingsSession(settings, values => saved = values);
        session.SelectedUiMode = mode;

        Assert.True(session.Apply());

        Assert.Equal(mode, settings.UiMode);
        Assert.Equal(new StoredSettings(Hotkey.Capture, CaptureMode.FullScreen, mode), saved);
    }

    [Fact]
    public void A_pending_ui_mode_is_discarded_on_cancel_without_saving()
    {
        var settings = new Settings(_ => true);
        var session = new SettingsSession(settings, _ => throw new Exception("Must not save before Apply."));
        session.SelectedUiMode = UiMode.Dark;

        Assert.Equal(UiMode.System, settings.UiMode);
        session.Cancel();

        Assert.Equal(UiMode.System, session.SelectedUiMode);
        Assert.Equal(UiMode.System, new SettingsSession(settings, _ => { }).SelectedUiMode);
    }

    [Fact]
    public void An_unavailable_hotkey_leaves_the_ui_mode_untouched()
    {
        var settings = new Settings(_ => false, null, CaptureMode.FullScreen, UiMode.Light);
        var session = new SettingsSession(settings, _ => throw new Exception("Must not save."));
        session.SelectedHotkey = new Hotkey(HotkeyModifiers.Alt, 'P');
        session.SelectedUiMode = UiMode.Dark;

        Assert.False(session.Apply());

        Assert.Equal(UiMode.Light, settings.UiMode);
    }

    [Fact]
    public void The_ui_mode_is_remembered_across_settings_instances()
    {
        string directory = Path.Combine(Path.GetTempPath(), "Eikonelle-exams-" + Guid.NewGuid());
        string path = Path.Combine(directory, "settings.json");
        try
        {
            var session = new SettingsSession(new Settings(_ => true), new SettingsStore(path).Save)
            {
                SelectedCaptureMode = CaptureMode.Region,
                SelectedUiMode = UiMode.Dark,
            };
            Assert.True(session.Apply());

            StoredSettings loaded = new SettingsStore(path).Load();
            var settings = new Settings(_ => true, loaded.Hotkey, loaded.CaptureMode, loaded.UiMode);

            Assert.Equal(UiMode.Dark, settings.UiMode);
            Assert.Equal(CaptureMode.Region, settings.CaptureMode);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void A_settings_file_written_before_ui_modes_existed_uses_the_default()
    {
        string directory = Path.Combine(Path.GetTempPath(), "Eikonelle-exams-" + Guid.NewGuid());
        string path = Path.Combine(directory, "settings.json");
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, """
                {"Hotkey":{"Modifiers":"Alt","VirtualKey":80},"CaptureMode":"Region"}
                """);

            StoredSettings loaded = new SettingsStore(path).Load();

            Assert.Equal(new Hotkey(HotkeyModifiers.Alt, 'P'), loaded.Hotkey);
            Assert.Equal(CaptureMode.Region, loaded.CaptureMode);
            Assert.Equal(UiMode.System, loaded.UiMode);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void An_error_while_loading_the_settings_uses_the_defaults()
    {
        string directory = Path.Combine(Path.GetTempPath(), "Eikonelle-exams-" + Guid.NewGuid());
        string path = Path.Combine(directory, "settings.json");
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, """
                {"Hotkey":{"Modifiers":"Alt","VirtualKey":80},"CaptureMode":"Region","UiMode":"Dark"}
                """);

            StoredSettings loaded;
            IReadOnlyList<string> problems;
            using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                loaded = new SettingsStore(path).Load(out problems);
            }

            Assert.Equal(StoredSettings.Default, loaded);
            Assert.NotEmpty(problems);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void A_persistence_error_for_the_capture_mode_is_reported_and_can_be_retried()
    {
        bool shouldFail = true;
        StoredSettings? saved = null;
        var session = new SettingsSession(new Settings(_ => true), values =>
        {
            if (shouldFail) throw new IOException("Disk is full.");
            saved = values;
        });
        session.SelectedCaptureMode = CaptureMode.Region;

        Assert.False(session.Apply());
        Assert.Contains("Disk is full.", session.Message);
        Assert.Equal(CaptureMode.Region, session.SelectedCaptureMode);

        shouldFail = false;
        Assert.True(session.Apply());
        Assert.Equal(CaptureMode.Region, saved?.CaptureMode);
    }

    [Fact]
    public void The_save_folder_path_is_displayed_with_a_button_for_the_folder_picker()
    {
        XDocument markup = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "MenuExamMarkup", "SettingsWindow.xaml"));
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement display = Assert.Single(markup.Descendants(), element =>
            (string?)element.Attribute(x + "Name") == "SaveFolderDisplay");
        Assert.Equal("TextBox", display.Name.LocalName);
        Assert.Equal("True", (string?)display.Attribute("IsReadOnly"));
        Assert.Single(markup.Descendants(), element =>
            element.Name.LocalName == "Button" &&
            (string?)element.Attribute("Click") == "BrowseSaveFolder_Click");
    }

    [Fact]
    public void Applying_makes_the_selected_save_folder_active_and_persists_it()
    {
        StoredSettings? saved = null;
        var settings = new Settings(_ => true);
        var session = new SettingsSession(settings, values => saved = values);
        string folder = Path.Combine(Path.GetTempPath(), "Eikonelle-chosen");
        session.SelectedSaveFolder = folder;

        Assert.True(session.Apply());

        Assert.Equal(folder, settings.SaveFolder);
        Assert.Equal(folder, saved?.SaveFolder);
    }

    [Fact]
    public void A_pending_save_folder_is_discarded_on_cancel_without_saving()
    {
        var settings = new Settings(_ => true);
        string original = settings.SaveFolder;
        var session = new SettingsSession(settings, _ => throw new Exception("Must not save before Apply."));
        session.SelectedSaveFolder = Path.Combine(Path.GetTempPath(), "Eikonelle-pending");

        Assert.Equal(original, settings.SaveFolder);
        session.Cancel();

        Assert.Equal(original, session.SelectedSaveFolder);
        Assert.Equal(original, new SettingsSession(settings, _ => { }).SelectedSaveFolder);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Screenshots")]
    [InlineData("C:Screenshots")]
    [InlineData("C:\\Screen|shots")]
    public void An_invalid_save_folder_is_reported_and_nothing_is_applied(string folder)
    {
        var settings = new Settings(_ => throw new Exception("Must not register."), null, CaptureMode.FullScreen, UiMode.Light);
        string original = settings.SaveFolder;
        var session = new SettingsSession(settings, _ => throw new Exception("Must not save."))
        {
            SelectedHotkey = new Hotkey(HotkeyModifiers.Alt, 'P'),
            SelectedUiMode = UiMode.Dark,
            SelectedSaveFolder = folder,
        };

        Assert.False(session.Apply());

        Assert.Contains("save folder", session.Message);
        Assert.Equal(original, settings.SaveFolder);
        Assert.Equal(UiMode.Light, settings.UiMode);
        Assert.Equal(Hotkey.Capture, settings.ScreenshotHotkey);
    }

    [Fact]
    public void The_save_folder_is_remembered_across_settings_instances()
    {
        string directory = Path.Combine(Path.GetTempPath(), "Eikonelle-exams-" + Guid.NewGuid());
        string path = Path.Combine(directory, "settings.json");
        string folder = Path.Combine(directory, "Screenshots");
        try
        {
            var session = new SettingsSession(new Settings(_ => true), new SettingsStore(path).Save)
            {
                SelectedUiMode = UiMode.Dark,
                SelectedSaveFolder = folder,
            };
            Assert.True(session.Apply());

            StoredSettings loaded = new SettingsStore(path).Load();
            var settings = new Settings(_ => true, loaded.Hotkey, loaded.CaptureMode, loaded.UiMode, loaded.SaveFolder);

            Assert.Equal(folder, settings.SaveFolder);
            Assert.Equal(UiMode.Dark, settings.UiMode);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void A_settings_file_written_before_save_folders_existed_uses_the_default()
    {
        string directory = Path.Combine(Path.GetTempPath(), "Eikonelle-exams-" + Guid.NewGuid());
        string path = Path.Combine(directory, "settings.json");
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, """
                {"Hotkey":{"Modifiers":"Alt","VirtualKey":80},"CaptureMode":"Region","UiMode":"Dark"}
                """);

            StoredSettings loaded = new SettingsStore(path).Load();

            Assert.Equal(new Hotkey(HotkeyModifiers.Alt, 'P'), loaded.Hotkey);
            Assert.Equal(UiMode.Dark, loaded.UiMode);
            Assert.Equal(ScreenshotFolder.Default, loaded.SaveFolder);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
