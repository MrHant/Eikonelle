namespace Eikonelle;

/// <summary>One entry in the <see cref="TrayIcon"/> context menu: a label and the
/// action taken when it is chosen.</summary>
public sealed class TrayMenuItem
{
    private readonly Action _invoke;

    public TrayMenuItem(string text, Action invoke)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        ArgumentNullException.ThrowIfNull(invoke);
        Text = text;
        _invoke = invoke;
    }

    /// <summary>The label shown in the context menu.</summary>
    public string Text { get; }

    /// <summary>Run this item's action, as if the user had clicked it.</summary>
    public void Invoke() => _invoke();
}

/// <summary>
/// The system-tray presence Eikonelle shows while it is running: an icon with a
/// context menu. Headless model; the WPF shell renders it and follows this state.
/// </summary>
public sealed class TrayIcon
{
    public TrayIcon(IReadOnlyList<TrayMenuItem> contextMenu)
    {
        ArgumentNullException.ThrowIfNull(contextMenu);
        if (contextMenu.Count == 0)
        {
            throw new ArgumentException("The tray icon must have a context menu.", nameof(contextMenu));
        }

        ContextMenu = contextMenu;
    }

    /// <summary>Whether the icon is currently shown in the tray.</summary>
    public bool IsVisible { get; private set; }

    /// <summary>The items shown when the icon is right-clicked. Never empty.</summary>
    public IReadOnlyList<TrayMenuItem> ContextMenu { get; }

    /// <summary>Show the icon in the tray. The application calls this on startup.</summary>
    public void Show() => IsVisible = true;

    /// <summary>Remove the icon from the tray. The application calls this on exit.</summary>
    public void Hide() => IsVisible = false;

    /// <summary>
    /// The application's settings and exit commands.
    /// </summary>
    public static TrayIcon CreateDefault(Action exit, Action openSettings)
    {
        ArgumentNullException.ThrowIfNull(exit);
        ArgumentNullException.ThrowIfNull(openSettings);
        return new TrayIcon(new[]
        {
            new TrayMenuItem("Settings", openSettings),
            new TrayMenuItem("Exit", exit),
        });
    }
}
