# Settings

There is a settings window.
Settings window can be opened from main menu or from the tray icon context menu.


Settings need to include configuration for:
 * Capture hotkey
 * Capture Mode
 * UI Mode
   * Light / Dark / System

Settings window have "Apply" and "Cancel" buttons.

## Retaining settings

Settings are retained in current user's local application data.
Clicking "Apply" persists the settings, closing the Settings window in any other way - discarding the pending changes.

## Error handling

If there is any exception/error during persisting the settings, the user sees the error message and stays on Settings window - so he can fix the issue and try again.

## References
- There are default values for all settings - described in case settings-defaults.md.
