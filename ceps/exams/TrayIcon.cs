using Eikonelle;

namespace Eikonelle.Exams;

// ceps exam for case `tray-icon` (ceps/cases/tray-icon.md).
//
// Case: "When application is running it should display a tray icon.
//        Tray icon should have a context menu."
//
// Clarified for this project: the context menu holds a single "Exit" item that
// shuts the application down; the tray icon has no left/double-click behaviour.
public class TrayIconExam
{
    [Fact]
    public void While_running_the_application_displays_the_tray_icon()
    {
        TrayIcon tray = TrayIcon.CreateDefault(exit: () => { });

        Assert.False(tray.IsVisible);

        tray.Show(); // what the application does on startup

        Assert.True(tray.IsVisible);
    }

    [Fact]
    public void The_tray_icon_has_a_context_menu()
    {
        TrayIcon tray = TrayIcon.CreateDefault(exit: () => { });

        Assert.NotEmpty(tray.ContextMenu);
    }

    [Fact]
    public void A_tray_icon_cannot_be_created_without_a_context_menu()
    {
        Assert.Throws<ArgumentException>(() => new TrayIcon(Array.Empty<TrayMenuItem>()));
    }

    [Fact]
    public void The_context_menu_is_a_single_Exit_command_that_shuts_the_app_down()
    {
        var exited = false;
        TrayIcon tray = TrayIcon.CreateDefault(exit: () => exited = true);

        TrayMenuItem only = Assert.Single(tray.ContextMenu);
        Assert.Equal("Exit", only.Text);

        only.Invoke();

        Assert.True(exited);
    }
}
