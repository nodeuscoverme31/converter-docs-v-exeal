# 10 — Build

PHASE: 10_BUILD  
STATUS: IN_PROGRESS

## Baseline
Repository: `nodeuscoverme31/converter-docs-v-exeal`  
BUILD_BASE_HEAD: `9c8b8fa329bfa09cc565e963b7b368f29d01c81b`  
Start branch/worktree: `build/phase-10` (isolated Git branch from the READY Plan-Check head)  
PLAN_CHECK_STATUS: `READY_FOR_BUILD`  
FAST_CHECK: baseline GitHub Actions `bootstrap-check` run `34522289331` — PASS

## Execution Mode
Mode: `SINGLE_AGENT`  
Workers: one implementation executor; no separate coding subagent/worktree runtime available.

## Task Progress
| Task | Status | Validation | Fast Check | Commit |
|---|---|---|---|---|
| T001 | COMPLETE | build PASS | PASS — run 34523442594 | `175e5b04cb3d9c91a64905ad3e98e335cffa8a49` |
| T002 | COMPLETE | integration tests PASS | PASS — run 34525190440 | `a4061c00decdf45f544f0cba0fe32114c46c3d52` |
| T003 | COMPLETE | unit tests PASS | PASS — run 34564595328 | `95467b68c771b840dc7381fd3f3ab3cb99b60e77` |
| T004 | COMPLETE | unit tests PASS | PASS — run 34564873097 | `9959e70c6dde808efaf096dac75f69f8af0baf69` |
| T005 | COMPLETE | 12-category legacy spike PASS | PASS — run 34571752300 | `7261629630737d7f20c21036b68b46b03d88aa7d` |
| T006 | COMPLETE | integration readback tests PASS | PASS — run 34573733653 | `111356e991e0e9ff243ed281c73816c98fd55a19` |
| T007 | PENDING | — | — | — |
| T008 | PENDING | — | — | — |
| T009 | PENDING | — | — | — |
| T010 | PENDING | — | — | — |
| T011 | PENDING | — | — | — |
| T012 | PENDING | — | — | — |

## Task Records
### T001
STATUS: COMPLETE  
BASE_BEFORE: `17592cc9c2ec48d8f767b622e031226bd2a48433`  
FILES_CHANGED: `Model/DocumentModel.cs`, `Model/ConversionResult.cs`, five internal contract files under `Word/`, `Conversion/`, `Excel/`, `Validation/`  
REUSE_DECISION: REUSE accepted .NET type system/contracts; no new dependency or subsystem.  
VALIDATION: project build succeeded on PR CI.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34523442594` — PASS, including restore/audit/build/test/format/smoke/publish.  
DEVIATIONS: NONE.  
DECISIONS: typed read/legacy exceptions and validation result are kept internal with the accepted domain contracts.  
COMMIT: `175e5b04cb3d9c91a64905ad3e98e335cffa8a49`

### T002
STATUS: COMPLETE  
BASE_BEFORE: `175e5b04cb3d9c91a64905ad3e98e335cffa8a49`  
FILES_CHANGED: `Word/DocxDocumentReader.cs`, `tests/.../Fixtures/DocxFixtureFactory.cs`, `tests/.../Integration/DocxDocumentReaderTests.cs`, plus `Properties/AssemblyInfo.cs` to expose internal production types to the existing test assembly.  
REUSE_DECISION: REUSE `DocumentFormat.OpenXml 3.5.1` DOM/package APIs and T001 models/contracts; no parser dependency or second document model added.  
VALIDATION: `DocxDocumentReaderTests` exercised ordered paragraphs/tables, merge hints, irregular/empty cells, nested-table separation, Unicode and unsupported drawing findings; final full test step PASS.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34525190440` — PASS, including restore/audit/build/test/format/smoke/publish.  
DEVIATIONS: `Properties/AssemblyInfo.cs` was an unplanned but local testability detail (`InternalsVisibleTo`); no architecture or product behavior changed.  
DECISIONS: nested table text is not duplicated into parent cell text; vertical-merge state is read from the Open XML enum value; empty technical paragraphs do not create spurious line breaks.  
COMMIT: `a4061c00decdf45f544f0cba0fe32114c46c3d52`

### T003
STATUS: COMPLETE  
BASE_BEFORE: `5c16347077c8553f73b53071ab5d6dd8a2293d64`  
FILES_CHANGED: `Conversion/TableNormalizer.cs`, `tests/.../Unit/TableNormalizerTests.cs`.  
REUSE_DECISION: EXTEND the accepted T001 `TableModel`/`NormalizedTable` contract; no extra grid library or alternate table model.  
VALIDATION: TDD RED was observed in run `34564354460` because `TableNormalizer` did not yet exist; final tests cover regular grids, horizontal spans, vertical merge continuation, irregular rows, real empty source cells and orphan continuation ambiguity.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34564595328` — PASS, including build/test/format/smoke/publish.  
DEVIATIONS: NONE.  
DECISIONS: covered merge coordinates carry empty generated cells with no `SourceCellId`; unmatched vertical continuation is preserved as an anchor plus `TABLE_VERTICAL_MERGE_AMBIGUOUS` warning rather than guessed/dropped.  
COMMIT: `95467b68c771b840dc7381fd3f3ab3cb99b60e77`

### T004
STATUS: COMPLETE  
BASE_BEFORE: `642155c1993adb6f8a547c7bf328d1df2025ae7c`  
FILES_CHANGED: `Conversion/ValuePolicy.cs`, `tests/.../Unit/ValuePolicyTests.cs`.  
REUSE_DECISION: REUSE .NET BCL invariant parsing (`DateTime.TryParseExact`, `long.TryParse`) and the T001 `OutputValuePlan`; no locale/parser dependency added.  
VALIDATION: TDD RED run `34564735987` failed only because `ValuePolicy` was absent; tests then passed for leading-zero IDs, >15-digit IDs, formula-like prefixes, strict positive integers, strict ISO dates, ambiguous decimals/dates and exact whitespace/line breaks.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34564873097` — PASS, including build/test/format/smoke/publish.  
DEVIATIONS: NONE.  
DECISIONS: only positive integer text with at most 15 digits and exact `yyyy-MM-dd` dates are auto-typed in MVP; ambiguous decimal/date forms and `= + - @` prefixes stay exact text.  
COMMIT: `9959e70c6dde808efaf096dac75f69f8af0baf69`

### T005
STATUS: COMPLETE — `LEGACY-DOC-001 = PASS`  
BASE_BEFORE: `4783c1a9e07e6667e87bbacff4fe43ac675b60c5`  
FILES_CHANGED: `tests/.../Integration/LegacyDocSpikeTests.cs`, `tests/.../Fixtures/LegacyDoc/README.md`, 12 binary `.doc` corpus fixtures. Temporary one-shot fixture-authoring workflows were created only to overcome the text-only GitHub connector and were removed before final validation.  
REUSE_DECISION: REUSE `DocSharp.Binary.Doc 0.21.0` exactly as selected by Phase 06; no LibreOffice/Word/native process in the tested conversion path. LibreOffice was used only once to author deterministic binary test fixtures, then removed from the repository/runtime path.  
VALIDATION: initial RED run `34571100619` failed because fixtures were absent. After corpus materialization, run `34571391910` exercised all 12 categories and exposed one invalid test fixture: its image was an authoring-time `INCLUDEPICTURE` link, not embedded data. That fixture was rejected, not treated as a DocSharp failure. It was replaced with Apache POI `PngPicture.doc` at commit `671a20eb...`; upstream `TestPictures.testPictureDetectionWithPNG` establishes one embedded picture. Final run `34571752300` passed all tests.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34571752300` — PASS, including locked restore/audit/build/test/format/smoke/publish.  
DEVIATIONS: Task card listed only spike tests/fixtures; due the connected GitHub API being UTF-8 text-only, temporary CI fixture-authoring workflows were necessary to materialize binary `.doc` files. Both one-shot workflows were deleted before PASS. The image fixture provenance is recorded in the corpus README.  
DECISIONS: the selected DocSharp path is accepted for MVP legacy `.doc` input on the required corpus. Protected documents are detected from the FIB encryption flag before conversion; damaged compound files are rejected. No fallback engine was introduced.  
COMMIT: `7261629630737d7f20c21036b68b46b03d88aa7d`

### T006
STATUS: COMPLETE  
BASE_BEFORE: `2ce77783a5a6f34194ad7f3df7902984c0b15f52`  
FILES_CHANGED: `Excel/ExcelWorkbookWriter.cs`, `Validation/OpenXmlOutputValidator.cs`, `tests/.../Integration/XlsxWriteReadbackTests.cs`.  
REUSE_DECISION: REUSE `ClosedXML 0.105.1` only for serialization and `DocumentFormat.OpenXml 3.5.1` for independent reopen/readback; no second spreadsheet abstraction or dependency added.  
VALIDATION: RED run `34572026380` failed because writer/validator implementations did not yet exist. First implementation run `34572314169` exposed compile-only defects (`System.IO` imports and an invalid `CellValues` pattern); these were fixed without changing task semantics. Final tests verify deterministic `Таблица N`/`Контекст`, exact leading-zero/long/formula-like text, typed safe number/date values, and absence of generated formulas.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34573733653` — PASS, including restore/audit/build/test/format/smoke/publish.  
DIFF_SCOPE: compare `2ce77783... → 111356e9...` contains exactly the two declared production files plus the declared integration test.  
DEVIATIONS: NONE.  
DECISIONS: workbook generation and validation are independent at serialized-package level; publication remains blocked until T007 and therefore T006 writes only the requested output path supplied by its caller.  
COMMIT: `111356e991e0e9ff243ed281c73816c98fd55a19`

## Build Deviations
- T002 added `Properties/AssemblyInfo.cs` solely for test access to internal accepted contracts; no public API introduced.
- T005 used temporary one-shot GitHub Actions workflows solely to author/replace binary test fixtures because the connector cannot write arbitrary binary content. Those workflows were removed before the task PASS; they are not product/runtime dependencies.

## Upstream Returns
- NONE

## Observations For Phase 11
- T005's locally-authored image candidate was invalid because it was a linked `INCLUDEPICTURE`; the final corpus uses a provenance-backed embedded-image fixture instead.

## Final Build State
HEAD: current `build/phase-10` head  
Worktree: isolated remote branch  
Pending tasks: 6  
Fast check: PASS through T006 (`34573733653`)  
Known non-blocking issues: NONE from completed tasks; T007–T012 remain.

## Handoff
NEXT_PHASE: NONE  
RETURN_TO_PHASE: NONE
