namespace Eikonelle;

/// <summary>
/// Modifier keys held together with a key to form a <see cref="Hotkey"/>.
/// Values match the Win32 <c>MOD_*</c> flags accepted by <c>RegisterHotKey</c>.
/// </summary>
[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8,
}

/// <summary>
/// A key combination that triggers a screenshot.
/// </summary>
public readonly record struct Hotkey(HotkeyModifiers Modifiers, uint VirtualKey)
{
    /// <summary>
    /// The fixed, non-configurable combination that takes a screenshot: <c>Ctrl+Shift+S</c>.
    /// </summary>
    public static Hotkey Capture { get; } =
        new(HotkeyModifiers.Control | HotkeyModifiers.Shift, 'S');
}
