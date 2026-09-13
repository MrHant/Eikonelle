# Repository guidance

Eikonelle is a personal, Windows-only screenshot manager built with C#,
.NET 10, and WPF. This guidance applies throughout the repository.

## Build and validation

Run from the repository root on Windows with the .NET 10 SDK and
[Task](https://taskfile.dev). Prefer these tasks over raw commands, and add any
new repeatable dev operation to `Taskfile.yml`:

```powershell
task build
task test
task test:filter -- TakingScreenshot
task check    # build + test
task run
```

The solution is `Eikonelle.slnx`; there is no `Eikonelle.sln`. Screenshot
capture exams require an accessible Windows desktop. Report environmental
validation failures explicitly. After manual validation, use the tray's Exit
command to stop the application; closing the preview does not shut it down.

For implementation changes, run the build, ceps exams, and any applicable
project checks. For UI changes, also verify the affected interaction on Windows
when possible; distinguish automated validation from manual checks. Documentation
only changes do not require launching the application or running tests.

## Project structure

- `src/Eikonelle.Core/`: domain logic targeting `net10.0-windows`, without a WPF
  dependency. Contains capture, hotkey definitions, preview state, tray menu
  state, and screenshot editing. Keep domain behavior here so exams can exercise
  it without a UI thread.
- `src/Eikonelle/`: WPF executable. `App` wires capture and application lifetime;
  `HotkeyListener` handles Win32 hotkeys; `TrayIconShell` uses WinForms
  `NotifyIcon`; preview and editor windows present the domain state.
- `tests/Eikonelle.Exams/`: xUnit runner referencing Core. Its project file
  compiles `ceps/exams/**/*.cs`; place ceps exams in that tree.
- `ceps/`: behavioral specification, executable exams, constraints, assumptions,
  and recorded user resolutions.

Follow the existing C# style: file-scoped namespaces, four-space indentation,
nullable references, and domain-oriented names. Keep WPF and WinForms details
in the application project. Preserve explicit ownership and disposal of bitmaps,
graphics objects, native handles, and event subscriptions.

## Specification workflow

Read and follow [ceps/ceps.md](ceps/ceps.md) before implementation. It governs
specification evidence, required reading, exams, user resolutions, assumptions,
and completion criteria.

Preserve unrelated user changes, including untracked specification files.
