using System.Drawing;
using System.Windows.Forms;
using ResourceDictionary = System.Windows.ResourceDictionary;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Eikonelle;

/// <summary>
/// Renders a <see cref="TrayIcon"/> model in the Windows notification area with a
/// WinForms <see cref="NotifyIcon"/>, mirroring its visibility and context menu. The
/// menu is drawn with the brushes of the application's current theme.
/// </summary>
public sealed class TrayIconShell : IDisposable
{
    private readonly TrayIcon _model;
    private readonly NotifyIcon _icon;
    private readonly ContextMenuStrip _menu;

    public TrayIconShell(TrayIcon model)
    {
        _model = model;

        _menu = new ContextMenuStrip();
        foreach (TrayMenuItem item in model.ContextMenu)
        {
            TrayMenuItem captured = item;
            _menu.Items.Add(captured.Text, image: null, (_, _) => captured.Invoke());
        }

        _icon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Eikonelle",
            ContextMenuStrip = _menu,
            Visible = _model.IsVisible,
        };
    }

    /// <summary>Show the icon in the tray. Called once the application is running.</summary>
    public void Show()
    {
        _model.Show();
        _icon.Visible = _model.IsVisible;
    }

    /// <summary>Draw the context menu with the theme brushes found in <paramref name="resources"/>.</summary>
    public void UseColors(ResourceDictionary resources)
    {
        var colors = new TrayMenuColors(
            Brush(resources, "UiSurfaceBrush"),
            Brush(resources, "UiForegroundBrush"),
            Brush(resources, "UiControlHoverBrush"),
            Brush(resources, "UiControlBorderBrush"));
        _menu.Renderer = new ToolStripProfessionalRenderer(colors) { RoundedEdges = false };
        _menu.BackColor = colors.ToolStripDropDownBackground;
        _menu.ForeColor = colors.Foreground;
        foreach (ToolStripItem item in _menu.Items)
        {
            item.ForeColor = colors.Foreground;
        }
    }

    private static Color Brush(ResourceDictionary resources, string key)
    {
        System.Windows.Media.Color color = ((SolidColorBrush)resources[key]).Color;
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public void Dispose()
    {
        _model.Hide();
        _icon.Visible = _model.IsVisible;
        _icon.Dispose();
        _menu.Dispose();
    }

    /// <summary>The theme colours the context menu is drawn with.</summary>
    private sealed class TrayMenuColors(Color background, Color foreground, Color highlight, Color border)
        : ProfessionalColorTable
    {
        public Color Foreground { get; } = foreground;

        public override Color ToolStripDropDownBackground => background;
        public override Color ImageMarginGradientBegin => background;
        public override Color ImageMarginGradientMiddle => background;
        public override Color ImageMarginGradientEnd => background;
        public override Color MenuBorder => border;
        public override Color MenuItemBorder => highlight;
        public override Color MenuItemSelected => highlight;
        public override Color MenuItemSelectedGradientBegin => highlight;
        public override Color MenuItemSelectedGradientEnd => highlight;
        public override Color MenuItemPressedGradientBegin => highlight;
        public override Color MenuItemPressedGradientEnd => highlight;
        public override Color SeparatorDark => border;
        public override Color SeparatorLight => background;
    }
}
