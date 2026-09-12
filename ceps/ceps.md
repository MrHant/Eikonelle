# ceps Protocol Specification

Version `0.3`

ceps (`spec` backwards) is a bottom-up executable specification protocol for AI-assisted software development. Behavioral cases, their descriptions, executable exams, and explicit constraints are the evidence from which an implementation is derived.

The key words **MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY** are normative.

## 1. Principles

1. **Behavior before implementation.** ceps specifies externally observable behavior. It does not prescribe internal architecture unless an explicit constraint or case requires it.
2. **Evidence before inference.** Agents derive behavior from cases, exams, fixtures, examples, and constraints. They MUST distinguish stated behavior from assumptions.
3. **Exams are executable evidence.** Passing exams is required, but not sufficient when an implementation contradicts a case description or constraint.
4. **Ambiguity is visible.** Contradictions and material gaps MUST NOT be silently resolved.
5. **The smallest coherent solution wins.** Agents SHOULD avoid unsupported features and unnecessary architecture. This principle governs the implementation, not the extent of exam coverage.

## 2. ceps project

A ceps project MUST contain this specification document as `ceps/ceps.md`.
Local copy of `ceps/ceps.md` governs.

The `cases`, `exams`, `assumptions`, and `answers` folders MUST exist in the `ceps/` folder. Any of them MAY be empty. This requirement scopes only ceps' own folders and does not relocate or replace anything else in the project.

A **ceps case** describes one behavior. A **ceps exam** is the executable evidence covering it, and lives in `ceps/exams/`. These are ceps artifacts. Whatever tests, checks, or other verifications the project already has are separate from them: the two coexist, overlapping coverage between them is expected, and an agent MUST NOT treat that overlap as redundancy to eliminate. An agent MUST NOT move, merge, delete, rewrite, or deduplicate the project's own tests against ceps exams, or ceps exams against the project's own tests, and MUST NOT omit a ceps exam for a case because the project already covers that behavior elsewhere. An agent MAY do so only when the user explicitly asks, and MUST report the change.

A project MAY declare global instruction files in `ceps/*.md`. Typical global instruction file is `ceps/constraints.md`.

Two of these folders are written by agents. `ceps/assumptions/` holds behavior an agent inferred but no human has accepted. `ceps/answers/` holds resolutions the user gave when an agent asked. Their standing as evidence is stated in §3, and their file rules in §6.

A project is **ceps-compliant** when folder structure is valid, and every discovered ceps case has a stable identifier and a ceps exam.

ceps-related project structure:
```
ceps/
├── ceps.md
├── *.md      # global instruction files, like constraints.md
├── cases/
├── exams/
├── assumptions/
└── answers/
```

## 3. Sources of evidence

ceps recognizes four sources of evidence:

1. **ceps cases** explain intent and observable behavior in natural language.
2. **ceps exams** provide deterministic examples and verification.
3. **Fixtures and exam data** provide concrete inputs, outputs, and boundary conditions.
4. **Constraints** state cross-cutting requirements such as platform support, security, performance, dependencies, or public interfaces.

ceps cases and ceps exams form one specification and MUST agree. There is no automatic precedence between evidence sources. If two sources conflict materially, the project is ambiguous and the agent MUST clarify details with the user before choosing behavior.

Assumptions (`ceps/assumptions/`) are not a source of evidence. An agent MUST NOT derive behavior from them.

Answers (`ceps/answers/`) are not a source of evidence either; they record a decision the user made about evidence. An answer takes precedence over the evidence it explicitly names, and over nothing else. An answer is a record of a resolution, not a permanent home for it: the durable fix is amending the evidence it names.

Existing implementation code and the project's own tests are context, not specification, unless a ceps case or constraint explicitly declares compatibility with them. Reading them is useful; they do not define required behavior.

## 4. ceps cases

ceps cases MUST be UTF-8 Markdown files. 

Name of the case file without .md extension MUST be treated as `id` - stable case identifier.
ceps case file MAY be placed in subdirectories of `ceps/cases/`. Subdirectories need to be retained in `id`.

A case's executable coverage lives in an exam file, its name is derived from the `id`.
For example:
- `ceps/cases/alpha.md` is covered by `alpha`-derived file in `ceps/exams/`. 
- `ceps/cases/group/beta.md` is covered by `beta`-derived file in `ceps/exams/group/`.

One ceps case MUST have exactly one exam file.

An exam file is written using the project's test framework. Within this specification, *exam* refers to the file and its role as evidence; the individual framework-level tests it contains are an implementation detail of the exam. One exam file MAY contain multiple such tests, all belonging to the same case.

Specific naming of the exam file is dependent on the programming language and test framework.

Naming the file rather than the tests inside it states where coverage belongs without prescribing how many tests express it or what they are called.

The body MUST state the expected behavior precisely enough to interpret the linked exam. It MAY include:

- a behavior-oriented title;
- preconditions or context;
- the triggering action;
- observable outcomes;
- representative examples and important boundaries;
- relevant error behavior.

A case MUST NOT require readers to infer essential behavior solely from its title. Cases SHOULD avoid prescribing classes, functions, algorithms, or storage choices unless those details are part of the public contract.
If logical modules are required in the code - they MUST be named explicitly in domain terms, and used consistently across all the cases.

## 5. Constraints (Optional)

Constraints MAY be included as additional instructions in `ceps/constraints.md` file.
Constraints capture requirements that individual examples cannot express reliably. A constraint MUST be explicit and verifiable where practical. Typical constraints include:

- supported runtimes and operating systems;
- public API or command-line compatibility;
- allowed or forbidden dependencies;
- security and privacy properties;
- performance limits and expectations;
- files or interfaces that must not change.

Agents MUST satisfy constraints even when the linked ceps cases or exams do not enforce them. Aspirational preferences SHOULD be labeled as non-normative guidance.

## 6. Assumptions and answers

Assumption and answer files MUST be UTF-8 Markdown files. As with cases, the name of the file without the `.md` extension is its `id`, and files MAY be placed in subdirectories, which are retained in the `id`. These identifiers are independent of case identifiers: an assumption or answer MAY share a name with a case, and is not required to.

Neither kind of file has a ceps exam.

An assumption records behavior an agent inferred but no human has accepted. It MUST state the inferred behavior in the form a ceps case would, precisely enough that accepting it requires no rewriting. An assumption that bears on existing evidence SHOULD name it by `id`.

An assumption stands until it is resolved. It is resolved when the user accepts it, when an answer decides it, or when a ceps case covers the same behavior. Accepting an assumption is the user's decision; the agent then removes the file, so that `ceps/assumptions/` holds only what is still undecided.

An answer records a resolution the user gave. It MUST state:

- the question as it was put to the user;
- the user's decision;
- `Resolves:` the `id` of each ceps case, constraint, or assumption the decision governs.

An answer takes precedence only over what `Resolves:` names (§3). An answer that names nothing governs nothing, and an agent MUST treat it as a record with no normative effect.

## 7. Agent behavior

An agent implementing a ceps project MUST:

1. read all discovered ceps cases (`ceps/cases/`) and additional instructions (`ceps/*.md` other than `ceps/ceps.md`) before implementation;
2. read `ceps/answers/` and `ceps/assumptions/`, and apply each answer to the evidence it resolves, so that questions already decided are not raised again;
3. inspect linked exams, fixtures, and relevant existing code;
4. identify contradictions, missing references, and material ambiguity;
5. stop and request clarification when ambiguity could change public behavior, data integrity, security, or compatibility, and record the user's resolution in `ceps/answers/` once given;
6. otherwise record minor assumptions in `ceps/assumptions/` and proceed;
7. implement only behavior supported by ceps evidence;
8. run the ceps exams (`ceps/exams/`) and the project's own checks;
9. report validation results, assumptions, answers recorded, and unresolved issues.

An agent MUST NOT weaken, delete, skip, or rewrite ceps exams merely to make validation pass. It MAY modify exams when explicitly asked to develop or correct the specification, but MUST clearly report those changes.

An agent MUST NOT change the project's own tests to accommodate its implementation. When one of them contradicts a ceps case, a constraint, or a ceps exam, this is ambiguity under step 5: the agent MUST stop and ask the user which behavior is correct. Once answered, the agent MUST record the resolution in `ceps/answers/` before continuing.

An agent MUST NOT modify ceps cases unless explicitly asked to do so.
A request to change specified behavior is not ambiguity under step 5. If explicitly asked to modify cases, the agent MUST amend the affected cases before changing the implementation, so that no ceps case contradicts the delivered behavior, and MUST report which cases it changed.

An agent MUST NOT modify instruction files (`ceps/*.md`) unless explicitly asked to do so.

An agent MAY write to `ceps/assumptions/` and `ceps/answers/` without being asked. It MUST NOT record an answer the user did not give, and MUST NOT move an assumption into `ceps/cases/`. It MUST remove a resolved assumption (§6).

## 8. Completion

An implementation is complete with respect to a ceps project when:

- every discovered ceps case has a corresponding exam;
- all ceps exams pass;
- the project's own verifications that applied before the work still pass, and any additional rules the project states for changing it have been followed;
- no known behavior contradicts a ceps case or constraint;
- assumptions and remaining limitations are reported to the user.

Passing checks that no ceps case describes does not compensate for a failed described check. Passing all checks does not resolve a known semantic contradiction. The project's own verifications do not substitute for ceps exams, and ceps exams do not substitute for them.

## 9. Versioning

The protocol uses Major.Minor versioning, such as `0.1`.

Any update to the ceps specification MUST be accompanied by a new version number.
