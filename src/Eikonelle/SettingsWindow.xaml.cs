using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Eikonelle;

public partial class SettingsWindow : Window
{
    private readonly SettingsSession _session;

    public SettingsWindow(Settings settings, SettingsStore store)
    {
        InitializeComponent();
        _session = new SettingsSession(settings, store.Save);
        DisplayHotkey();
        FullScreenOption.IsChecked = _session.SelectedCaptureMode == CaptureMode.FullScreen;
        RegionOption.IsChecked = _session.SelectedCaptureMode == CaptureMode.Region;
    }

    private void CaptureMode_Checked(object sender, RoutedEventArgs e)
    {
        _session.SelectedCaptureMode = sender == RegionOption ? CaptureMode.Region : CaptureMode.FullScreen;
        StatusText.Text = "";
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
        _session.SelectedHotkey = new Hotkey(modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));
        StatusText.Text = "";
        DisplayHotkey();
    }

    private void DisplayHotkey()
    {
        var parts = new List<string>();
        Hotkey selected = _session.SelectedHotkey;
        if (selected.Modifiers.HasFlag(HotkeyModifiers.Control)) parts.Add("Ctrl");
        if (selected.Modifiers.HasFlag(HotkeyModifiers.Alt)) parts.Add("Alt");
        if (selected.Modifiers.HasFlag(HotkeyModifiers.Shift)) parts.Add("Shift");
        if (selected.Modifiers.HasFlag(HotkeyModifiers.Win)) parts.Add("Win");
        parts.Add(KeyInterop.KeyFromVirtualKey((int)selected.VirtualKey).ToString());
        HotkeyInput.Text = string.Join("+", parts);
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        _session.Apply();
        StatusText.Text = _session.Message;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        _session.Cancel();
        base.OnClosed(e);
    }
}
