using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `tray-icon` (ceps/cases/tray-icon.md).
public class TrayIconExam
{
    [Fact]
    public void While_running_the_application_displays_the_tray_icon()
    {
        TrayIcon tray = TrayIcon.CreateDefault(exit: () => { }, openSettings: () => { });

        Assert.False(tray.IsVisible);

        tray.Show();

        Assert.True(tray.IsVisible);
    }

    [Fact]
    public void The_tray_icon_has_a_context_menu()
    {
        TrayIcon tray = TrayIcon.CreateDefault(exit: () => { }, openSettings: () => { });

        Assert.NotEmpty(tray.ContextMenu);
    }

    [Fact]
    public void A_tray_icon_cannot_be_created_without_a_context_menu()
    {
        Assert.Throws<ArgumentException>(() => new TrayIcon(Array.Empty<TrayMenuItem>()));
    }

    [Fact]
    public void The_context_menu_includes_an_Exit_command_that_shuts_the_app_down()
    {
        var exited = false;
        TrayIcon tray = TrayIcon.CreateDefault(exit: () => exited = true, openSettings: () => { });

        TrayMenuItem only = Assert.Single(tray.ContextMenu, item => item.Text == "Exit");
        Assert.Equal("Exit", only.Text);

        only.Invoke();

        Assert.True(exited);
    }
}
