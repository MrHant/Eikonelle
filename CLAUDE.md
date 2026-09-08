# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Eikonelle is a personal, **Windows-only** screenshot manager, built on **.NET 10 + WPF** (C#).

## Build & test

```
dotnet build Eikonelle.slnx
dotnet test Eikonelle.slnx                 # runs the ceps exams
dotnet test Eikonelle.slnx --filter "FullyQualifiedName~TakingScreenshot"   # one exam
dotnet run --project src/Eikonelle         # launch the app (registers a global hotkey, shows no window until first capture)
```

The solution file is `Eikonelle.slnx` (the newer XML format — plain `Eikonelle.sln` does not exist).
`dotnet` is on PATH; the Rust toolchain under `~/.cargo` is unused (an earlier, abandoned stack choice).

## Code architecture

Three projects:

- `src/Eikonelle.Core/` — a `net10.0-windows` class library holding the domain logic and **no
  WPF dependency**, so exams can exercise it without a UI thread. Types: `Hotkey` /
  `HotkeyModifiers` (the trigger; `Hotkey.Capture` is the fixed combo), `PrimaryScreen` /
  `ScreenBounds` (the monitor being captured, via `GetSystemMetrics` P/Invoke), `Screenshot`
  (owns a `System.Drawing.Bitmap`), `ScreenshotCapture` (`Capture()` → `Screenshot` of the
  primary monitor via `Graphics.CopyFromScreen`), `PreviewModel` (observable state: `Current`
  screenshot + `IsVisible`), and `ScreenshotCommand` (`Execute()` = capture then
  `PreviewModel.Show`).
- `src/Eikonelle/` — the WPF executable (`WinExe`). `App` wires `Hotkey.Capture` through a
  `HotkeyListener` (Win32 `RegisterHotKey` + `WM_HOTKEY` hook on a window handle) to a
  `ScreenshotCommand`, then raises `PreviewWindow`. `PreviewWindow` observes `PreviewModel` and
  renders `Current` into an `Image`. The app has `ShutdownMode=OnExplicitShutdown` and runs
  windowless until the first capture; closing the preview hides it rather than exiting.
- `tests/Eikonelle.Exams/` — the exam runner (xUnit). It references only `Eikonelle.Core` and
  compiles the exam source from `ceps/exams/**/*.cs` via a `<Compile Include>` glob, keeping the
  exam files themselves inside the ceps tree.

The split between `Eikonelle.Core` (headless, testable) and `Eikonelle` (WPF shell) is the load-bearing
architectural choice: keep new domain behavior in Core so a ceps exam can cover it.

## ceps specification protocol

Development here is driven by **ceps** (spec backwards), a bottom-up executable specification
protocol. The full normative text is `ceps/ceps.md` (the local copy governs); `ceps/constraints.md`
holds project-wide constraints. Read every file in `ceps/cases/` and every `ceps/*.md` instruction
file before writing implementation code.

Structure:
- `ceps/cases/<id>.md` — one behavior each, described in natural language. The file path minus
  `.md` is the stable `id`; subdirectories are part of the `id`.
- `ceps/exams/` — exactly one exam file per case, its path derived from the `id`
  (`ceps/cases/group/beta.md` → `beta`-derived file under `ceps/exams/group/`). Written in the
  project's test framework; an exam file may hold multiple framework-level tests for that one case.
- `ceps/constraints.md` and other `ceps/*.md` — cross-cutting requirements that must be satisfied
  even when no case or exam enforces them. Currently: **Windows-only**.

Rules that constrain how you work in this repo:
- Do **not** modify `ceps/cases/*` or any `ceps/*.md` instruction file unless the user explicitly
  asks. A request to change specified behavior is not "ambiguity" — but if asked, amend the
  affected cases before changing code and report what changed.
- Do **not** weaken, skip, delete, or rewrite ceps exams to make validation pass.
- ceps exams and the project's own tests are separate artifacts that coexist; overlapping coverage
  is expected. Never merge, dedupe, move, or delete one against the other unless explicitly asked.
- If a case, constraint, or exam materially contradicts another source (or the project's own
  tests), stop and ask the user which behavior is correct. Once resolved, record the answer as an
  instruction in a `ceps/*.md` file before continuing.
- Implement only behavior supported by ceps evidence; prefer the smallest coherent solution and
  avoid unsupported features or unnecessary architecture. If logical modules are needed, name them
  in domain terms and use those names consistently across cases.
- An implementation is complete only when every case has an exam, all exams pass, pre-existing
  project checks still pass, and no known behavior contradicts a case or constraint. Report
  assumptions and unresolved issues to the user.

## Current cases

- `taking-screenshot` — pressing a key combination captures a screenshot, shown in a preview
  window. Exam: `ceps/exams/TakingScreenshot.cs`. Project clarifications (from the user, not yet
  written into the case): trigger is a fixed, non-configurable **Ctrl+Shift+S**; capture covers
  the **primary monitor**; the screenshot is shown in the preview window only — not saved to disk
  or copied to the clipboard.
- `tray-icon` — while the app runs it shows a notification-area icon with a context menu. Exam:
  `ceps/exams/TrayIcon.cs`. Domain type: `TrayIcon` / `TrayMenuItem` in `Eikonelle.Core`; the WPF
  shell renders it via `TrayIconShell` (WinForms `NotifyIcon`, needs `UseWindowsForms`). Project
  clarifications (from the user, not yet written into the case): the context menu holds a single
  **Exit** item that shuts the app down (the app's only explicit-exit path); the icon has **no**
  left/double-click behaviour. Non-normative: placeholder `SystemIcons.Application` glyph,
  "Eikonelle" tooltip.
