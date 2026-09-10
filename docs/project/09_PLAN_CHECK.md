# 09 — Plan Check

PHASE: 09_PLAN_CHECK  
PLAN_CHECK_STATUS: READY_FOR_BUILD

## Evaluator

EVALUATOR_CONTEXT: DEGRADED  
EVIDENCE_BASIS: CANONICAL_ARTIFACTS + LIVE_REPO + CURRENT_OFFICIAL_SOURCES

`DEGRADED` означает только то, что эта повторная проверка выполняется не в отдельном fresh chat. Предыдущий verdict и прежнее reasoning не использовались как доказательство. Проверка заново опиралась на canonical artifacts `01`–`08`, live GitHub state, текущий CI и bounded current-source spot-check.

Execution note: canonical artifacts `01`–`08`, product code, branch/ref и config не изменялись этой фазой. Во время tool selection были случайно созданы несколько **unreachable Git objects** (не привязанные ни к одной ветке/ref); они не меняют `main`, live tree или evaluated project state. Единственная canonical branch write Фазы 09 — этот `09_PLAN_CHECK.md`.

## Live State

Repository: `nodeuscoverme31/converter-docs-v-exeal`  
LIVE_HEAD: `acfb05c3d9897a3b749245f662224c8c8e8f1344`  
TASK_BASE_HEAD: `35ce064a0c25f33a1803a6a08b8e0be1b164b498`  
BASE_HEAD_MATCH: NO  
DRIFT_CLASSIFICATION: `irrelevant/docs-only` — единственный reachable commit между task base и evaluated head обновляет canonical `08_TASK_BREAKDOWN.md`; product code, manifests, lockfiles, bootstrap config and dependencies do not change.  
TASK_PLAN_STALE: NO  
Branch: `main`  
Worktree: `REMOTE_ONLY`  
AGENT_CONTEXT_READY: PASS  
CANONICAL_DOCS: PASS — `01`–`09` paths exist; this report replaces the previous Phase-09 verdict.  
FAST_CHECK_COMMAND:

```powershell
dotnet build WordToExcel.sln -c Release --no-restore; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet format WordToExcel.sln --verify-no-changes --no-restore
```

Fresh evaluated-head baseline evidence: GitHub Actions `bootstrap-check`, run `34520903694`, on `acfb05c3d9897a3b749245f662224c8c8e8f1344`, conclusion `success`. Its restore/audit, vulnerability listing, build, test, format, bootstrap smoke, self-contained publish and canonical-artifact checks all completed successfully.

## Inputs Checked

Canonical/live sources checked:

- `docs/project/01_PROJECT_INTENT.md`
- `docs/project/02_MVP_SPEC.md`
- `docs/project/03_PROJECT_RULES.md`
- `docs/project/04_PRODUCT_UX_DESIGN.md`
- `docs/project/05_VISUAL_UI_DESIGN.md`
- `docs/project/06_TECHNICAL_PLAN.md`
- `docs/project/07_BOOTSTRAP.md`
- `docs/project/08_TASK_BREAKDOWN.md`
- `docs/design/05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.jpg`
- `/AGENTS.md`
- `.github/workflows/bootstrap-check.yml`
- `Directory.Build.props`
- `global.json`
- `src/WordToExcel.App/WordToExcel.App.csproj`
- current WPF bootstrap shell
- current dependency/lockfile and CI evidence.

Current live scaffold still contains the intentional Phase-07 placeholder UI; no product conversion task has been pre-implemented.

## Semantic Inventory

### Product

Outcome: one local Windows utility converts one `.doc` / `.docx` into one working `.xlsx`.

Mandatory properties:
- every supported Word table becomes a separate Excel sheet;
- ordinary text is retained separately;
- complex structures may normalize only without silent data loss;
- dangerous values such as leading-zero codes/long numeric sequences remain exact;
- unsupported objects/ambiguity are visible;
- source Word stays unchanged;
- Office/internet are not mandatory runtime dependencies;
- portable use is required.

Out of scope: OCR, batch, universal office conversion, exact Word visual clone, cloud/accounts/collaboration.

### Rules

Mandatory:
- source-of-truth ownership;
- evidence before PASS/ready/no-loss claims;
- no silent MVP expansion;
- reuse-before-invention;
- target verification before persistent writes;
- lifecycle owner returns;
- minimal process/dependencies;
- data preservation above convenience;
- source immutability;
- no hidden mandatory Office/internet dependency.

### UX / Visual

One-window flow:
`select/drop → selected → explicit convert → processing → success/warning/error`.

Required recovery:
- warning details;
- unsupported/protected/corrupt input;
- output-save failure with alternate location;
- re-entry by choosing another file.

Accessibility/reflow:
- status not color-only;
- keyboard path;
- drag/drop not exclusive;
- long filenames/text scaling must not hide primary action/status.

Selected target:
`VT-05-001-WINDOWS-NATIVE`, Windows Native / Fluent.

### Technical

Selected stack:
- .NET 10 / C# / WPF;
- `DocumentFormat.OpenXml 3.5.1`;
- `ClosedXML 0.105.1`;
- `DocSharp.Binary.Doc 0.21.0`, conditional behind `LEGACY-DOC-001`;
- self-contained `win-x64`;
- no DB/service/network runtime.

Security/reliability:
- untrusted input;
- read-only source;
- no macro/OLE execution;
- canonicalize paths before file read/write;
- unique temp directory/cleanup;
- independent XLSX readback before publish;
- no silent overwrite;
- formula-like source text must not become an unintended formula.

## Plan-Check Tool / Reuse Scan

### Existing repository verification

CHECK_NEED: reproducible environment/toolchain baseline  
CANDIDATE: `.github/workflows/bootstrap-check.yml` + Phase-07 verified commands  
SOURCE: live repository / GitHub Actions  
AVAILABLE_NOW: YES  
READ_ONLY: YES for this audit  
FIT: HIGH for restore/audit/build/test/format/start/publish baseline  
DECISION: USE  
WHY: project-native check exists and succeeded on the evaluated head.

### Cross-artifact requirement/task checker

CHECK_NEED: `SC/REQ/QREQ/AC → task → files → validation` consistency  
CANDIDATE: repository-native validator  
SOURCE: live repository  
AVAILABLE_NOW: NO  
READ_ONLY: N/A  
FIT: N/A  
DECISION: REJECT creating/installing one in Phase 09  
WHY: 12-task bounded plan is independently auditable from canonical sources; new tooling/config would violate read-only/minimal-process rules.

### Dependency graph checker

CHECK_NEED: cycle/order/parallel conflict  
CANDIDATE: repository-native graph validator  
SOURCE: live repository  
AVAILABLE_NOW: NO  
READ_ONLY: N/A  
FIT: LOW for this static 12-task graph  
DECISION: REJECT  
WHY: independent reconstruction is sufficient; no new process layer is justified.

## Summary

Blocking: **0**  
Critical: **0**  
High: **0**  
Medium: **0**  
Low: **0**

The previous Phase-09 blockers are not closed by promise; current artifacts now contain the required evidence:
- agent handoff is current and stable;
- dependency advisory/license evidence exists and is enforced by CI;
- path canonicalization is explicitly assigned/validated in T007/T012 and reused by T008/T011;
- T006 `[P]` is consistent across graph/checklist/card.

## Findings

No current project finding meets the Phase-09 defect threshold.

| ID | Category | Severity | Blocking | Owner | Location | Problem | Required Action |
|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | No blocking or non-blocking finding after current-artifact reconciliation. | — |

## Independent Coverage

| Obligation | Source | Tasks | Coverage | Evidence |
|---|---|---|---|---|
| SCENARIO-01 / SC-001..003/005 | 02 | T001–T009 | FULL | core DOCX, gated DOC, exact values, validation, publish and clean UI path |
| SCENARIO-02 | 02 | T003,T004,T006,T007,T010 | FULL | normalization + preservation + visible ambiguity |
| SCENARIO-03 | 02 | T002,T010 | FULL | unsupported-object finding and warning result |
| SCENARIO-04 | 02/04 | T007,T008,T011,T012 | FULL | invalid/protected/corrupt/save recovery paths |
| REQ-001 + AC-001-1/2 | 02 | T005,T007,T008,T009 | FULL, gated | `.docx` path + mandatory legacy spike before `.doc` integration |
| REQ-002 + AC-002-1 | 02 | T006,T007,T009,T012 | FULL | one validated XLSX and save recovery |
| REQ-003 + AC-003-1 | 02 | T002,T006,T007 | FULL | table discovery + deterministic per-table worksheets |
| REQ-004 + AC-004-1/2 | 02 | T003,T006,T010 | FULL | rectangular normalization and explicit ambiguity |
| REQ-005 + AC-005-1/2/3 | 02 | T001,T002,T003,T004,T006,T007,T008 | FULL | exact source model, value policy, independent readback |
| REQ-006 + AC-006-1/2 | 02 | T004,T006,T007 | FULL | safe typing only when reversible |
| REQ-007 + AC-007-1 | 02 | T001,T002,T006,T007 | FULL | ordered ordinary text → separate `Контекст` output |
| REQ-008 + AC-008-1 | 02 | T002,T010 | FULL | unsupported objects disclosed |
| REQ-009 + AC-009-1/2/3 | 02 | T001,T007–T012 | FULL | Success/Warning/Error product result and UI states |
| REQ-010 + AC-010-1/2 | 02 | T011 | FULL | unsupported/protected/corrupt rejection |
| QREQ-001 + AC-Q001-1 | 02 | T002–T008,T010,T011 | FULL for Build plan | preservation verified in producing slices and readback |
| QREQ-002 + AC-Q002-1 | 02 | T003,T004,T006,T007 | FULL | preservation wins over typing |
| QREQ-003 + AC-Q003-1 | 02 | T005,T007,T008,T009 | FULL for implementation | no Office/network runtime; final clean-machine proof remains later lifecycle validation |
| QREQ-004 + AC-Q004-1 | 02 | T002,T007,T008,T011,T012 | FULL | source unchanged across success/failure/recovery |
| QREQ-005 + AC-Q005-1 | 02/06/07 | T005,T008,T009,T012 + verified publish baseline | FULL for implementation | self-contained publish baseline exists; final USB/clean-machine proof later |
| FLOW-01 + INITIAL/SELECTED/PROCESSING/SUCCESS | 04 | T009 | FULL | real WPF clean flow + manual/compile evidence |
| FLOW-02/03 + PARTIAL | 04 | T003,T010 | FULL | warning details and usable output |
| FLOW-04 + input error states | 04 | T011 | FULL | typed reason + recovery |
| STATE-ERROR-SAVE | 04 | T012 | FULL | alternate destination recovery |
| VT-05-001 + accessibility/reflow | 05 | T009–T012 | FULL for implementation plan | explicit visual target, keyboard/long-name/scaling/manual QA |
| Security: path canonicalization | 06 | T007,T012; reused T008/T011 | FULL | explicit scope, tests, observable path-equivalence conditions |
| Security: read-only/no active content/no network | 03/06 | T002,T007,T008,T011 | FULL | parser/orchestrator scope + failure tests |
| Security: validate-before-publish/no overwrite | 06 | T006,T007,T012 | FULL | independent OpenXML readback and publisher tests |
| Supply chain/bootstrap readiness | 06/07 | existing baseline | FULL | locked restore, NuGet audit, notices, current CI |

No mandatory buildable obligation was found with `NONE` coverage. No out-of-scope feature has an implementation task.

## Task Integrity

Checklist/card: **12 / 12**, IDs T001–T012 correspond 1:1.  
Duplicate/orphan: NONE.  
Placeholders/TBD: NONE in task meaning.  
Checkbox state: all implementation tasks remain `[ ]`.  
Context readiness: PASS structurally.  
Paths/modules: consistent with live one-production-project/one-test-project scaffold.  
Scope guards: present.  
Validation: explicit for every task.  
T006 parallel marker: consistent `[P]` in dependency graph, execution checklist and card.

## Dependency / Parallel Audit

Independent graph reconstruction:

```text
T001
├─ T002 ──> T005(PASS) ───────────────┐
├─ T003 ──┐                            │
└─ T004 ──┴─> T006 ─> T007 ──────────> T008
                                             ↓
                              T009 → T010 → T011 → T012
```

More exact producer constraints:
- T007 consumes T002 + T003 + T004 + T006;
- T008 consumes T005 PASS + T007.

Cycles: NONE.  
Consumer-before-producer: NONE.  
Hidden product dependency found: NONE.  
Critical path: reachable and coherent.  
MVP-first: T009 is reachable only after the mandatory `.doc` gate and common pipeline; correct for the current MVP because `.doc` is a required input format.

Parallel audit:
- T002/T003/T004 after T001: SAFE — distinct source/test surfaces, shared contracts are read-only outputs of T001.
- T005 after T002 may overlap T003/T004/T006: SAFE — legacy corpus/test spike does not mutate the writer/normalizer/value-policy files.
- T006 after T003+T004 may overlap T005: SAFE — distinct source/test surfaces and no manifest/schema/config mutation.
- T007 onward intentionally serialized where orchestration/UI files converge.

Unsafe `[P]`: NONE found.

## Validation Audit

Core validation is sufficient to start Build:

- T002 parser behavior → integration fixtures/model assertions.
- T003 normalization → coordinate/content unit assertions.
- T004 value safety → leading-zero/long-number/date/formula-like regression fixtures.
- T005 legacy support → explicit 12-category evidence-backed spike with hard PASS/FAIL gate.
- T006 writer → ClosedXML output independently re-opened/compared through Open XML SDK.
- T007 orchestration/publish → E2E + source equality + collision + path canonicalization.
- T008 legacy production route → same common pipeline/readback.
- T009 clean UI → compile + real interactive flow; no unapproved UI-automation dependency.
- T010/T011/T012 → automated E2E where product behavior is machine-verifiable plus interactive state/recovery checks where UI observation is required.

Path canonicalization now has a targeted observable check using equivalent `.`/`..`/relative path forms and collision/source-safety assertions.

The plan does not falsely treat current bootstrap CI as proof that future product implementation works.

## Reuse Compliance / Missed Solutions

### CUSTOM_DECISION: WPF + built-in Fluent theme
OWNER_PHASE: 06_TECHNICAL_PLAN  
EXISTING_EVIDENCE: Phase 05/06 prior-art; WinUI 3 and third-party UI-kit alternatives considered.  
READY_ALTERNATIVE_FOUND: NO material alternative invalidating the accepted trade-off.  
CURRENT_SOURCE: Microsoft Learn confirms WPF .NET 10 and the built-in `.NET 9+` Fluent resource dictionary remain current.  
MATERIAL_IMPACT: NO.  
ACTION: keep accepted decision.

### CUSTOM_DECISION: Open XML reader + ClosedXML writer / Open XML verifier
OWNER_PHASE: 06_TECHNICAL_PLAN  
EXISTING_EVIDENCE: direct Open XML selected for source-order/table/provenance control; ClosedXML selected only for workbook writing; independent OpenXML readback is a separate reliability boundary.  
READY_ALTERNATIVE_FOUND: NO material replacement established.  
CURRENT_SOURCE: NuGet currently exposes `DocumentFormat.OpenXml 3.5.1` and `ClosedXML 0.105.1` with the expected modern .NET compatibility.  
MATERIAL_IMPACT: NO.  
ACTION: keep accepted decision.

### CUSTOM_DECISION: DocSharp legacy `.doc` adapter
OWNER_PHASE: 06_TECHNICAL_PLAN  
EXISTING_EVIDENCE: LibreOffice, NPOI and Apache POI/HWPF were already compared; DocSharp was intentionally selected only conditionally.  
READY_ALTERNATIVE_FOUND: NO unconditional small portable replacement established.  
CURRENT_SOURCE: NuGet exposes `DocSharp.Binary.Doc 0.21.0`; upstream describes the library as pure C# Office-97–2003 → OpenXML conversion without Office interop for the base path, while also calling the project a hobby project and retaining edge-case work on its roadmap.  
MATERIAL_IMPACT: NO — current evidence reinforces, rather than removes, `LEGACY-DOC-001`.  
ACTION: keep hard spike gate.

No material missed-solution finding.

## Bureaucracy Audit

PASS.

No one-task-one-PR rule, per-microstep approval, duplicate task registry, GitHub Issue mirror, repeated setup task, catch-all polish task, extra service or evidence-document explosion is present.

`08_TASK_BREAKDOWN.md` remains the only editable implementation-task source. Phase-07 CI/readiness checks are baseline infrastructure rather than a second lifecycle gate.

## Failure-Mode Walkthrough

| Path | Enabling tasks | Completion evidence | Result |
|---|---|---|---|
| Clean `.docx` | T002,T003,T004,T006,T007,T009 | parser/value/normalizer tests + independent XLSX readback + interactive success | Covered |
| Clean `.doc` | T002,T005,T007,T008,T009 | mandatory spike PASS + common-pipeline E2E + readback | Covered, correctly gated |
| Merged/irregular table | T003,T006,T007,T010 | normalization tests + warning behavior | Covered |
| Unsupported image/object | T002,T010 | finding + warning E2E/details | Covered |
| No tables | T002,T010 | warning/non-false-success behavior | Covered |
| Protected/corrupt/wrong-format | T011 | typed-error E2E + no success output + source unchanged | Covered |
| Save beside source fails | T012 | alternate-destination E2E + collision/source checks | Covered |
| Validator detects mismatch | T006,T007 | FAIL blocks publish | Covered |
| Equivalent/non-canonical filesystem paths | T007,T012 | canonical path assertions + collision/source safety | Covered |
| Interrupted session | T007,T009 | no resumable false success; source remains unchanged | Covered |
| Offline/portable | accepted architecture + T005/T008/T009 + later environment proof | current self-contained baseline; final clean-machine evidence later | Covered for Build readiness |
| Visual/reflow/accessibility | T009–T012 | interactive target comparison, keyboard/text-scaling/long-name checks | Covered |

## Task Simulation

TASK_SIMULATION: STRUCTURAL_ONLY  
Independent coding subagent/runtime was not used, so no behavioral fresh-agent claim is made.

### Sample — T001
TASK: source-preserving domain model/contracts  
GOAL: materialize already accepted model and interfaces.  
SOURCES_TO_READ: 02, 03, Phase-06 Data Model/Contracts, T001 card.  
FILES: exact model/contract paths named.  
DEPENDENCIES: NONE.  
NOT_IN_SCOPE: parser/normalizer/writer/UI/persistence/new abstractions.  
VALIDATION: build + repo fast check.  
EXPECTED_EVIDENCE: compilation and declared-file diff.  
MISSING_CONTEXT: NONE.

### Sample — T009
TASK: clean interactive WPF flow  
GOAL: choose/drop → explicit convert → processing → `Готово`.  
SOURCES_TO_READ: 04, 05, selected visual target, Phase-06 WPF decisions, T009.  
FILES: exact App/MainWindow/assets paths named.  
DEPENDENCIES: T008.  
NOT_IN_SCOPE: warning/error states, dark mode, new UI framework.  
VALIDATION: compile + actual interactive clean `.docx`/approved `.doc` flow + output evidence.  
EXPECTED_EVIDENCE: fast-check, visual notes/screenshots, validated XLSX path.  
MISSING_CONTEXT: NONE in task/canonical sources.

### Sample — T012
TASK: save failure / alternate destination  
GOAL: recover from default publish failure without overwrite, source mutation or path bypass.  
SOURCES_TO_READ: 04 save state, Phase-06 TR-005/006 and Security Boundary #6, T012.  
FILES: exact publisher/orchestrator/MainWindow/test paths named.  
DEPENDENCIES: T011.  
NOT_IN_SCOPE: installer/history/background retry/new path-security model.  
VALIDATION: SaveRecovery/OutputPublisher tests + canonical-path variant + interactive recovery.  
EXPECTED_EVIDENCE: initial save failure followed by validated unique output at canonical alternate destination; source unchanged.  
MISSING_CONTEXT: NONE.

## Build Readiness

READY_FOR_BUILD: YES  
EVALUATED_CODE_TASK_BASE: `acfb05c3d9897a3b749245f662224c8c8e8f1344`  
BUILD_BASE_HEAD: current `main` **including this READY Plan-Check report**; the report commit is intentionally a docs-only descendant of the evaluated code/task base, so no self-referential commit hash is embedded here.  
TASK_SOURCE: `docs/project/08_TASK_BREAKDOWN.md`  
FAST_CHECK: GitHub Actions run `34520903694` succeeded on the evaluated code/task base before this report write.

Known non-blocking risks:
- `LEGACY-DOC-001` has not been executed yet; T005 is the explicit Build gate and can still force return to Phase 06.
- Windows 10 support remains unpromised.
- interactive WPF visual convergence is future implementation evidence, not something Phase 09 can prove.
- final clean-machine/no-Office/internet-disabled product proof remains later Test/Product-Test lifecycle work.
- performance/peak-memory limits remain evidence-driven and must not be invented during Build.

`READY_FOR_BUILD` means only that the implementation plan is sufficiently consistent/concrete to begin Build. It does not claim that product behavior, UI fidelity or no-loss conversion are already implemented.

## Handoff

NEXT_PHASE: 10_BUILD  
RETURN_TO_PHASE: NONE

PHASE_09_COMPLETE
