namespace Eikonelle;

/// <summary>The screenshot hotkey currently registered by the application.</summary>
public sealed class Settings
{
    private readonly Func<Hotkey, bool> _register;

    public Settings(Func<Hotkey, bool> register, Hotkey? initialHotkey = null)
    {
        ArgumentNullException.ThrowIfNull(register);
        _register = register;
        ScreenshotHotkey = initialHotkey ?? Hotkey.Capture;
    }

    public Hotkey ScreenshotHotkey { get; private set; } = Hotkey.Capture;

    public bool TryChangeHotkey(Hotkey hotkey)
    {
        if (!IsValid(hotkey))
        {
            return false;
        }

        if (hotkey == ScreenshotHotkey)
        {
            return true;
        }

        if (!_register(hotkey))
        {
            return false;
        }

        ScreenshotHotkey = hotkey;
        return true;
    }

    public static bool IsValid(Hotkey hotkey) =>
        (hotkey.Modifiers & ~(HotkeyModifiers.Control | HotkeyModifiers.Alt |
            HotkeyModifiers.Shift | HotkeyModifiers.Win)) == 0 &&
        hotkey.VirtualKey is > 0 and < 255 and not (0x10 or 0x11 or 0x12 or
            0x5B or 0x5C or 0xA0 or 0xA1 or 0xA2 or 0xA3 or 0xA4 or 0xA5);
}
