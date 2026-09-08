using System.Drawing;
using System.Windows.Forms;

namespace Eikonelle;

/// <summary>
/// Renders a <see cref="TrayIcon"/> model in the Windows notification area with a
/// WinForms <see cref="NotifyIcon"/>, mirroring its visibility and context menu.
/// </summary>
public sealed class TrayIconShell : IDisposable
{
    private readonly TrayIcon _model;
    private readonly NotifyIcon _icon;

    public TrayIconShell(TrayIcon model)
    {
        _model = model;

        var menu = new ContextMenuStrip();
        foreach (TrayMenuItem item in model.ContextMenu)
        {
            TrayMenuItem captured = item;
            menu.Items.Add(captured.Text, image: null, (_, _) => captured.Invoke());
        }

        _icon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Eikonelle",
            ContextMenuStrip = menu,
            Visible = _model.IsVisible,
        };
    }

    /// <summary>Show the icon in the tray. Called once the application is running.</summary>
    public void Show()
    {
        _model.Show();
        _icon.Visible = _model.IsVisible;
    }

    public void Dispose()
    {
        _model.Hide();
        _icon.Visible = _model.IsVisible;
        _icon.Dispose();
    }
}
