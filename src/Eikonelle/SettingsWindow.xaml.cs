using System.IO;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Eikonelle;

public partial class SettingsWindow : Window
{
    private readonly Settings _settings;
    private readonly SettingsStore _store;
    private Hotkey _selected;

    public SettingsWindow(Settings settings, SettingsStore store)
    {
        InitializeComponent();
        _settings = settings;
        _store = store;
        _selected = settings.ScreenshotHotkey;
        DisplayHotkey();
    }

    private void HotkeyInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.Tab)
        {
            return;
        }

        e.Handled = true;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or
            Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin or Key.None)
        {
            return;
        }

        var modifiers = HotkeyModifiers.None;
        ModifierKeys pressed = Keyboard.Modifiers;
        if (pressed.HasFlag(ModifierKeys.Control)) modifiers |= HotkeyModifiers.Control;
        if (pressed.HasFlag(ModifierKeys.Shift)) modifiers |= HotkeyModifiers.Shift;
        if (pressed.HasFlag(ModifierKeys.Alt)) modifiers |= HotkeyModifiers.Alt;
        if (pressed.HasFlag(ModifierKeys.Windows)) modifiers |= HotkeyModifiers.Win;
        _selected = new Hotkey(modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));
        StatusText.Text = "";
        DisplayHotkey();
    }

    private void DisplayHotkey()
    {
        var parts = new List<string>();
        if (_selected.Modifiers.HasFlag(HotkeyModifiers.Control)) parts.Add("Ctrl");
        if (_selected.Modifiers.HasFlag(HotkeyModifiers.Alt)) parts.Add("Alt");
        if (_selected.Modifiers.HasFlag(HotkeyModifiers.Shift)) parts.Add("Shift");
        if (_selected.Modifiers.HasFlag(HotkeyModifiers.Win)) parts.Add("Win");
        parts.Add(KeyInterop.KeyFromVirtualKey((int)_selected.VirtualKey).ToString());
        HotkeyInput.Text = string.Join("+", parts);
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (!_settings.TryChangeHotkey(_selected))
        {
            StatusText.Text = "This combination is invalid or unavailable. Choose another; your current hotkey is still active.";
            return;
        }

        try
        {
            _store.Save(_settings.ScreenshotHotkey);
            StatusText.Text = "Hotkey applied and saved.";
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            StatusText.Text = "The hotkey is active for this session, but could not be saved. " + error.Message;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
