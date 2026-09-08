# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Eikonelle is a personal, **Windows-only** screenshot manager. The repository is currently
pre-implementation: it contains a specification (`ceps/`), license, and README, but no
application code, build system, or chosen language/framework yet. When adding the first
implementation, pick the stack that yields the smallest coherent solution for the specified
behavior and record the decision (see ceps principle 5 below), then update this file with
the real build/lint/test commands.

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

- `taking-screenshot` — pressing a specified key combination captures a screenshot, shown in a
  preview window. (No exam yet.)
