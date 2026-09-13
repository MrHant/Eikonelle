using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Eikonelle;

/// <summary>
/// Registers a system-wide <see cref="Hotkey"/> against a window's message loop and
/// raises <see cref="Pressed"/> whenever the combination is pressed.
/// </summary>
public sealed class HotkeyListener : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const uint MOD_NOREPEAT = 0x4000;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly HwndSource _source;
    private int _hotkeyId = 1;
    private bool _disposed;

    public event EventHandler? Pressed;

    public HotkeyListener(Window window, Hotkey hotkey)
    {
        IntPtr handle = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(handle)
            ?? throw new InvalidOperationException("Window has no message loop.");
        if (!RegisterHotKey(handle, _hotkeyId, (uint)hotkey.Modifiers | MOD_NOREPEAT, hotkey.VirtualKey))
        {
            throw new InvalidOperationException("Could not register the screenshot hotkey.");
        }
        _source.AddHook(WndProc);
    }

    public bool TryChangeHotkey(Hotkey hotkey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        int nextId = _hotkeyId == 1 ? 2 : 1;
        if (!RegisterHotKey(_source.Handle, nextId,
            (uint)hotkey.Modifiers | MOD_NOREPEAT, hotkey.VirtualKey))
        {
            return false;
        }

        UnregisterHotKey(_source.Handle, _hotkeyId);
        _hotkeyId = nextId;
        return true;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _source.RemoveHook(WndProc);
        UnregisterHotKey(_source.Handle, _hotkeyId);
    }
}
