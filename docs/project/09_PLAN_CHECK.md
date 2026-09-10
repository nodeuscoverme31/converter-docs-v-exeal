# 09 — Plan Check

PHASE: 09_PLAN_CHECK  
PLAN_CHECK_STATUS: NOT_READY

## Evaluator

EVALUATOR_CONTEXT: DEGRADED  
EVIDENCE_BASIS: CANONICAL_ARTIFACTS + LIVE_REPO + CURRENT_OFFICIAL_SOURCES

`DEGRADED` означает только то, что проверка выполнялась не в отдельном fresh chat. Предыдущее conversational reasoning не использовалось как evidence; выводы ниже основаны на canonical artifacts, live GitHub state и текущих официальных источниках.

## Live State

Repository: `nodeuscoverme31/converter-docs-v-exeal`  
LIVE_HEAD: `930ef1c8d555275686607e9324fedc4481b4a7ce`  
TASK_BASE_HEAD: `a1eafd337508f4c84ffd11614cff928d45135efe`  
BASE_HEAD_MATCH: NO  
DRIFT_CLASSIFICATION: `irrelevant/docs-only`  
TASK_PLAN_STALE: NO  
Branch: `main`  
Worktree: `REMOTE_ONLY`  
AGENT_CONTEXT_READY: NOT_READY_FOR_BUILD  
CANONICAL_DOCS: PASS — `01`–`08` present  
FAST_CHECK: `dotnet build WordToExcel.sln -c Release --no-restore; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet format WordToExcel.sln --verify-no-changes --no-restore`

`LIVE_HEAD` отличается от `TASK_BASE_HEAD` ровно одним commit, который добавляет только `docs/project/08_TASK_BREAKDOWN.md`; product code, manifests, lockfiles и bootstrap workflow не менялись. Поэтому сам task plan не считается stale из-за hash drift.

Fresh baseline evidence на `LIVE_HEAD`: GitHub Actions `bootstrap-check`, run `34513412200`, conclusion `success`.

## Inputs Checked

Проверены canonical/live:
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
- `global.json`
- production/test project files and lockfiles
- current repository tree and current CI run.

Live scaffold still contains only the Phase-07 WPF shell plus bootstrap smoke; no product `REQ-*` implementation exists.

## Plan-Check Tool / Reuse Scan

### Existing repository baseline checks
CHECK_NEED: reproducible restore/build/test/format/start/publish baseline  
CANDIDATE: `.github/workflows/bootstrap-check.yml` + verified Phase-07 commands  
SOURCE: live repository / GitHub Actions  
AVAILABLE_NOW: YES  
READ_ONLY: YES for this audit  
FIT: HIGH for environment/toolchain baseline; it does not check cross-artifact consistency.  
DECISION: USE  
WHY: this is the project-native verification path and current run succeeds on `LIVE_HEAD`.

### Requirement/task consistency checker
CHECK_NEED: `SC/REQ/QREQ/AC → task → files → validation` consistency  
CANDIDATE: repository-native validator  
SOURCE: live repository  
AVAILABLE_NOW: NO  
READ_ONLY: N/A  
FIT: N/A  
DECISION: REJECT creating/installing a new checker in Phase 09  
WHY: the plan has 12 tasks and can be boundedly audited from canonical sources; adding tooling/config would violate Phase-09 read-only scope.

### Dependency graph checker
CHECK_NEED: cycle/ordering/parallel-conflict review  
CANDIDATE: repository-native graph tool / Spec-Kit-style analyze  
SOURCE: live repository + workflow prior-art  
AVAILABLE_NOW: NO project-native installation/configuration  
READ_ONLY: not available without adoption/configuration  
FIT: LOW for this small static graph  
DECISION: REJECT for this audit  
WHY: independent graph reconstruction is sufficient; Phase 09 must not adopt a new process layer.

## Summary

Blocking: **3**  
Critical: **0**  
High: **1**  
Medium: **2**  
Low: **1**

Earliest owning phase for remediation: **07_BOOTSTRAP**.

## Findings

| ID | Category | Severity | Blocking | Owner | Location | Evidence | Problem | Required Action | Downstream Affected |
|---|---|---:|---|---|---|---|---|---|---|
| F09-001 | Agent context / bootstrap | HIGH | YES | 07_BOOTSTRAP | `/AGENTS.md`; bootstrap readiness | Live `AGENTS.md` still says the next owning phase is Phase 08 and its source-of-truth map stops at `07_BOOTSTRAP.md`, while live repo already contains completed `08_TASK_BREAKDOWN.md` and Phase 09 is active. The bootstrap workflow's canonical-doc check also stops at 07. | A fresh Build agent can follow the repo-wide instruction file and be directed to an already completed phase without being told that `08_TASK_BREAKDOWN.md` is the executable task source. `AGENT_CONTEXT_READY: PASS` recorded in Phase 07 was valid for entering Phase 08, not for entering Build after later artifacts appeared. | Refresh the repository-wide operational instruction surface for current Build handoff: expose `08_TASK_BREAKDOWN.md` and the current Plan-Check gate, or make lifecycle discovery deliberately non-stale. Re-run bootstrap/readiness verification. Do not duplicate the canon. | 08, 09, 10 |
| F09-002 | Bootstrap / supply chain | MEDIUM | YES | 07_BOOTSTRAP | `06_TECHNICAL_PLAN.md` Supply chain → live repo / `07_BOOTSTRAP.md` / CI | Phase 06 requires, before bootstrap, pinned versions, current package metadata, known-advisory check, and third-party license notices for shipped dependencies/assets. Versions and lockfiles exist, but the live tree, Phase-07 evidence and CI contain no recorded advisory result and no third-party notice/license artifact. | Phase 07 declared bootstrap complete without evidence for an explicit technical-plan bootstrap obligation. This does not prove a vulnerable package exists; it proves the required check/record is missing. | Perform a fresh advisory check on the locked dependency graph, record the evidence, and add only the license/notice material actually required for shipped dependencies/assets. If the check forces a dependency decision, route that decision to Phase 06. | 07, 08, 09, 10, release |
| F09-003 | Security coverage / task decomposition | MEDIUM | YES | 08_TASK_BREAKDOWN | T007/T012 and coverage map | Phase 06 security boundary explicitly requires file-path canonicalization before reading/writing. No T001–T012 card assigns or validates this obligation; T007/T012 cover read-only, safe naming and publishing but omit canonicalization. | Build can satisfy every listed task card while silently omitting an accepted security rule. A fresh agent would have to discover and self-assign work outside the task contract. | Add the existing Phase-06 path-canonicalization obligation to the appropriate input/publish task card(s), with observable validation, without inventing a new architecture. Update coverage/dependency text only where affected. | 08, 09, 10 |
| F09-004 | Task integrity / parallel marker | LOW | NO | 08_TASK_BREAKDOWN | T006 | Dependency graph and T006 detailed card mark T006 parallel-capable, but the executable checklist omits `[P]` for T006. | Canonical status projection and detailed card disagree. This does not block sequential execution, but creates ambiguity for parallel scheduling. | Make the checklist/card/graph marker consistent when Phase 08 is next amended. | 08, 10 |

## Independent Coverage

| Obligation | Source | Tasks | Coverage | Evidence |
|---|---|---|---|---|
| SCENARIO-01 / SC-001..003/005 | 02 MVP Spec | T001–T009 | FULL for planned behavior | DOCX core, legacy gate, writer/readback and clean UI are decomposed. |
| SCENARIO-02 | 02 MVP Spec | T003,T004,T006,T007,T010 | FULL | Normalization, preservation and warning path are assigned. |
| SCENARIO-03 | 02 MVP Spec | T002,T010 | FULL | Unsupported-object detection and user warning are assigned. |
| SCENARIO-04 | 02 MVP Spec + 04 UX | T007,T008,T011,T012 | FULL | Invalid/protected/corrupt input and output-save recovery are assigned. |
| REQ-001 / AC-001-1/2 | 02 MVP Spec | T005,T007,T008,T009 | FULL, contingent on `LEGACY-DOC-001` | `.docx` core plus hard-gated `.doc` route. |
| REQ-002 / AC-002-1 | 02 MVP Spec | T006,T007,T009,T012 | FULL | XLSX creation, validation, publication and save recovery. |
| REQ-003 / AC-003-1 | 02 MVP Spec | T002,T006,T007 | FULL | Table discovery and one-sheet-per-table writer path. |
| REQ-004 / AC-004-1/2 | 02 MVP Spec | T003,T006,T010 | FULL | Rectangular normalization and ambiguity warning. |
| REQ-005 / AC-005-1/2/3 | 02 MVP Spec | T001,T002,T003,T004,T006,T007,T008 | FULL | Exact data model, value policy, readback and E2E. |
| REQ-006 / AC-006-1/2 | 02 MVP Spec | T004,T006,T007 | FULL | Safe typing only when reversible. |
| REQ-007 / AC-007-1 | 02 MVP Spec | T001,T002,T006,T007 | FULL | Ordinary text separated into context output. |
| REQ-008 / AC-008-1 | 02 MVP Spec | T002,T010 | FULL | Omitted object becomes explicit finding/warning. |
| REQ-009 / AC-009-1/2/3 | 02 MVP Spec | T001,T007–T012 | FULL | Success/warning/error outcomes are planned. |
| REQ-010 / AC-010-1/2 | 02 MVP Spec | T011 | FULL | Protected/corrupt/unsupported input rejection. |
| QREQ-001 / AC-Q001-1 | 02 MVP Spec | T002–T008,T010,T011 | FULL for Build plan | Preservation is tested per producing slice; final product proving continues later. |
| QREQ-002 / AC-Q002-1 | 02 MVP Spec | T003,T004,T006,T007 | FULL | Preservation wins over typing. |
| QREQ-003 / AC-Q003-1 | 02 MVP Spec | T005,T007,T008,T009 | FULL for implementation; product-environment proof later | Architecture has no Office/network runtime dependency; clean-machine proof is intentionally deferred to Test/Debug. |
| QREQ-004 / AC-Q004-1 | 02 MVP Spec | T002,T007,T008,T011,T012 | FULL | Source unchanged is asserted through success/failure paths. |
| QREQ-005 / AC-Q005-1 | 02 MVP Spec | T005,T008,T009,T012 + existing publish baseline | FULL for implementation; portable proof later | Self-contained publish baseline exists; final USB/clean-machine proof is later lifecycle work. |
| FLOW-01 + INITIAL/SELECTED/PROCESSING/SUCCESS | 04 UX | T009 | FULL | One-window file selection/drop, explicit start, honest processing and success result. |
| FLOW-02/03 + PARTIAL | 04 UX | T003,T010 | FULL | Warning/detail behavior assigned. |
| FLOW-04 + FORMAT/PROTECTED/CORRUPT | 04 UX | T011 | FULL | Typed failure/recovery assigned. |
| STATE-ERROR-SAVE | 04 UX | T012 | FULL | Alternate-destination recovery assigned. |
| VT-05-001 Windows Native/Fluent | 05 Visual | T009–T012 | FULL for implementation plan | Real repo visual artifact exists; UI states reference it. |
| File-path canonicalization before read/write | 06 Technical Plan / Security Boundary | NONE explicit | NONE | F09-003. |
| Pre-bootstrap advisory/license evidence | 06 Technical Plan / Supply chain | Phase 07 obligation | NONE in live evidence | F09-002. |
| Current fresh-agent lifecycle/task discovery | 07 Bootstrap responsibility | `/AGENTS.md` | PARTIAL / stale | F09-001. |

No mandatory product requirement was found with zero feature-task coverage. The blocking gaps are bootstrap/agent-readiness and one accepted technical security obligation not carried into task cards.

## Task Integrity

Checklist/card: 12 checklist IDs and 12 detailed cards, T001–T012, one-to-one.  
Duplicate/orphan: none found.  
Placeholders: no `TODO/TBD/implement later` found in task meaning.  
Checkboxes: all implementation tasks remain `[ ]`.  
Context readiness: cards themselves are generally self-contained, but repo-level Build context is blocked by F09-001.  
Real paths: planned new files sit under the materialized one-production-project/one-test-project structure; existing modified files exist.  
Known integrity defect: T006 `[P]` marker mismatch, F09-004.

## Dependency / Parallel Audit

Cycles: NONE found.  
Consumer-before-producer: NONE found in the reconstructed graph.  
Hidden dependency: no product-contract hidden dependency found; `T005 PASS → T008` is explicit.  
Unsafe `[P]`: no proven merge conflict among T002/T003/T004/T005. T006 is technically parallel with the legacy branch after T003+T004, but its marker is inconsistent between checklist and card/graph.  
Critical path: structurally valid.  
MVP-first: T009 is reachable only after T005 PASS → T008; this is explicit and correct because `.doc` is mandatory MVP input. A T005 FAIL correctly prevents claiming full MVP completion and returns the technical decision upstream.

Independent reconstructed order:

```text
T001
├─ T002 ──> T005(PASS) ────────────────┐
├─ T003 ──┐                            │
└─ T004 ──┴─> T006 ─> T007 ──────────> T008
                                             ↓
                              T009 → T010 → T011 → T012
```

No cycle was introduced by the serialized shared-UI tasks.

## Validation Audit

Core data validation is strong enough to begin implementation after blockers are removed: parser, normalizer, value policy, writer/readback, orchestrator, legacy path and failure/recovery slices each have targeted automated or observable checks. The writer is independently re-opened via Open XML, so success does not rely only on the ClosedXML writer object graph.

T009 correctly uses interactive verification rather than introducing an unapproved UI automation dependency. Phase-04/05 visual, keyboard, long-filename and scaling behavior is included in observable acceptance.

Known non-blocking validation boundary: clean Windows / no Office / no installed .NET / internet-disabled end-user proof is not a Build completion proof in T009; Phase 08 explicitly defers full environment evidence to later Test/Debug. This is acceptable for starting Build because the architecture and existing self-contained publish baseline preserve the obligation.

Blocking validation gap: path canonicalization has no task-level targeted validation (F09-003).

## Reuse Compliance / Missed Solutions

### Custom DOCX reader
CUSTOM_DECISION: direct Open XML SDK reader with project-owned source-preserving model  
OWNER_PHASE: 06_TECHNICAL_PLAN  
EXISTING_EVIDENCE: Phase 01/06 considered `docx2csv`; Phase 06 chose direct Open XML access to control source order, merges, provenance and unsupported-object findings.  
READY_ALTERNATIVE_FOUND: NO material replacement established  
SOURCE: `DocumentFormat.OpenXml 3.5.1` current NuGet package; Phase-06 prior-art record  
MATERIAL_IMPACT: NO  
ACTION: keep accepted decision.

### Legacy `.doc`
CUSTOM_DECISION: `DocSharp.Binary.Doc 0.21.0` behind a gate  
OWNER_PHASE: 06_TECHNICAL_PLAN  
EXISTING_EVIDENCE: Phase 06 compared LibreOffice, NPOI, Apache POI/HWPF and other paths; DocSharp is conditional only.  
READY_ALTERNATIVE_FOUND: NO unconditionally superior small portable replacement established  
SOURCE: https://www.nuget.org/packages/DocSharp.Binary.Doc/ — checked 2026-09-10; package is 0.21.0, supports modern .NET, converts Office 97–2003 binary formats without Office interop/native dependencies for the base package, and its own roadmap still calls out edge-case work.  
MATERIAL_IMPACT: NO — this reinforces the existing mandatory spike rather than invalidating it.  
ACTION: keep `LEGACY-DOC-001` hard gate.

### XLSX writer + independent validator
CUSTOM_DECISION: ClosedXML writer + Open XML SDK independent readback  
OWNER_PHASE: 06_TECHNICAL_PLAN  
EXISTING_EVIDENCE: Phase 06 separates convenient writing from independent serialized-output validation.  
READY_ALTERNATIVE_FOUND: NO material alternative found that removes the validation need  
SOURCE: current NuGet pages for `ClosedXML 0.105.1` and `DocumentFormat.OpenXml 3.5.1`, checked 2026-09-10  
MATERIAL_IMPACT: NO  
ACTION: keep accepted decision.

No material missed-solution blocker was found.

## Bureaucracy Audit

No one-task-one-PR rule, per-step approval gate, duplicate editable task registry, GitHub-Issue mirror, repeated setup phase or catch-all polish task is present. The 12-task plan is proportionate to the MVP and keeps tests with the behavior they validate.

Result: PASS, no process-bloat finding.

## Failure-Mode Walkthrough

| Path | Tasks | Completion/verification point | Result |
|---|---|---|---|
| Clean `.docx` | T002,T003,T004,T006,T007,T009 | DOCX E2E + independent XLSX validator + interactive success state | Covered |
| Clean `.doc` | T002,T005,T007,T008,T009 | required 12-category spike PASS + legacy E2E + same validator | Covered conditionally, correctly gated |
| Complex/ambiguous table | T003,T006,T007,T010 | normalizer tests + warning E2E/details | Covered |
| Image/unsupported object | T002,T010 | parser finding + warning E2E | Covered |
| No tables | T002,T010 | warning rather than false clean success | Covered |
| Unsupported/protected/corrupt input | T011 | failure E2E + absence of success output + retry UI | Covered |
| Save beside source fails | T012 | save-recovery E2E + alternate destination + collision test | Covered |
| Validation mismatch | T006,T007,T012 | validator FAIL blocks publication | Covered |
| Interrupted session | T007,T009 | no resume; source unchanged; incomplete result not success | Covered |
| Offline/portable runtime | existing self-contained baseline + T005/T008/T009 | final clean-machine proof deferred to Test/Debug | Covered for Build readiness, final evidence later |
| Visual/reflow/accessibility | T009–T012 | interactive visual/keyboard/scaling checks against VT-05-001 | Covered |
| Canonicalized filesystem paths | none explicit | none | NOT covered — F09-003 |

## Task Simulation

TASK_SIMULATION: STRUCTURAL_ONLY  
Independent repository-aware coding subagent/runtime was not used; no behavioral fresh-agent claim is made.

### Sample T001
TASK: source-preserving domain model/contracts  
GOAL: materialize already accepted internal model and contracts.  
SOURCES_TO_READ: `02_MVP_SPEC.md`, `03_PROJECT_RULES.md`, Phase-06 Data Model/Contracts, T001 card.  
FILES: exact `Model/`, `Word/`, `Conversion/`, `Excel/`, `Validation/` contract paths are named.  
DEPENDENCIES: NONE.  
NOT_IN_SCOPE: parser/normalizer/writer/UI behavior, persistence, new abstractions.  
VALIDATION: build + repo fast check.  
EXPECTED_EVIDENCE: compile success and declared-file diff.  
MISSING_CONTEXT: none in the task itself.

### Sample T009
TASK: clean interactive WPF flow  
GOAL: confirmed one-window FLOW-01 through `Готово`.  
SOURCES_TO_READ: Phase 04, Phase 05, visual target, Phase 06 WPF decisions, T009 card.  
FILES: `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `Assets/Icons.xaml`.  
DEPENDENCIES: T008.  
NOT_IN_SCOPE: warnings/errors, dark mode, wizard/settings, new UI packages.  
VALIDATION: real app interaction + fast check + output/validator evidence.  
EXPECTED_EVIDENCE: screenshots/notes and validated result.  
MISSING_CONTEXT: task card is sufficient, but repository entry instructions are stale — F09-001.

### Sample T012
TASK: save failure + alternate destination recovery  
GOAL: preserve validated output when default destination cannot be published.  
SOURCES_TO_READ: Phase-04 `STATE-ERROR-SAVE`, Phase-06 Output Publisher/TR-005/006, T012 card.  
FILES: exact publisher/orchestrator/MainWindow/test paths are named.  
DEPENDENCIES: T011.  
NOT_IN_SCOPE: installer, saved history, background retry.  
VALIDATION: E2E/unit + interactive recovery + fast check.  
EXPECTED_EVIDENCE: failure then validated unique output at alternate destination, source unchanged.  
MISSING_CONTEXT: accepted path-canonicalization security obligation is absent from the task — F09-003.

## Build Readiness

READY_FOR_BUILD: NO  
BUILD_BASE_HEAD: NONE — Build must not start from the current plan state.  
TASK_SOURCE: `docs/project/08_TASK_BREAKDOWN.md`  
FAST_CHECK: live baseline succeeds, but baseline success does not clear plan/context findings.  
Known non-blocking risks:
- T006 `[P]` checklist/card mismatch.
- full clean-machine/no-Office/offline portable proof remains later lifecycle validation.
- Windows 10 remains unpromised.
- `.doc` support remains contingent on `LEGACY-DOC-001`.

The blocking condition is not product-code failure; it is that the repository is not yet a self-consistent Build handoff and one accepted security obligation is missing from the task decomposition.

## Remediation Route

First return only to the earliest owner:

`RETURN_TO_PHASE: 07_BOOTSTRAP`

Phase 07 must address F09-001 and F09-002 without implementing product behavior. If supply-chain review requires a dependency/architecture change, route that specific decision to Phase 06.

After Phase-07 remediation, Phase 08 must re-check the new repository base and address F09-003 (and F09-004 while touching the plan), then Phase 09 must be re-run on the resulting current artifacts. Findings are not closed by promise.

## Handoff

NEXT_PHASE: NONE  
RETURN_TO_PHASE: 07_BOOTSTRAP

PHASE_09_COMPLETE
