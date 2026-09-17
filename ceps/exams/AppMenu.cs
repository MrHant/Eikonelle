using System.Xml.Linq;
using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `app_menu` (ceps/cases/app_menu.md).
public class AppMenuExam
{
    [Fact]
    public void A_menu_cannot_be_empty()
    {
        Assert.Throws<ArgumentException>(() => new AppMenu(Array.Empty<AppMenuItem>()));
    }

    [Fact]
    public void The_preview_screen_offers_the_editor_and_the_settings_window()
    {
        AppMenu menu = AppMenu.ForPreview(() => { }, () => { });

        Assert.Equal(new[] { "Editor", "Settings" }, menu.Items.Select(item => item.Text));
        Assert.Single(WindowMarkup("PreviewWindow").Descendants(), element => element.Name.LocalName == "AppMenuBar");
    }

    [Theory]
    [InlineData("EditorWindow")]
    [InlineData("SettingsWindow")]
    public void Editor_and_settings_have_no_menu_and_keep_their_action_buttons(string window)
    {
        XDocument markup = WindowMarkup(window);

        Assert.DoesNotContain(markup.Descendants(), element =>
            element.Name.LocalName is "AppMenuBar" or "Menu" or "MenuItem");
        foreach (string action in new[] { "Apply", "Cancel" })
        {
            Assert.Single(markup.Descendants(), element =>
                element.Name.LocalName == "Button" &&
                (string?)element.Attribute("Click") == $"{action}_Click");
        }
    }

    [Fact]
    public void Choosing_a_preview_item_runs_its_command()
    {
        var chosen = new List<string>();
        AppMenu menu = AppMenu.ForPreview(() => chosen.Add("editor"), () => chosen.Add("settings"));

        Assert.Single(menu.Items, item => item.Text == "Editor").Invoke();
        Assert.Single(menu.Items, item => item.Text == "Settings").Invoke();

        Assert.Equal(new[] { "editor", "settings" }, chosen);
    }

    private static XDocument WindowMarkup(string window) =>
        XDocument.Load(Path.Combine(AppContext.BaseDirectory, "MenuExamMarkup", $"{window}.xaml"));
}
