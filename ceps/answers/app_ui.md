# UI Mode scope clarification

Question: app_ui says UI Mode covers only things implemented by the app itself, and leaves Windows-native
things out. The tray icon's context menu (a WinForms menu the app builds) and the region selection overlay
(an app window showing the frozen screenshot with a selection rectangle) are drawn by the app, but were
earlier assumed to be out of scope. Should either follow UI Mode?

Decision: Both. The tray context menu follows UI Mode, and so does the overlay's own chrome (its backdrop and
selection colours). The screenshot shown in the overlay is never recoloured. Message boxes and the folder
picker are Windows-native and stay out of UI Mode.

Resolves: app_ui
