# Settings interaction details

Related cases: settings, settings-defaults.

- Opening Settings again activates the existing Settings window.
- Apply takes effect all-or-nothing: if the selected hotkey is invalid or unavailable, or the selected Save
  Folder is invalid, none of the selected settings is applied.
- A settings file written before Capture Mode existed holds only the hotkey (its modifiers and key at the top
  level); its hotkey still loads.
- At startup, one message lists each setting that was invalid in the settings file and now uses its default. A
  settings file that is not a JSON object, or that fails to load, is reported the same way. A setting that is
  missing from the file is not reported.
- The Save Folder path is read-only and is changed only with the folder picker, which opens at the current
  Save Folder when it exists.
