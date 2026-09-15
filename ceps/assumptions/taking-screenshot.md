# Taking screenshot details

Related case: taking-screenshot.

- The screenshot captures the primary monitor only.
- The screenshot is only displayed in the preview window. Taking it does not save a file or copy an image to the clipboard.
- The application shows no window until the first screenshot is taken.
- Closing the preview window hides it; the application keeps running and the hotkey stays active.
- In Region mode, the primary monitor is captured when the hotkey is pressed, and the region is selected on
  that frozen image, shown full-screen over the primary monitor with a cross cursor. The region is limited to
  the primary monitor.
- Pressing Escape during region selection cancels it: no screenshot is taken and the preview is not shown.
- Releasing the mouse without dragging out a non-empty rectangle selects nothing; the selection stays open so
  the user can drag again.
- Pressing the screenshot hotkey while a region is being selected is ignored.
