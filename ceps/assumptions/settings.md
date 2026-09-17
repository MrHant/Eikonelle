# Settings interaction details

Related case: settings.

- Opening Settings again activates the existing Settings window.
- Capture Mode is chosen with two options, "Full Screen" and "Region".
- Apply takes effect all-or-nothing: if the selected hotkey is invalid or unavailable, the selected capture mode
  is not applied either.
- A settings file written before Capture Mode existed (holding only the hotkey) still loads its hotkey and uses
  the default Full Screen capture mode. An unrecognized saved capture mode falls back to Full Screen with a message.
