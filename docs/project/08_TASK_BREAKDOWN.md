# 08 — Task Breakdown

PHASE: 08_TASK_BREAKDOWN  
STATUS: COMPLETE  
REPOSITORY: `nodeuscoverme31/converter-docs-v-exeal`  
BASE_HEAD: `35ce064a0c25f33a1803a6a08b8e0be1b164b498`  
BASE_WORKTREE_STATUS: `REMOTE_ONLY` — Phase 08 uses the live GitHub tree snapshot; no separate local worktree is open.  
PROJECT_MODE: GREENFIELD  
REMEDIATION_TRIGGER: `09_PLAN_CHECK.md` findings `F09-003`, `F09-004`

## Inputs Checked

Rechecked live versions at `BASE_HEAD`: `docs/project/01_PROJECT_INTENT.md`, `02_MVP_SPEC.md`, `03_PROJECT_RULES.md`, `04_PRODUCT_UX_DESIGN.md`, `05_VISUAL_UI_DESIGN.md`, `06_TECHNICAL_PLAN.md`, remediated `07_BOOTSTRAP.md`, `docs/design/05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.jpg`, `README.md`, `AGENTS.md`, actual `src/`, `tests/`, workflow and lockfiles. `09_PLAN_CHECK.md` was read only as the return trigger for `F09-003/F09-004`.

Selected Phase-05 source remains `05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.png`; repository projection is `docs/design/05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.jpg`.

The remediated live repo is consistent with Phase 07. `AGENT_CONTEXT_READY: PASS`. Product behavior is still not implemented.

## Live Repository Preflight

```text
REPOSITORY: nodeuscoverme31/converter-docs-v-exeal
BASE_HEAD: 35ce064a0c25f33a1803a6a08b8e0be1b164b498
BRANCH: main
WORKTREE_STATUS: REMOTE_ONLY
PROJECT_MODE: GREENFIELD
CANONICAL_DOCS: PASS — docs/project/01..09 present
AGENT_INSTRUCTIONS: PASS — /AGENTS.md present and points to 08 task source + 09 pre-Build gate
AGENT_CONTEXT_READY: PASS
VERIFIED_SETUP: dotnet restore WordToExcel.sln --locked-mode
VERIFIED_START: dotnet run --project src/WordToExcel.App/WordToExcel.App.csproj -c Release --no-build -- --bootstrap-smoke
VERIFIED_FAST_CHECK: dotnet build WordToExcel.sln -c Release --no-restore; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet format WordToExcel.sln --verify-no-changes --no-restore
VERIFIED_TEST_COMMAND: dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build
```

Actual scaffold: `App.xaml` already loads built-in WPF Fluent resources; `MainWindow.xaml` is the Phase-07 placeholder; test project contains only the bootstrap smoke.

## Verified Execution Baseline

Evidence: GitHub Actions `bootstrap-check`, run `34516489971`, on `BASE_HEAD`, conclusion `success`.

SETUP: PASS — locked restore with the current Phase-07 NuGet audit guard.  
START: PASS — `dotnet run --project src/WordToExcel.App/WordToExcel.App.csproj -c Release --no-build -- --bootstrap-smoke`  
FAST_CHECK: PASS — Phase-07 build + test + format chain above.  
TEST: PASS — `dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build`  
DEPENDENCY AUDIT: PASS at the current Phase-07 point-in-time baseline.  
PORTABLE_PUBLISH_BASELINE: PASS — `dotnet publish src/WordToExcel.App/WordToExcel.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false`

All tasks must preserve these commands, the accepted no-Office/no-network/no-service runtime boundary, and the Phase-06 security boundaries.

## MVP First

MVP_FIRST_COMPLETION_POINT: **T009**

After T009 the clean user flow works end-to-end for `.docx` and for `.doc` only after mandatory PASS `LEGACY-DOC-001`: choose/drop → explicit start → validated `.xlsx` → `Готово` with actual name/path. Warning/error/recovery completeness is closed by T010–T012.

## Story / Scenario Completion Criteria

### SCENARIO-01 — Successful Word → Excel
GOAL: one readable `.doc/.docx` → one working `.xlsx`.  
TASKS: T001–T009.  
INDEPENDENT_COMPLETION_CRITERIA: separate sheet per table; text in `Контекст`; dangerous values exact; publish only after independent validation; source unchanged; filesystem paths canonicalized before source read/output write; clean WPF flow available.  
INDEPENDENT_VERIFY: repo test/fast-check + portable publish + interactive clean `.docx` and corpus-approved `.doc`.

### SCENARIO-02 — Complex table
GOAL: merged/irregular table → rectangular grid without silent loss; ambiguity is visible.  
TASKS: T003,T004,T006,T007,T010.  
INDEPENDENT_COMPLETION_CRITERIA: merge/irregular/empty topology preserves content once; ambiguity not guessed; warning visible.  
INDEPENDENT_VERIFY: `TableNormalizerTests`, `XlsxWriteReadbackTests`, `WarningConversionTests`, fast-check.

### SCENARIO-03 — Unsupported object
GOAL: supported text/tables remain usable; omitted image/drawing/OLE is disclosed.  
TASKS: T002,T010.  
INDEPENDENT_COMPLETION_CRITERIA: unsupported-object finding exists and final state is `Готово с предупреждениями`.  
INDEPENDENT_VERIFY: embedded-image fixture + warning E2E + interactive details.

### SCENARIO-04 — Input/save failure
GOAL: unsupported/protected/corrupt input or save failure never becomes false success.  
TASKS: T007,T008,T011,T012.  
INDEPENDENT_COMPLETION_CRITERIA: typed error, no successful publish, source unchanged, understandable recovery; alternate destination is canonicalized before write and works when valid.  
INDEPENDENT_VERIFY: failure fixtures, E2E save/input tests, interactive recovery, fast-check.

## Dependency Graph

```text
T001
├─ T002 [P] ───────┬────────────> T007
│                  └─> T005 [P] ─┐
├─ T003 [P] ─┐                    │
└─ T004 [P] ─┴─> T006 [P] ──────┘
T002 + T006 ─────────────────────> T007
T005(PASS) + T007 ───────────────> T008
T008 → T009 → T010 → T011 → T012
```

Graph is acyclic. T010→T011→T012 is intentionally serialized because these tasks modify the same `MainWindow`/`ConversionOrchestrator` surfaces.

## Critical Path

Core: `T001 → {T002,T003,T004} → T006 → T007 → T008 → T009 → T010 → T011 → T012`  
Legacy gate: `T001 → T002 → T005(PASS) → T008`.  
T008 waits for both clean-core and legacy-gate branches.

## Parallel Opportunities

`T002 [P]`, `T003 [P]`, `T004 [P]` may run after T001: separate source/test files, no shared manifest/config changes. `T005 [P]` may overlap T003/T004/T006 once T002 exists. `T006 [P]` may run after T003+T004 while T005 is still running. T007+ are not parallel because they converge on shared orchestration/UI.

## Execution Checklist

- [ ] T001 Materialize source-preserving domain model and accepted internal contracts — `src/WordToExcel.App/Model/`, contract files in `Word/`, `Conversion/`, `Excel/`, `Validation/`
- [ ] T002 [P] Parse `.docx` into ordered `DocumentModel` with tables, text, nested relations and unsupported-object findings — `src/WordToExcel.App/Word/DocxDocumentReader.cs`
- [ ] T003 [P] Normalize Word table topology into a loss-aware rectangular logical grid — `src/WordToExcel.App/Conversion/TableNormalizer.cs`
- [ ] T004 [P] Apply lossless Excel value policy for text, safe numbers/dates and formula-like input — `src/WordToExcel.App/Conversion/ValuePolicy.cs`
- [ ] T005 [P] Execute `LEGACY-DOC-001` fidelity spike against the required `.doc` corpus — `tests/WordToExcel.Tests/Integration/LegacyDocSpikeTests.cs`
- [ ] T006 [P] Produce and independently validate XLSX from normalized document data — `src/WordToExcel.App/Excel/ExcelWorkbookWriter.cs`, `src/WordToExcel.App/Validation/OpenXmlOutputValidator.cs`
- [ ] T007 Publish a validated clean `.docx` end-to-end through detector/orchestrator/publisher with canonical source/output paths — `src/WordToExcel.App/Conversion/ConversionOrchestrator.cs`
- [ ] T008 Integrate approved `.doc → temporary .docx` adapter into the common pipeline — `src/WordToExcel.App/Word/LegacyDocConverter.cs`
- [ ] T009 Deliver clean interactive WPF flow through `Готово` — `src/WordToExcel.App/MainWindow.xaml`
- [ ] T010 Deliver warning flows for ambiguity, unsupported objects and no-table documents — `src/WordToExcel.App/MainWindow.xaml`, `tests/WordToExcel.Tests/E2E/WarningConversionTests.cs`
- [ ] T011 Deliver typed input failures for unsupported, protected and corrupt documents — `src/WordToExcel.App/Conversion/InputDetector.cs`, `tests/WordToExcel.Tests/E2E/InputFailureTests.cs`
- [ ] T012 Deliver output-save recovery with canonical alternate destination and no silent overwrite — `src/WordToExcel.App/Conversion/OutputPublisher.cs`, `tests/WordToExcel.Tests/E2E/SaveRecoveryTests.cs`

## Detailed Task Cards

### T001 — Source-preserving domain model and internal contracts
GOAL: Materialize the accepted Phase-06 model/contracts so all slices share provenance-aware data and product-level result.

IMPLEMENTS:
- SCENARIO: SCENARIO-01, SCENARIO-02
- SC: SC-002, SC-004
- REQ: REQ-004, REQ-005, REQ-007, REQ-009
- QREQ: QREQ-001, QREQ-002
- AC: prerequisite for AC-004-1/2, AC-005-1/2/3, AC-007-1, AC-009-1/2/3
- UX: product result remains `Success | Warning | Error`
- VISUAL: NONE
- TECH_DECISION: TR-004; CONTRACT-01..06; Data Model/Invariants

IN_SCOPE: `DocumentModel`, ordered blocks, paragraph/table/source-cell, normalized table/cell, value plan, provenance/findings, `ConversionResult`, and named interfaces.  
NOT_IN_SCOPE: parser/normalizer/writer/UI behavior; persistence; new abstractions.

FILES:
- CREATE: `src/WordToExcel.App/Model/DocumentModel.cs`, `Model/ConversionResult.cs`, `Word/IWordDocumentReader.cs`, `Word/ILegacyDocConverter.cs`, `Conversion/ITableNormalizer.cs`, `Excel/IExcelWorkbookWriter.cs`, `Validation/IOutputValidator.cs`
- MODIFY: NONE
- TEST: NONE
- OTHER: Phase-06 Data Model/Invariants/Interfaces.

CONSUMES: Phase-06 accepted model/contracts and `RULE-DATA-001..003`.  
PRODUCES: compile-time domain/contracts for T002–T008.  
DEPENDS_ON: NONE  
BLOCKS: T002,T003,T004,T005,T006,T007,T008

IMPLEMENTATION NOTES: keep contracts internal; exact source text/order/spans/findings/provenance; UI never consumes parser-specific exceptions.  
VALIDATION:
- TEST_REQUIRED: NO — compile-time contract task; behavior tested downstream
- TEST_LEVEL: compile/build
- TARGETED_VERIFY: `dotnet build WordToExcel.sln -c Release --no-restore`
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: accepted types/contracts compile without package/config changes
- PASS_CONDITION: build succeeds; no feature behavior or extra architecture added
DONE_WHEN: all accepted model elements/contracts exist and build passes.  
EVIDENCE: build output + diff limited to declared files.  
PARALLEL: NO — foundation.

### T002 — DOCX reader to ordered source-preserving model
GOAL: Read `.docx` as untrusted data into exact ordered `DocumentModel`.

IMPLEMENTS:
- SCENARIO: SCENARIO-01, SCENARIO-03
- SC: SC-002, SC-004
- REQ: REQ-003, REQ-005, REQ-007, REQ-008
- QREQ: QREQ-001, QREQ-004
- AC: AC-003-1, AC-005-3, AC-007-1, AC-008-1, AC-Q001-1
- UX: supplies context/findings
- VISUAL: NONE
- TECH_DECISION: DECISION-003; TR-004; CONTRACT-01

IN_SCOPE: source-order paragraphs/tables; empty cells; Unicode/line breaks; grid/span/vMerge hints; nested tables with parent relation; drawing/image/OLE finding; no active-content execution.  
NOT_IN_SCOPE: normalization, value typing, XLSX writing, `.doc`, UI.

FILES:
- CREATE: `src/WordToExcel.App/Word/DocxDocumentReader.cs`, `tests/WordToExcel.Tests/Integration/DocxDocumentReaderTests.cs`, `tests/WordToExcel.Tests/Fixtures/DocxFixtureFactory.cs`
- MODIFY: NONE
- TEST: `tests/WordToExcel.Tests/Integration/DocxDocumentReaderTests.cs`
- OTHER: `DocumentFormat.OpenXml 3.5.1`

CONSUMES: T001 model + `IWordDocumentReader`.  
PRODUCES: DOCX reader/findings for T005,T007,T010,T011.  
DEPENDS_ON: T001  
BLOCKS: T005,T007,T010,T011

IMPLEMENTATION NOTES: source read-only; no external resource following; nested table is separate `TableId`; corruption leaves via accepted typed error model.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: integration
- TARGETED_VERIFY: verified test command; require `DocxDocumentReaderTests`
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: fixtures give exact ordered blocks/table counts/content/provenance/nested relation/findings
- PASS_CONDITION: supported fixture content represented exactly or explicitly finding-marked; no active content executed
DONE_WHEN: one/many tables, text around tables, merge hints, irregular/nested/image/empty/Unicode cases covered.  
EVIDENCE: integration output + fixture/model assertions + source unchanged check.  
PARALLEL: YES — after T001, separate files from T003/T004.

### T003 — Loss-aware rectangular table normalization
GOAL: Convert `TableModel` to rectangular `NormalizedTable` without deleting, shifting or duplicating source content.

IMPLEMENTS:
- SCENARIO: SCENARIO-02
- SC: SC-002, SC-003, SC-004
- REQ: REQ-004, REQ-005
- QREQ: QREQ-001, QREQ-002
- AC: AC-004-1, AC-004-2, AC-Q001-1
- UX: ambiguity later becomes warning
- VISUAL: NONE
- TECH_DECISION: CONTRACT-03; merge/occupancy invariants

IN_SCOPE: horizontal spans, vertical merges, irregular rows, empty cells/rows, anchor rule, ambiguity finding.  
NOT_IN_SCOPE: parsing, value typing, XLSX, UI.

FILES:
- CREATE: `src/WordToExcel.App/Conversion/TableNormalizer.cs`, `tests/WordToExcel.Tests/Unit/TableNormalizerTests.cs`
- MODIFY: NONE
- TEST: `tests/WordToExcel.Tests/Unit/TableNormalizerTests.cs`
- OTHER: Phase-06 merge/empty-cell rules

CONSUMES: T001 table/normalized models + `ITableNormalizer`.  
PRODUCES: normalized grid/findings for T006/T007/T010.  
DEPENDS_ON: T001  
BLOCKS: T006,T007,T010

IMPLEMENTATION NOTES: content only in anchor; covered cells empty; ambiguity is not guessed.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: unit
- TARGETED_VERIFY: verified test command; require `TableNormalizerTests`
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: known tables yield expected dimensions/coordinates without disappearance/duplication
- PASS_CONDITION: deterministic cases exact; ambiguous case returns finding
DONE_WHEN: normal/span/vMerge/irregular/empty cases and preservation invariants pass.  
EVIDENCE: unit output + coordinate/finding assertions.  
PARALLEL: YES — separate surface after T001.

### T004 — Lossless Excel value policy
GOAL: Choose `Text | SafeNumber | SafeDate` only when source meaning is preserved; ambiguous/formula-like input stays text.

IMPLEMENTS:
- SCENARIO: SCENARIO-01, SCENARIO-02
- SC: SC-002, SC-003
- REQ: REQ-005, REQ-006
- QREQ: QREQ-001, QREQ-002
- AC: AC-005-1/2/3, AC-006-1/2, AC-Q002-1
- UX: no manual type controls
- VISUAL: NONE
- TECH_DECISION: Value Safety Policy / Formula safety

IN_SCOPE: leading zeros, long identifiers, safe numbers/dates, locale-sensitive ambiguity, Unicode/line breaks/significant spaces, `= + - @`.  
NOT_IN_SCOPE: header inference, formulas, user type settings, locale guessing.

FILES:
- CREATE: `src/WordToExcel.App/Conversion/ValuePolicy.cs`, `tests/WordToExcel.Tests/Unit/ValuePolicyTests.cs`
- MODIFY: NONE
- TEST: `tests/WordToExcel.Tests/Unit/ValuePolicyTests.cs`
- OTHER: Phase-06 Value Safety Policy

CONSUMES: T001 `OutputValuePlan`.  
PRODUCES: value plans for T006.  
DEPENDS_ON: T001  
BLOCKS: T006,T007

IMPLEMENTATION NOTES: `EXACT SOURCE VALUE > CONVENIENT EXCEL TYPE`; ambiguity/formula-like source stays text.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: unit
- TARGETED_VERIFY: verified test command; require `ValuePolicyTests`
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: each required fixture gets reversible safe mode/value
- PASS_CONDITION: `001234`, long codes, formula-like text unchanged; only reversible safe values typed
DONE_WHEN: all mandatory Phase-06 value fixtures covered.  
EVIDENCE: unit output + equivalence assertions.  
PARALLEL: YES — separate surface after T001.

### T005 — `LEGACY-DOC-001` fidelity spike
GOAL: Prove or reject `DocSharp.Binary.Doc 0.21.0` `.doc → temp .docx → DocumentModel` before production legacy support.

IMPLEMENTS:
- SCENARIO: SCENARIO-01
- SC: SC-002, SC-005
- REQ: REQ-001
- QREQ: QREQ-001, QREQ-003, QREQ-005
- AC: prerequisite for AC-001-1, AC-Q001-1, AC-Q003-1, AC-Q005-1
- UX: NONE
- VISUAL: NONE
- TECH_DECISION: DECISION-004; `LEGACY-DOC-001`

IN_SCOPE: required 12-category `.doc` corpus; DocSharp conversion; T002 parse; explicit truth comparison; evidence-backed PASS/FAIL.  
NOT_IN_SCOPE: production adapter; LibreOffice implementation; changing `.doc` requirement; masking mismatches.

FILES:
- CREATE: `tests/WordToExcel.Tests/Integration/LegacyDocSpikeTests.cs`; `tests/WordToExcel.Tests/Fixtures/LegacyDoc/` with `simple-table.doc`, `multiple-tables.doc`, `merged-cells.doc`, `irregular-rows.doc`, `unicode-cyrillic.doc`, `leading-zero-codes.doc`, `long-numeric-identifiers.doc`, `surrounding-text.doc`, `nested-table.doc`, `embedded-image.doc`, `protected.doc`, `damaged.doc`
- MODIFY: NONE
- TEST: `tests/WordToExcel.Tests/Integration/LegacyDocSpikeTests.cs`
- OTHER: existing `DocSharp.Binary.Doc 0.21.0`

CONSUMES: T001 model, T002 reader, Phase-06 spike criteria.  
PRODUCES: `LEGACY-DOC-001 = PASS|FAIL` evidence + corpus.  
DEPENDS_ON: T001,T002  
BLOCKS: T008 and any `.doc` support claim.

IMPLEMENTATION NOTES: corpus truth must be explicit; PASS requires no unmarked expected-content loss, correct Cyrillic, usable merge hints, unsupported-object detection, no Office/native external process. FAIL → Phase 06; no silent fallback.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: integration/spike
- TARGETED_VERIFY: verified test command; require all `LegacyDocSpikeTests` corpus criteria
- REPO_FAST_CHECK: after PASS
- OBSERVABLE_RESULT: each corpus item has explicit truth and result
- PASS_CONDITION: every Phase-06 mandatory legacy criterion passes; otherwise outcome is FAIL and return
DONE_WHEN: all 12 categories exercised with evidence-backed PASS/FAIL.  
EVIDENCE: per-fixture comparison + mismatch list if any + proof no external Office conversion.  
PARALLEL: YES — after T002, isolated from T003/T004/T006.

### T006 — XLSX writer with independent readback
GOAL: Write temporary `.xlsx` from normalized data and independently verify structure/values before publication.

IMPLEMENTS:
- SCENARIO: SCENARIO-01, SCENARIO-02
- SC: SC-002, SC-003, SC-004
- REQ: REQ-002, REQ-003, REQ-005, REQ-006, REQ-007
- QREQ: QREQ-001, QREQ-002
- AC: AC-002-1, AC-003-1, AC-005-1/2/3, AC-006-1/2, AC-007-1, AC-Q001-1, AC-Q002-1
- UX: output layout only
- VISUAL: NONE
- TECH_DECISION: DECISION-005; CONTRACT-04/05; TR-005

IN_SCOPE: ClosedXML temp workbook; deterministic `Таблица N`; `Контекст` with `Порядок/Тип/Содержимое`; OpenXML independent reopen/check; exact values; no accidental formulas.  
NOT_IN_SCOPE: final filesystem publish/naming; UI; `.doc`.

FILES:
- CREATE: `src/WordToExcel.App/Excel/ExcelWorkbookWriter.cs`, `src/WordToExcel.App/Validation/OpenXmlOutputValidator.cs`, `tests/WordToExcel.Tests/Integration/XlsxWriteReadbackTests.cs`
- MODIFY: NONE
- TEST: `tests/WordToExcel.Tests/Integration/XlsxWriteReadbackTests.cs`
- OTHER: `ClosedXML 0.105.1`, `DocumentFormat.OpenXml 3.5.1`

CONSUMES: T001 writer/validator contracts, T003 normalized tables, T004 value plans.  
PRODUCES: temp writer + independent validation result for T007.  
DEPENDS_ON: T001,T003,T004  
BLOCKS: T007

IMPLEMENTATION NOTES: writer never parses Word; validator reopens serialized package independently; generated metadata stays distinguishable; no formula generation.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: integration
- TARGETED_VERIFY: verified test command; require `XlsxWriteReadbackTests`
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: valid OpenXML package with expected sheets/context/cell values
- PASS_CONDITION: mismatch → validation FAIL; exact/dangerous values round-trip unchanged
DONE_WHEN: separate sheets/context/exact values/no-formula invariants pass independent readback.  
EVIDENCE: tests + validator assertions + temp workbook inspection.  
PARALLEL: YES — after T003/T004, separate from T002/T005.

### T007 — Clean DOCX orchestration and safe publication
GOAL: First complete core `.docx` pipeline: canonicalize path → detect → read → normalize/value-plan → temp XLSX → validate → safe publish → `ConversionResult`.

IMPLEMENTS:
- SCENARIO: SCENARIO-01
- SC: SC-001, SC-002, SC-003, SC-005
- REQ: REQ-001, REQ-002, REQ-003, REQ-005, REQ-006, REQ-007, REQ-009
- QREQ: QREQ-001, QREQ-002, QREQ-003, QREQ-004
- AC: AC-001-2, AC-002-1, AC-003-1, AC-005-1/2/3, AC-006-1/2, AC-007-1, AC-009-1, AC-Q001-1, AC-Q002-1, AC-Q004-1
- UX: clean success result + output beside source
- VISUAL: NONE
- TECH_DECISION: TR-005/006/007; Orchestrator/Input Detector/Output Publisher; Phase-06 Security Boundary #6 — canonicalize file paths before read/write

IN_SCOPE: clean `.docx` detection; canonical absolute source path before source file I/O; canonical default output/directory path before publish file I/O; one session; unique temp dir/cleanup; source read-only; validation gate; output beside source; `Name (1).xlsx` collision policy; success with actual path.  
NOT_IN_SCOPE: `.doc` production path, warning UI, invalid-input UI, alternate destination UI, new filesystem sandbox/policy beyond accepted path canonicalization.

FILES:
- CREATE: `src/WordToExcel.App/Conversion/InputDetector.cs`, `Conversion/OutputPublisher.cs`, `Conversion/ConversionOrchestrator.cs`, `tests/WordToExcel.Tests/E2E/DocxConversionTests.cs`, `tests/WordToExcel.Tests/Unit/OutputPublisherTests.cs`
- MODIFY: NONE
- TEST: listed E2E/unit files
- OTHER: Phase-06 Security Boundaries + Phase-07 publish baseline

CONSUMES: T002,T003,T004,T006 + T001 result.  
PRODUCES: clean `.docx` orchestrator and publisher for later tasks, with canonical source/default-output path handling at the filesystem boundary.  
DEPENDS_ON: T001,T002,T003,T004,T006  
BLOCKS: T008,T009,T010,T011,T012

IMPLEMENTATION NOTES: canonicalize accepted source/default destination paths before reading/writing and use canonical paths for filesystem identity/safety decisions; publish only after validator PASS; never overwrite source; no active content execution/network/queue/service. Do not invent a broader path-access policy that Phase 06 did not approve.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: E2E + unit
- TARGETED_VERIFY: verified test command; require `DocxConversionTests` + `OutputPublisherTests`, including equivalent path forms containing relative/`.`/`..` segments that resolve to the same canonical source/destination
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: clean DOCX through a non-canonical-but-valid path form resolves to one canonical source path and produces one validated collision-safe XLSX at the canonical destination; source unchanged
- PASS_CONDITION: source/default destination are canonical before file I/O; equivalent path forms do not bypass collision/source-safety decisions; expected sheets/context/values pass; validator failure cannot publish success
DONE_WHEN: clean `.docx` core path, path canonicalization, cleanup and naming are verified.  
EVIDENCE: E2E output + canonical-path assertions + source equality + validator + collision test.  
PARALLEL: NO — core convergence.

### T008 — Production legacy DOC adapter after spike PASS
GOAL: Route `.doc` through `ILegacyDocConverter` to temporary `.docx` and reuse the T007 common pipeline, only after T005 PASS.

IMPLEMENTS:
- SCENARIO: SCENARIO-01
- SC: SC-001, SC-002, SC-005
- REQ: REQ-001, REQ-005, REQ-009
- QREQ: QREQ-001, QREQ-003, QREQ-004, QREQ-005
- AC: AC-001-1, AC-005-1/2/3, AC-009-1, AC-Q001-1, AC-Q003-1, AC-Q004-1, AC-Q005-1
- UX: `.doc` same flow as `.docx`
- VISUAL: NONE
- TECH_DECISION: DECISION-004; CONTRACT-02; TR-003; reuse T007 canonical source-path boundary

IN_SCOPE: production DocSharp adapter; temp DOCX lifecycle; `InputKind.Doc`; common canonical source-path handling, reader/normalizer/writer/validator/publisher; typed legacy conversion failure.  
NOT_IN_SCOPE: alternate parser, LibreOffice fallback, new runtime dependency, separate Excel behavior.

FILES:
- CREATE: `src/WordToExcel.App/Word/LegacyDocConverter.cs`, `tests/WordToExcel.Tests/E2E/LegacyDocConversionTests.cs`
- MODIFY: `src/WordToExcel.App/Conversion/ConversionOrchestrator.cs`
- TEST: `tests/WordToExcel.Tests/E2E/LegacyDocConversionTests.cs`
- OTHER: T005 corpus, existing DocSharp package

CONSUMES: T005 PASS, T001 contract, T002 reader, T007 pipeline/path boundary.  
PRODUCES: full `.doc` route for T009–T012.  
DEPENDS_ON: T005=PASS,T007  
BLOCKS: T009 and full input-format completion.

IMPLEMENTATION NOTES: T005 FAIL means do not execute; temp `.docx` is internal; source read-only; source path uses the already verified T007 canonicalization boundary; no second normalizer/writer path.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: E2E
- TARGETED_VERIFY: verified test command; require `LegacyDocConversionTests`
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: approved `.doc` produces same semantic workbook invariants as equivalent `.docx`
- PASS_CONDITION: no Office/external process; final XLSX passes same validator; legacy source path cannot bypass the common canonical path handling
DONE_WHEN: legacy route is common-pipeline behavior.  
EVIDENCE: E2E + validator + source unchanged + common-path-boundary assertion.  
PARALLEL: NO — shared orchestrator + hard gate.

### T009 — Clean interactive WPF flow
GOAL: Replace placeholder with confirmed Windows Native/Fluent clean flow: choose/drop → selected file → explicit convert → processing → `Готово` + result path/actions.

IMPLEMENTS:
- SCENARIO: SCENARIO-01
- SC: SC-001, SC-003, SC-005
- REQ: REQ-001, REQ-002, REQ-009
- QREQ: QREQ-003, QREQ-005
- AC: AC-001-1/2, AC-002-1, AC-009-1, AC-Q003-1, AC-Q005-1
- UX: FLOW-01; SURFACE-01/02; INITIAL/FILE-SELECTED/PROCESSING/SUCCESS
- VISUAL: `docs/design/05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.jpg`; `VT-05-001-WINDOWS-NATIVE`
- TECH_DECISION: DECISION-001/002/006

IN_SCOPE: drag/drop + filtered system picker; filename/path; explicit button; indeterminate processing/no fake %; success; actual path; `Открыть папку`; `Преобразовать другой файл`; keyboard; long filename/reflow/text scaling; small local vector cues.  
NOT_IN_SCOPE: warning/error states, dark mode, Mica blocker, wizard/settings, auto-start drop, external UI/icon packages.

FILES:
- CREATE: `src/WordToExcel.App/Assets/Icons.xaml`
- MODIFY: `src/WordToExcel.App/App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`
- TEST: NONE — no accepted UI automation framework
- OTHER: Phase-04/05 docs + visual target

CONSUMES: T008 full orchestrator + `ConversionResult`.  
PRODUCES: first user-operable clean vertical slice; UI surface for T010–T012.  
DEPENDS_ON: T008  
BLOCKS: T010,T011,T012

IMPLEMENTATION NOTES: one-window hierarchy; UI sees product result only; no MVVM/DI framework merely for this window; third-party icon asset requires license/notice review; pixel-perfect not required.  
VALIDATION:
- TEST_REQUIRED: NO — adding UI automation would be a new dependency; core conversion is automated elsewhere
- TEST_LEVEL: interactive smoke + compile
- TARGETED_VERIFY: run app; picker/drop known clean `.docx` and approved `.doc`; convert; inspect success/path/open-folder
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: clean flow completes without technical settings
- PASS_CONDITION: both formats reach success; primary action/status survive long filename/text scaling; status not color-only
DONE_WHEN: clean FLOW-01 matches accepted UX and uses real orchestrator.  
EVIDENCE: fast-check + screenshots/notes + output/validator evidence.  
PARALLEL: NO — shared UI.

### T010 — Warning and partial-result flows
GOAL: Make nonfatal ambiguity/unsupported content visible without hiding usable Excel output.

IMPLEMENTS:
- SCENARIO: SCENARIO-02, SCENARIO-03
- SC: SC-002, SC-003, SC-004
- REQ: REQ-004, REQ-008, REQ-009
- QREQ: QREQ-001
- AC: AC-004-1/2, AC-008-1, AC-009-2, AC-Q001-1
- UX: FLOW-02/03; STATE-PARTIAL; SURFACE-03
- VISUAL: warning state of `VT-05-001-WINDOWS-NATIVE`
- TECH_DECISION: WARNING-UNSUPPORTED-OBJECT; WARNING-AMBIGUOUS-TABLE; CONTRACT-06

IN_SCOPE: findings→Warning; `Готово с предупреждениями`; summary/details; result/open-folder remain available; image/object/ambiguity messages; no-table edge uses existing Warning with `Таблицы не найдены` rather than false full success; accessible text+icon.  
NOT_IN_SCOPE: OCR, object rendering, fourth status, diagnostic logging platform.

FILES:
- CREATE: `tests/WordToExcel.Tests/E2E/WarningConversionTests.cs`
- MODIFY: `src/WordToExcel.App/Conversion/ConversionOrchestrator.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `Assets/Icons.xaml`
- TEST: `tests/WordToExcel.Tests/E2E/WarningConversionTests.cs`
- OTHER: Phase-04 SURFACE-03

CONSUMES: T002 findings, T003 ambiguity, T009 UI.  
PRODUCES: product Warning path.  
DEPENDS_ON: T009  
BLOCKS: T011

IMPLEMENTATION NOTES: Warning only when supported data preserved; validation mismatch is Error; product language only; no-table adds no new status.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: E2E + interactive UI
- TARGETED_VERIFY: verified test command requiring `WarningConversionTests`; open details interactively
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: image/ambiguous fixture yields usable XLSX only with warning; no-table never shows full success
- PASS_CONDITION: omitted/ambiguous item named and supported data validate
DONE_WHEN: scenario 02/03 warning paths and no-table non-false-success are observable.  
EVIDENCE: E2E + validator + warning screenshot/notes.  
PARALLEL: NO — shared orchestration/UI.

### T011 — Typed input failure and recovery
GOAL: Reject unsupported/protected/corrupt inputs without successful workbook and expose accepted error/retry UI.

IMPLEMENTS:
- SCENARIO: SCENARIO-04
- SC: SC-004
- REQ: REQ-009, REQ-010
- QREQ: QREQ-001, QREQ-004
- AC: AC-009-3, AC-010-1/2, AC-Q004-1
- UX: FLOW-04; ERROR-FORMAT/PROTECTED/CORRUPT
- VISUAL: error state of `VT-05-001-WINDOWS-NATIVE`
- TECH_DECISION: ERROR-INPUT-UNSUPPORTED/PROTECTED/CORRUPT/LEGACY-CONVERT; preserve T007 canonical source-path boundary

IN_SCOPE: extension/container mismatch, damaged container, password/encryption detection where supported, legacy conversion failure mapping, ordinary-language error, choose-other recovery, source unchanged.  
NOT_IN_SCOPE: password recovery/entry, document repair, save error (T012).

FILES:
- CREATE: `tests/WordToExcel.Tests/E2E/InputFailureTests.cs`, `tests/WordToExcel.Tests/Fixtures/Invalid/not-word.docx`, `corrupt.docx`, `protected.docx`
- MODIFY: `src/WordToExcel.App/Conversion/InputDetector.cs`, `Conversion/ConversionOrchestrator.cs`, `Word/DocxDocumentReader.cs`, `Word/LegacyDocConverter.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `Assets/Icons.xaml`
- TEST: `tests/WordToExcel.Tests/E2E/InputFailureTests.cs`
- OTHER: T005 legacy invalid fixtures

CONSUMES: T008 input paths + T009 UI.  
PRODUCES: typed Error behavior.  
DEPENDS_ON: T010 — serialization on shared orchestrator/UI.  
BLOCKS: T012

IMPLEMENTATION NOTES: extension alone is not success; no partial unreadable output as success; no stack traces; no password feature; do not bypass T007 canonical source-path handling for invalid/error routes.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: E2E/integration + interactive
- TARGETED_VERIFY: verified test command requiring `InputFailureTests`
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: invalid/protected/corrupt fixtures give typed error, no success file, retry action
- PASS_CONDITION: no false success; source unchanged; Phase-04 error category text
DONE_WHEN: AC-010-1/2 and AC-009-3 covered with representative inputs.  
EVIDENCE: tests + absence of final success + source equality + error-state notes.  
PARALLEL: NO — shared input/orchestrator/UI.

### T012 — Save failure and alternate-destination recovery
GOAL: If validated XLSX cannot publish beside source, show save error, canonicalize the alternate destination before write, allow an accessible destination, and retry with all safety gates intact.

IMPLEMENTS:
- SCENARIO: SCENARIO-04
- SC: SC-004
- REQ: REQ-002, REQ-009
- QREQ: QREQ-004, QREQ-005
- AC: AC-002-1, AC-009-3, AC-Q004-1, AC-Q005-1
- UX: STATE-ERROR-SAVE; Output Location/Naming UX
- VISUAL: error/recovery state
- TECH_DECISION: ERROR-OUTPUT-WRITE; TR-005/006; Output Publisher; Phase-06 Security Boundary #6 — canonicalize file paths before read/write

IN_SCOPE: inaccessible destination failure; user save error; system destination selection without new package; canonical absolute alternate destination before output file I/O; safe retry; collision-safe naming; no unvalidated success.  
NOT_IN_SCOPE: installer, remembered destination/history, background retry, new filesystem sandbox/policy beyond accepted path canonicalization.

FILES:
- CREATE: `tests/WordToExcel.Tests/E2E/SaveRecoveryTests.cs`
- MODIFY: `src/WordToExcel.App/Conversion/OutputPublisher.cs`, `Conversion/ConversionOrchestrator.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`
- TEST: `tests/WordToExcel.Tests/E2E/SaveRecoveryTests.cs`, `tests/WordToExcel.Tests/Unit/OutputPublisherTests.cs`
- OTHER: Phase-06 Security Boundaries; no new dependency

CONSUMES: T011 error UI + T006/T007 validated temp-output and canonical-path contract.  
PRODUCES: complete save-recovery flow using the same canonical output-path safety rule.  
DEPENDS_ON: T011  
BLOCKS: NONE

IMPLEMENTATION NOTES: canonicalize the selected alternate directory/output path before write and collision decisions; never overwrite existing output; validation failure never becomes success; no destination persistence; retry must preserve validation/source-read-only rules.  
VALIDATION:
- TEST_REQUIRED: YES
- TEST_LEVEL: integration/E2E + interactive
- TARGETED_VERIFY: verified test command requiring `SaveRecoveryTests` + `OutputPublisherTests`, including an alternate path containing equivalent relative/`.`/`..` segments
- REPO_FAST_CHECK: Phase-07 fast-check
- OBSERVABLE_RESULT: inaccessible target → save error; valid alternate target is canonicalized → validated unique XLSX at the resolved destination
- PASS_CONDITION: alternate destination is canonical before file I/O; equivalent path forms cannot bypass collision/source safety; no overwrite/source mutation/false success
DONE_WHEN: Phase-04 save-error recovery and canonical alternate-destination handling are observable and testable.  
EVIDENCE: save/retry tests + canonical-path assertions + unique-name + source check + recovery notes.  
PARALLEL: NO — shared final recovery surface.

## Coverage Map

```text
SCENARIO-01 → T001,T002,T003,T004,T005,T006,T007,T008,T009
SCENARIO-02 → T003,T004,T006,T007,T010
SCENARIO-03 → T002,T010
SCENARIO-04 → T007,T008,T011,T012

SC-001 → T007,T008,T009
SC-002 → T001,T002,T003,T004,T005,T006,T007,T008,T010
SC-003 → T003,T004,T006,T007,T009,T010
SC-004 → T001,T003,T006,T010,T011,T012
SC-005 → T005,T007,T008,T009

REQ-001 → T005,T007,T008,T009
REQ-002 → T006,T007,T009,T012
REQ-003 → T002,T006,T007
REQ-004 → T003,T010
REQ-005 → T001,T002,T003,T004,T006,T007,T008
REQ-006 → T004,T006,T007
REQ-007 → T001,T002,T006,T007
REQ-008 → T002,T010
REQ-009 → T001,T007,T008,T009,T010,T011,T012
REQ-010 → T011

QREQ-001 → T001,T002,T003,T004,T005,T006,T007,T008,T010,T011
QREQ-002 → T001,T003,T004,T006,T007
QREQ-003 → T005,T007,T008,T009
QREQ-004 → T002,T007,T008,T011,T012
QREQ-005 → T005,T008,T009,T012

AC-001-1 → T005,T008,T009
AC-001-2 → T007,T009
AC-002-1 → T006,T007,T009,T012
AC-003-1 → T002,T006,T007
AC-004-1 → T003,T006,T010
AC-004-2 → T003,T010
AC-005-1 → T004,T006,T007,T008
AC-005-2 → T004,T006,T007,T008
AC-005-3 → T002,T004,T006,T007,T008
AC-006-1 → T004,T006,T007
AC-006-2 → T004,T006,T007
AC-007-1 → T002,T006,T007
AC-008-1 → T002,T010
AC-009-1 → T007,T008,T009
AC-009-2 → T010
AC-009-3 → T011,T012
AC-010-1 → T011
AC-010-2 → T011
AC-Q001-1 → T002,T003,T004,T005,T006,T007,T008,T010,T011
AC-Q002-1 → T004,T006,T007
AC-Q003-1 → T005,T008,T009
AC-Q004-1 → T007,T008,T011,T012
AC-Q005-1 → T005,T008,T009,T012

FLOW-01 → T009
FLOW-02 → T003,T010
FLOW-03 → T002,T010
FLOW-04 → T011,T012
STATE-INITIAL/FILE-SELECTED/PROCESSING/SUCCESS → T009
STATE-PARTIAL → T010
STATE-ERROR-FORMAT/PROTECTED/CORRUPT → T011
STATE-ERROR-SAVE → T012
STATE-INTERRUPTED → T007,T009
VT-05-001-WINDOWS-NATIVE → T009,T010,T011,T012
TR-001 → T007,T008,T009
TR-002 → T009 + preserved Phase-07 publish baseline
TR-003 → T005,T008
TR-004 → T001,T002
TR-005 → T006,T007,T012
TR-006 → T007,T012
TR-007 → T002,T007,T008
SECURITY BOUNDARY #6 — canonicalize file paths before read/write → T007,T012; reused by T008/T011 through the common pipeline
```

Coverage review: all mandatory scenario/success/requirement/quality/acceptance obligations are mapped; every task has an obligation; UX/visual states are attached to concrete slices; the previously missing Phase-06 path-canonicalization obligation is now explicitly assigned and validated; OCR/batch/future formats have no task.

## Explicitly Not Planned

OCR/image-to-table; batch conversion; universal office conversion; exact Word visual reproduction; Office COM/automatic Office install; cloud/API/accounts/sync/telemetry; database/queue/service/web server; installer/MSIX as only delivery; single-file EXE gate; Electron/WebView/WinUI migration; enterprise UI kit; Word-text formula generation; automatic header inference/forced ListObject; dark-mode requirement; Windows 10 promise; Cancel/fake progress without evidence; GitHub Issues as second registry; one-task-one-branch/PR policy.

## Build-Time Confirmations

- `LEGACY-DOC-001` is a hard gate. T005 PASS is required before T008; FAIL returns to Phase 06 and no fallback dependency is added silently.
- Each T005 corpus file needs explicit expected truth/provenance; unknown-content files cannot count toward PASS.
- Phase-06 Security Boundary #6 is not optional: T007 owns canonicalization before source/default-output I/O and T012 owns canonicalization of an alternate output destination; T008/T011 must reuse the common source-path boundary rather than create a bypass.
- Path canonicalization is the accepted obligation. Build must not silently expand it into a new filesystem sandbox, allowlist or permission model without an owning technical decision.
- No-table documents use existing `Warning` rather than a fourth status: Phase 02 forbids false table-success and Phase 04 defines three outcomes. Phase 09 must reject this mapping if it determines it changes approved UX semantics.
- Local vector geometry may be implementation detail only without external package/asset. Third-party assets require license/notice verification.
- T009 may adjust bootstrap window dimensions only to satisfy confirmed reflow/text-scaling/action visibility, not invent a new navigation/layout model.
- Do not invent performance/capacity limits in Build. Measurement belongs to later Test/Debug unless a concrete implementation blocker appears.
- Windows 10 remains unpromised; do not change stack to chase it during Build.
- Full clean-machine/no-Office/internet-disabled product evidence belongs to later Test/Debug; Build must preserve the architecture and self-contained publish baseline.
- Behavioral fresh-agent task-readiness test was NOT EXECUTED; readiness here is structural only.

## Fresh-Agent Task Readiness

Structural result: PASS for T001–T012. Each card states outcome, obligations, real files, sources/accepted decisions, scope guard, consumes/produces, validation, dependencies and evidence. T007/T012 now make the accepted filesystem path-canonicalization boundary explicit, so a fresh Build agent does not have to infer this security work from Phase 06. Behavioral task-readiness with an independent repository-aware coding agent was not executed and is not claimed.

## Internal Self-Check

Repo/base: artifacts 01–07, remediated bootstrap state and actual scaffold/instructions read; `BASE_HEAD` fixed at `35ce064a0c25f33a1803a6a08b8e0be1b164b498`; Phase-07 commands and successful current-base CI verified; `AGENT_CONTEXT_READY: PASS`.  
Coverage: all SCENARIO/SC/REQ/QREQ/AC plus UX/visual obligations mapped; accepted Security Boundary #6 is explicitly mapped to T007/T012; no orphan/out-of-scope task.  
Granularity: one primary outcome/task; T001 only justified foundation; T005 bounded required spike; no catch-all backend/UI/tests/polish task.  
Dependencies: acyclic; `[P]` surfaces checked; T006 is consistently marked `[P]` in graph/checklist/card; MVP-first T009; shared UI/orchestrator work serialized.  
Execution readiness: checklist and cards are 1:1 T001–T012; exact paths supplied; placeholder markers absent; Bootstrap not duplicated; no production implementation executed.

## Handoff

NEXT_PHASE: 09_PLAN_CHECK  
RETURN_TO_PHASE: NONE

Pass to Phase 09: artifacts `01`–`08`; repo + `BASE_HEAD`; Phase-07 verified commands; checklist/cards; dependency graph/critical path/parallel opportunities; coverage map including Security Boundary #6; `MVP_FIRST_COMPLETION_POINT: T009`; Build-Time Confirmations including `LEGACY-DOC-001` and path canonicalization.

PHASE_08_COMPLETE
