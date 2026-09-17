namespace Eikonelle;

/// <summary>One entry in the application menu: a label and the action taken when it
/// is chosen.</summary>
public sealed class AppMenuItem
{
    private readonly Action _invoke;

    public AppMenuItem(string text, Action invoke)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        ArgumentNullException.ThrowIfNull(invoke);
        Text = text;
        _invoke = invoke;
    }

    /// <summary>The label shown in the menu.</summary>
    public string Text { get; }

    /// <summary>Run this item's action, as if the user had chosen it.</summary>
    public void Invoke() => _invoke();
}

/// <summary>
/// The menu every application screen shows. The items available vary by screen, so
/// each screen builds its own. Headless model; the WPF shell renders it.
/// </summary>
public sealed class AppMenu
{
    public AppMenu(IReadOnlyList<AppMenuItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            throw new ArgumentException("Every screen shows a menu with items.", nameof(items));
        }

        Items = items;
    }

    /// <summary>The items this screen offers, in display order. Never empty.</summary>
    public IReadOnlyList<AppMenuItem> Items { get; }

    /// <summary>The preview screen: opening the editor and the settings window.</summary>
    public static AppMenu ForPreview(Action openEditor, Action openSettings)
    {
        ArgumentNullException.ThrowIfNull(openEditor);
        ArgumentNullException.ThrowIfNull(openSettings);
        return new AppMenu(new[]
        {
            new AppMenuItem("Editor", openEditor),
            new AppMenuItem("Settings", openSettings),
        });
    }

    /// <summary>The editor screen: applying or cancelling the editing session.</summary>
    public static AppMenu ForEditor(Action apply, Action cancel) => ApplyCancel(apply, cancel);

    /// <summary>The settings screen: applying or cancelling the pending settings.</summary>
    public static AppMenu ForSettings(Action apply, Action cancel) => ApplyCancel(apply, cancel);

    private static AppMenu ApplyCancel(Action apply, Action cancel)
    {
        ArgumentNullException.ThrowIfNull(apply);
        ArgumentNullException.ThrowIfNull(cancel);
        return new AppMenu(new[]
        {
            new AppMenuItem("Apply", apply),
            new AppMenuItem("Cancel", cancel),
        });
    }
}
