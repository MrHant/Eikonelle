# Settings interaction details

Related case: settings.

- Preview, Editor, and Settings each expose File > Settings. Opening Settings again activates the existing Settings window.
- Apply activates the selected hotkey and saves it in the current user's local application data. Closing without Apply discards the pending selection.
- If Windows rejects a new combination, the previous hotkey stays active and Settings explains the failure. If saving fails, Settings reports that the new hotkey is active only for the current session.
- Missing settings use Ctrl+Shift+S. Unreadable or invalid settings show a message and use that default. If the startup hotkey is unavailable, the application reports the problem and exits.
- Tab navigates out of the hotkey field; pressing a modifier alone does not select a hotkey.
