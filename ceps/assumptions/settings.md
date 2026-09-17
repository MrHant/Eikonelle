# Settings interaction details

Related case: settings.

- Opening Settings again activates the existing Settings window.
- Capture Mode is chosen with two options, "Full Screen" and "Region".
- UI Mode is chosen with three options, "Light", "Dark", and "System".
- Apply takes effect all-or-nothing: if the selected hotkey is invalid or unavailable, neither the selected
  capture mode nor the selected UI mode is applied.
- A settings file written before Capture Mode existed (holding only the hotkey) still loads its hotkey and uses
  the default Full Screen capture mode. An unrecognized saved capture mode falls back to Full Screen with a message.
- A settings file written before UI Mode existed loads its other values and uses the default System UI mode.
  An unrecognized saved UI mode falls back to System with a message.
