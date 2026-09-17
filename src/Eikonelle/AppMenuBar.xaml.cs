namespace Eikonelle;

/// <summary>Renders the <see cref="AppMenu"/> of the screen that hosts it.</summary>
public partial class AppMenuBar : System.Windows.Controls.UserControl
{
    public AppMenuBar() => InitializeComponent();

    /// <summary>Replace the rendered items with the given screen's menu.</summary>
    public void Show(AppMenu menu)
    {
        ArgumentNullException.ThrowIfNull(menu);
        MenuRoot.Items.Clear();
        foreach (AppMenuItem item in menu.Items)
        {
            var rendered = new System.Windows.Controls.MenuItem { Header = item.Text };
            rendered.Click += (_, _) => item.Invoke();
            MenuRoot.Items.Add(rendered);
        }
    }
}
