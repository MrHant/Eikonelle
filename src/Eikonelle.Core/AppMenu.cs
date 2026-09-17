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
/// The preview screen's application menu. Headless model; the WPF shell renders it.
/// </summary>
public sealed class AppMenu
{
    public AppMenu(IReadOnlyList<AppMenuItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            throw new ArgumentException("A menu must have items.", nameof(items));
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
}
