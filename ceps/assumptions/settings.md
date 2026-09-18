# Settings interaction details

Related cases: settings, settings-defaults.

- Opening Settings again activates the existing Settings window.
- Capture Mode is chosen with two options, "Full Screen" and "Region".
- UI Mode is chosen with three options, "Light", "Dark", and "System".
- Apply takes effect all-or-nothing: if the selected hotkey is invalid or unavailable, or the selected Save
  Folder is invalid, none of the selected settings is applied.
- A settings file written before Capture Mode existed holds only the hotkey (its modifiers and key at the top
  level); its hotkey still loads.
- At startup, one message lists each setting that was invalid in the settings file and now uses its default. A
  settings file that is not a JSON object, or cannot be read, is reported the same way and all defaults are used.
  A setting that is simply missing from the file uses its default without a message.
- The Save Folder is shown as a read-only path and is changed only with the folder picker, which opens at the
  current Save Folder when it exists.
