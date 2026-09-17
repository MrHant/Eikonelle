# Application UI appearance details

Related cases: app_ui, settings, settings-defaults.

- The UI mode chosen in settings decides the appearance: Light always draws the UI light, Dark always draws
  it dark, and System draws whatever Windows is configured to use for apps.
- A UI mode that is not one of the three supported values is drawn as System.
- A newly chosen UI mode takes effect when Settings is applied, on every open window at once, with no restart.
  Selecting it does not preview it, and closing Settings any other way leaves the appearance unchanged.
- While the chosen mode is System, the running application follows Windows: changing the Windows app
  appearance redraws it without a restart.
- The appearance covers the preview, editor, and settings screens, including their menus. The tray icon's
  context menu, the region selection overlay, and message boxes are drawn by Windows and are not affected.
- Only the UI around a screenshot changes; the screenshot itself is never recoloured.
