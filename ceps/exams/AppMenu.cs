using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `app_menu` (ceps/cases/app_menu.md).
public class AppMenuExam
{
    [Fact]
    public void Every_screen_shows_a_menu()
    {
        Assert.NotEmpty(PreviewMenu().Items);
        Assert.NotEmpty(EditorMenu().Items);
        Assert.NotEmpty(SettingsMenu().Items);
    }

    [Fact]
    public void A_screen_menu_cannot_be_empty()
    {
        Assert.Throws<ArgumentException>(() => new AppMenu(Array.Empty<AppMenuItem>()));
    }

    [Fact]
    public void The_preview_screen_offers_the_editor_and_the_settings_window()
    {
        Assert.Equal(new[] { "Editor", "Settings" }, Texts(PreviewMenu()));
    }

    [Fact]
    public void The_editor_screen_offers_apply_and_cancel()
    {
        Assert.Equal(new[] { "Apply", "Cancel" }, Texts(EditorMenu()));
    }

    [Fact]
    public void The_settings_screen_offers_apply_and_cancel()
    {
        Assert.Equal(new[] { "Apply", "Cancel" }, Texts(SettingsMenu()));
    }

    [Fact]
    public void The_available_items_vary_between_screens()
    {
        Assert.NotEqual(Texts(PreviewMenu()), Texts(EditorMenu()));
    }

    [Fact]
    public void Choosing_a_preview_item_runs_its_command()
    {
        var chosen = new List<string>();
        AppMenu menu = AppMenu.ForPreview(() => chosen.Add("editor"), () => chosen.Add("settings"));

        Invoke(menu, "Editor");
        Invoke(menu, "Settings");

        Assert.Equal(new[] { "editor", "settings" }, chosen);
    }

    [Fact]
    public void Choosing_an_editor_item_runs_its_command()
    {
        var chosen = new List<string>();
        AppMenu menu = AppMenu.ForEditor(() => chosen.Add("apply"), () => chosen.Add("cancel"));

        Invoke(menu, "Apply");
        Invoke(menu, "Cancel");

        Assert.Equal(new[] { "apply", "cancel" }, chosen);
    }

    [Fact]
    public void Choosing_a_settings_item_runs_its_command()
    {
        var chosen = new List<string>();
        AppMenu menu = AppMenu.ForSettings(() => chosen.Add("apply"), () => chosen.Add("cancel"));

        Invoke(menu, "Apply");
        Invoke(menu, "Cancel");

        Assert.Equal(new[] { "apply", "cancel" }, chosen);
    }

    private static AppMenu PreviewMenu() => AppMenu.ForPreview(() => { }, () => { });

    private static AppMenu EditorMenu() => AppMenu.ForEditor(() => { }, () => { });

    private static AppMenu SettingsMenu() => AppMenu.ForSettings(() => { }, () => { });

    private static string[] Texts(AppMenu menu) => menu.Items.Select(item => item.Text).ToArray();

    private static void Invoke(AppMenu menu, string text) =>
        Assert.Single(menu.Items, item => item.Text == text).Invoke();
}
