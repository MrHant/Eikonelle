# Repository guidance

Eikonelle is a personal, Windows-only screenshot manager built with C#,
.NET 10, and WPF. This guidance applies throughout the repository.

## Build and validation

Run from the repository root on Windows with the .NET 10 SDK:

```powershell
dotnet build Eikonelle.slnx
dotnet test Eikonelle.slnx
dotnet test Eikonelle.slnx --filter "FullyQualifiedName~TakingScreenshot"
dotnet run --project src/Eikonelle
```

The solution is `Eikonelle.slnx`; there is no `Eikonelle.sln`. Launching the
application registers a global hotkey and creates a tray icon. It remains
windowless until capture; closing the preview hides it, and the tray's Exit
command shuts it down. Screenshot capture exams require an accessible Windows
desktop. Report environmental validation failures explicitly.

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

The local `ceps/ceps.md` governs the specification protocol. Read it before
implementation, together with every `ceps/*.md` instruction file, every case
under `ceps/cases/`, and the contents of `ceps/answers/` and `ceps/assumptions/`.
Inspect linked exams, fixtures, and relevant code before changing behavior.

- Cases, exams, fixtures, and constraints define behavior. Existing code and
  project tests provide context; they do not independently define requirements.
  `CLAUDE.md` contains additional project notes, but historical behavior summaries
  must be checked against current ceps evidence and user instructions.
- Apply recorded answers only to the identifiers listed in their `Resolves:`
  field. Do not ask again about an already resolved question.
- Do not modify cases or `ceps/*.md` instruction files unless explicitly asked.
  When authorized to amend cases, update them before the implementation and
  report which cases changed.
- Each case needs exactly one exam file derived from its identifier, preserving
  any case subdirectories. Multiple xUnit tests may cover that case in its file.
- Do not weaken, skip, delete, or rewrite exams to make validation pass. Do not
  change the project's own tests to accommodate an implementation. Keep ceps
  exams and other project tests separate, even where coverage overlaps.
- For material contradictions or ambiguity affecting public behavior, data
  integrity, security, or compatibility, seek the user's resolution before
  choosing behavior. Record the actual decision in `ceps/answers/` with the
  question, decision, and `Resolves:` identifiers.
- Record minor inferred behavior in `ceps/assumptions/` and remove resolved
  assumptions. Assumptions are not specification evidence. Never invent user
  answers or promote assumptions into cases.
- Implement the smallest coherent behavior supported by the evidence. Preserve
  unrelated user changes, including untracked specification files.

Before claiming an implementation complete, verify that every discovered case
has an exam, all exams and applicable project checks pass, and no known behavior
contradicts a case or constraint. Report validation, assumptions, recorded
answers, and unresolved gaps; passing existing exams alone is insufficient.
