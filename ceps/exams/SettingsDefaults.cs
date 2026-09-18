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

        Hotkey loaded = new SettingsStore(path).Load().Hotkey;

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

    [Fact]
    public void Unconfigured_settings_use_the_Full_Screen_capture_mode()
    {
        var settings = new Settings(_ => true);
        var session = new SettingsSession(settings, _ => { });
        string path = Path.Combine(Path.GetTempPath(), "Eikonelle-defaults-" + Guid.NewGuid(), "settings.json");

        Assert.Equal(CaptureMode.FullScreen, settings.CaptureMode);
        Assert.Equal(CaptureMode.FullScreen, session.SelectedCaptureMode);
        Assert.Equal(CaptureMode.FullScreen, new SettingsStore(path).Load().CaptureMode);
    }

    [Fact]
    public void A_configured_capture_mode_is_not_replaced_by_the_default()
    {
        var settings = new Settings(_ => true, null, CaptureMode.Region);

        Assert.Equal(CaptureMode.Region, settings.CaptureMode);
        Assert.Equal(CaptureMode.Region, new SettingsSession(settings, _ => { }).SelectedCaptureMode);
    }

    [Fact]
    public void Unconfigured_settings_use_the_System_ui_mode()
    {
        var settings = new Settings(_ => true);
        var session = new SettingsSession(settings, _ => { });
        string path = Path.Combine(Path.GetTempPath(), "Eikonelle-defaults-" + Guid.NewGuid(), "settings.json");

        Assert.Equal(UiMode.System, settings.UiMode);
        Assert.Equal(UiMode.System, session.SelectedUiMode);
        Assert.Equal(UiMode.System, new SettingsStore(path).Load().UiMode);
    }

    [Fact]
    public void A_configured_ui_mode_is_not_replaced_by_the_default()
    {
        var settings = new Settings(_ => true, null, CaptureMode.FullScreen, UiMode.Dark);

        Assert.Equal(UiMode.Dark, settings.UiMode);
        Assert.Equal(UiMode.Dark, new SettingsSession(settings, _ => { }).SelectedUiMode);
    }

    [Fact]
    public void Unconfigured_settings_save_into_Documents_Eikonelle()
    {
        string expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Eikonelle");
        var settings = new Settings(_ => true);
        string path = Path.Combine(Path.GetTempPath(), "Eikonelle-defaults-" + Guid.NewGuid(), "settings.json");

        Assert.Equal(expected, settings.SaveFolder);
        Assert.Equal(expected, new SettingsSession(settings, _ => { }).SelectedSaveFolder);
        Assert.Equal(expected, new SettingsStore(path).Load().SaveFolder);
    }

    [Fact]
    public void A_configured_save_folder_is_not_replaced_by_the_default()
    {
        string configured = Path.Combine(Path.GetTempPath(), "Eikonelle-configured");
        var settings = new Settings(_ => true, null, CaptureMode.FullScreen, UiMode.System, configured);

        Assert.Equal(configured, settings.SaveFolder);
        Assert.Equal(configured, new SettingsSession(settings, _ => { }).SelectedSaveFolder);
    }

    private static readonly StoredSettings Configured = new(
        new Hotkey(HotkeyModifiers.Alt, 'P'), CaptureMode.Region, UiMode.Dark, @"D:\Shots");

    private const string ConfiguredHotkeyJson = """{"Modifiers":"Alt","VirtualKey":80}""";

    public static TheoryData<string, StoredSettings> MissingSettings => new()
    {
        { $$"""{"CaptureMode":"Region","UiMode":"Dark","SaveFolder":"D:\\Shots"}""", Configured with { Hotkey = Hotkey.Capture } },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"UiMode":"Dark","SaveFolder":"D:\\Shots"}""", Configured with { CaptureMode = CaptureMode.FullScreen } },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"CaptureMode":"Region","SaveFolder":"D:\\Shots"}""", Configured with { UiMode = UiMode.System } },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"CaptureMode":"Region","UiMode":"Dark"}""", Configured with { SaveFolder = ScreenshotFolder.Default } },
        { "{}", StoredSettings.Default },
    };

    [Theory]
    [MemberData(nameof(MissingSettings))]
    public void Each_setting_missing_from_the_file_uses_its_default_and_keeps_the_others(string json, StoredSettings expected)
    {
        StoredSettings loaded = LoadFile(json, out IReadOnlyList<string> problems);

        Assert.Equal(expected, loaded);
        Assert.Empty(problems);
    }

    public static TheoryData<string, StoredSettings, string> InvalidSettings => new()
    {
        { $$"""{"Hotkey":{"Modifiers":"Control","VirtualKey":17},"CaptureMode":"Region","UiMode":"Dark","SaveFolder":"D:\\Shots"}""", Configured with { Hotkey = Hotkey.Capture }, "hotkey" },
        { $$"""{"Hotkey":"Ctrl+P","CaptureMode":"Region","UiMode":"Dark","SaveFolder":"D:\\Shots"}""", Configured with { Hotkey = Hotkey.Capture }, "hotkey" },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"CaptureMode":"Window","UiMode":"Dark","SaveFolder":"D:\\Shots"}""", Configured with { CaptureMode = CaptureMode.FullScreen }, "capture mode" },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"CaptureMode":7,"UiMode":"Dark","SaveFolder":"D:\\Shots"}""", Configured with { CaptureMode = CaptureMode.FullScreen }, "capture mode" },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"CaptureMode":"Region","UiMode":"Sepia","SaveFolder":"D:\\Shots"}""", Configured with { UiMode = UiMode.System }, "UI mode" },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"CaptureMode":"Region","UiMode":42,"SaveFolder":"D:\\Shots"}""", Configured with { UiMode = UiMode.System }, "UI mode" },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"CaptureMode":"Region","UiMode":"Dark","SaveFolder":"Shots"}""", Configured with { SaveFolder = ScreenshotFolder.Default }, "save folder" },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"CaptureMode":"Region","UiMode":"Dark","SaveFolder":null}""", Configured with { SaveFolder = ScreenshotFolder.Default }, "save folder" },
        { $$"""{"Hotkey":{{ConfiguredHotkeyJson}},"CaptureMode":"Region","UiMode":"Dark","SaveFolder":12}""", Configured with { SaveFolder = ScreenshotFolder.Default }, "save folder" },
    };

    [Theory]
    [MemberData(nameof(InvalidSettings))]
    public void Each_invalid_setting_in_the_file_uses_its_default_and_keeps_the_others(
        string json, StoredSettings expected, string reported)
    {
        StoredSettings loaded = LoadFile(json, out IReadOnlyList<string> problems);

        Assert.Equal(expected, loaded);
        Assert.Contains(reported, Assert.Single(problems));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[1, 2]")]
    [InlineData("")]
    public void An_unreadable_settings_file_uses_all_defaults(string content)
    {
        StoredSettings loaded = LoadFile(content, out IReadOnlyList<string> problems);

        Assert.Equal(StoredSettings.Default, loaded);
        Assert.NotEmpty(problems);
    }

    [Fact]
    public void Loaded_defaults_are_the_settings_in_effect()
    {
        StoredSettings loaded = LoadFile("""{"Hotkey":"broken","CaptureMode":"broken","UiMode":"broken","SaveFolder":"broken"}""", out _);
        var settings = new Settings(_ => true, loaded.Hotkey, loaded.CaptureMode, loaded.UiMode, loaded.SaveFolder);

        Assert.Equal(Hotkey.Capture, settings.ScreenshotHotkey);
        Assert.Equal(CaptureMode.FullScreen, settings.CaptureMode);
        Assert.Equal(UiMode.System, settings.UiMode);
        Assert.Equal(ScreenshotFolder.Default, settings.SaveFolder);
    }

    private static StoredSettings LoadFile(string content, out IReadOnlyList<string> problems)
    {
        string directory = Path.Combine(Path.GetTempPath(), "Eikonelle-defaults-" + Guid.NewGuid());
        string path = Path.Combine(directory, "settings.json");
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, content);
            return new SettingsStore(path).Load(out problems);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
