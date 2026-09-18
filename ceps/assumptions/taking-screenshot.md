# Taking screenshot details

Related case: taking-screenshot.

- The screenshot captures the primary monitor only.
- Taking a screenshot does not copy an image to the clipboard.
- The saved file is exactly what the preview first shows: the whole primary monitor in Full Screen mode, only the
  selected region in Region mode. A cancelled region selection saves nothing. Applying edits in the editor does
  not save or overwrite a file (per answer `editor`).
- Each screenshot is saved as a PNG named `Eikonelle yyyy-MM-dd HH-mm-ss.png` after the local time it was taken.
  An existing file is never overwritten; a name already in use gets a ` (2)`, ` (3)`, ... suffix.
- The Save Folder is created, including missing parent folders, when a screenshot is saved into it.
- If the screenshot cannot be saved, it is still shown in the preview, and a message box names the folder and the
  reason. Nothing is retried.
- The application shows no window until the first screenshot is taken.
- Closing the preview window hides it; the application keeps running and the hotkey stays active.
- In Region mode, the primary monitor is captured when the hotkey is pressed, and the region is selected on
  that frozen image, shown full-screen over the primary monitor with a cross cursor. The region is limited to
  the primary monitor.
- Pressing Escape during region selection cancels it: no screenshot is taken and the preview is not shown.
- Releasing the mouse without dragging out a non-empty rectangle selects nothing; the selection stays open so
  the user can drag again.
- Pressing the screenshot hotkey while a region is being selected is ignored.
