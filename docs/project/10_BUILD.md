# 10 — Build

PHASE: 10_BUILD  
STATUS: READY_FOR_INTERACTIVE_SMOKE

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
| Task | Status | Validation | Fast Check | Completion Commit |
|---|---|---|---|---|
| T001 | COMPLETE | build PASS | PASS — run `34523442594` | `175e5b04cb3d9c91a64905ad3e98e335cffa8a49` |
| T002 | COMPLETE | integration tests PASS | PASS — run `34525190440` | `a4061c00decdf45f544f0cba0fe32114c46c3d52` |
| T003 | COMPLETE | unit tests PASS | PASS — run `34564595328` | `95467b68c771b840dc7381fd3f3ab3cb99b60e77` |
| T004 | COMPLETE | unit tests PASS | PASS — run `34564873097` | `9959e70c6dde808efaf096dac75f69f8af0baf69` |
| T005 | COMPLETE | 12-category legacy spike PASS | PASS — run `34571752300` | `7261629630737d7f20c21036b68b46b03d88aa7d` |
| T006 | COMPLETE | XLSX write/readback PASS | PASS — run `34573733653` | `111356e991e0e9ff243ed281c73816c98fd55a19` |
| T007 | COMPLETE | DOCX E2E + publisher PASS | PASS — run `34574622457` | `1f82e7af7722612454d5b21ad04cbc21e979233f` |
| T008 | COMPLETE | legacy DOC E2E PASS | PASS — run `34575233210` | `89eb4bbb6aed9d6aebcabe99561d632b5de0744b` |
| T009 | COMPLETE | clean WPF flow builds and full suite PASS | PASS — run `34576464106` | `4f1d9f94e463f78531adc7f99ac217049b062574` |
| T010 | COMPLETE | warning E2E PASS | PASS — run `34577209657` | `cdaefe70b22d4f2568e0273b585ee6d20337970f` |
| T011 | COMPLETE | typed input-failure E2E PASS | PASS — run `34578073133` | `ed5d44755df6a7767739c659b4d95a97786c9594` |
| T012 | COMPLETE | save-recovery E2E + publisher unit tests PASS | PASS — run `34578888238` | `4e1ee91d6ca71b45f29fa594af9ca13034b28292` |

## Task Records

### T001 — Domain model and internal contracts
STATUS: COMPLETE  
RESULT: accepted source-preserving models, product result and internal reader/normalizer/writer/validator contracts compile.  
VALIDATION: build PASS; downstream tests exercise the contracts.  
FAST_CHECK: `34523442594` — PASS.  
COMMIT: `175e5b04cb3d9c91a64905ad3e98e335cffa8a49`

### T002 — DOCX reader
STATUS: COMPLETE  
RESULT: ordered paragraphs/tables, merge hints, irregular and empty cells, nested-table separation, Unicode and unsupported drawing findings are represented without executing active content.  
VALIDATION: `DocxDocumentReaderTests` PASS.  
FAST_CHECK: `34525190440` — PASS.  
COMMIT: `a4061c00decdf45f544f0cba0fe32114c46c3d52`

### T003 — Table normalization
STATUS: COMPLETE  
RESULT: regular grids, horizontal spans, vertical continuations, irregular rows and ambiguity findings normalize to a deterministic rectangular model without silently guessing ambiguous merges.  
VALIDATION: `TableNormalizerTests` PASS.  
FAST_CHECK: `34564595328` — PASS.  
COMMIT: `95467b68c771b840dc7381fd3f3ab3cb99b60e77`

### T004 — Lossless Excel value policy
STATUS: COMPLETE  
RESULT: leading-zero IDs, >15-digit identifiers and formula-like text remain text; only intentionally safe integer/ISO-date cases are typed.  
VALIDATION: `ValuePolicyTests` PASS.  
FAST_CHECK: `34564873097` — PASS.  
COMMIT: `9959e70c6dde808efaf096dac75f69f8af0baf69`

### T005 — Legacy `.doc` fidelity spike
STATUS: COMPLETE — `LEGACY-DOC-001 = PASS`  
RESULT: selected `DocSharp.Binary.Doc 0.21.0` path passed the required 12-category legacy corpus; protected and damaged legacy containers are rejected. No Office/LibreOffice runtime fallback was introduced.  
VALIDATION: `LegacyDocSpikeTests` PASS on the accepted corpus.  
FAST_CHECK: `34571752300` — PASS.  
COMMIT: `7261629630737d7f20c21036b68b46b03d88aa7d`

### T006 — XLSX generation and independent validation
STATUS: COMPLETE  
RESULT: `ClosedXML 0.105.1` serializes normalized output and `DocumentFormat.OpenXml 3.5.1` independently reopens/readbacks the package before publication. Generated formulas are not introduced.  
VALIDATION: `XlsxWriteReadbackTests` PASS.  
FAST_CHECK: `34573733653` — PASS.  
COMMIT: `111356e991e0e9ff243ed281c73816c98fd55a19`

### T007 — Clean DOCX orchestration and safe publication
STATUS: COMPLETE  
RESULT: detector → reader → normalizer → value policy → XLSX writer → independent validator → publisher works end-to-end; validation blocks publication on failure; output collisions resolve without overwrite.  
VALIDATION: DOCX E2E and `OutputPublisherTests` PASS.  
FAST_CHECK: `34574622457` — PASS.  
COMMIT: `1f82e7af7722612454d5b21ad04cbc21e979233f`

### T008 — Legacy DOC integration
STATUS: COMPLETE  
RESULT: `.doc` is converted only inside the per-session temporary directory and then converges on the same reader/normalizer/value-policy/writer/validator/publisher path as `.docx`; final naming remains based on the original source.  
VALIDATION: `LegacyDocConversionTests` PASS; source bytes remain unchanged.  
FAST_CHECK: `34575233210` — PASS.  
COMMIT: `89eb4bbb6aed9d6aebcabe99561d632b5de0744b`

### T009 — Clean interactive WPF conversion flow
STATUS: COMPLETE  
BASE_BEFORE: `3da09dabb9b717c55216152b9f6d0e35149c5c68`  
FILES_CHANGED: `App.xaml`, `App.xaml.cs`, new `Assets/Icons.xaml`, `MainWindow.xaml`, `MainWindow.xaml.cs`.  
RESULT: bootstrap placeholder replaced by the native WPF user flow: choose/drop Word file → explicit conversion → visible result, output path and recovery/reset controls. The production `ConversionOrchestrator` is wired into application startup.  
VALIDATION: project build, full automated suite, format verification, bootstrap smoke and portable publish PASS.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34576464106` — PASS.  
DEVIATIONS: visual resources were added in `Assets/Icons.xaml` as part of the accepted WPF surface; no new runtime subsystem or service.  
COMMIT: `4f1d9f94e463f78531adc7f99ac217049b062574`

### T010 — Warning and partial-result flows
STATUS: COMPLETE  
BASE_BEFORE: `4f1d9f94e463f78531adc7f99ac217049b062574`  
FILES_CHANGED: `Assets/Icons.xaml`, `Conversion/ConversionOrchestrator.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, new `tests/WordToExcel.Tests/E2E/WarningConversionTests.cs`.  
RESULT: ambiguity/unsupported-object findings are surfaced to the user; documents without tables produce an explicit `NO_TABLES` warning while preserving ordinary text on `Контекст`; final UI state is `Готово с предупреждениями` rather than false clean success.  
VALIDATION: `WarningConversionTests` plus full suite PASS.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34577209657` — PASS.  
DEVIATIONS: NONE.  
COMMIT: `cdaefe70b22d4f2568e0273b585ee6d20337970f`

### T011 — Typed unsupported/protected/corrupt input failures
STATUS: COMPLETE  
BASE_BEFORE: `cdaefe70b22d4f2568e0273b585ee6d20337970f`  
FILES_CHANGED: `Conversion/ConversionOrchestrator.cs`, `Conversion/InputDetector.cs`, new `tests/WordToExcel.Tests/E2E/InputFailureTests.cs`, invalid-input fixtures and fixture README.  
RESULT: extension alone is not trusted; unsupported, corrupt and encrypted/protected Word inputs are classified as typed failures and never reported as success. Encrypted OOXML compound containers are identified before conversion.  
VALIDATION: `InputFailureTests` plus full suite PASS.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34578073133` — PASS.  
DEVIATIONS: one temporary GitHub Actions workflow materialized the binary encrypted DOCX fixture from the provenance-pinned Apache POI source; the workflow was removed before task completion and is not a runtime dependency.  
COMMIT: `ed5d44755df6a7767739c659b4d95a97786c9594`

### T012 — Save recovery to alternate destination
STATUS: COMPLETE  
BASE_BEFORE: `ed5d44755df6a7767739c659b4d95a97786c9594`  
FILES_CHANGED: `Conversion/ConversionOrchestrator.cs`, `Conversion/OutputPublisher.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, new `tests/WordToExcel.Tests/E2E/SaveRecoveryTests.cs`, extended `tests/WordToExcel.Tests/Unit/OutputPublisherTests.cs`.  
RESULT: output-write failure produces a visible recovery action; user may select another folder; alternate destination is canonicalized before use; collision handling remains non-overwriting.  
VALIDATION: `SaveRecoveryTests`, `OutputPublisherTests` and full suite PASS.  
FAST_CHECK: GitHub Actions `bootstrap-check` run `34578888238` — PASS.  
DEVIATIONS: NONE.  
COMMIT: `4e1ee91d6ca71b45f29fa594af9ca13034b28292`

## Post-task Hardening
After T012, two WPF foreground fixes were applied so light application surfaces remain readable even when Windows itself is using a dark theme:
- `e13c6060d241781cdcdc530d3529383d2d4aab59` — keep light-surface text readable in dark Windows theme.
- `53f1c9a3e48bb615e2ca2c8c2f9bd86131e01c0b` — pin readable foreground for the light application theme.

CI packaging was then changed from a self-contained folder artifact to a single-file Windows executable:
- PRODUCT_HEAD_BEFORE_DOC_CLOSEOUT: `dc406eb5d64f57f2d4e037ab0aeb2fbf8a7c4055`
- change: `dotnet publish ... -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:IncludeNativeLibrariesForSelfExtract=true`
- staged artifact: `WordToExcel-win-x64-single-exe` containing `WordToExcel.exe`
- GitHub Actions `bootstrap-check` run `34582652988` — PASS on `dc406eb5d64f57f2d4e037ab0aeb2fbf8a7c4055`.

## Build Deviations
- T002 added `Properties/AssemblyInfo.cs` solely for test access to internal accepted contracts; no public API was introduced.
- T005 used temporary one-shot GitHub Actions workflows solely to materialize/replace binary legacy fixtures because the connected text API could not author those binaries directly. The workflows were removed before PASS and are not runtime dependencies.
- T011 used the same temporary-fixture pattern for one encrypted DOCX corpus item; provenance is recorded with the fixture and the workflow was removed before PASS.
- No Office process, LibreOffice runtime fallback, network service, database or background service was added to the product path.

## Upstream Returns
- NONE

## Acceptance Gate
Automated implementation is complete. Phase 10 remains intentionally unmerged until an interactive Windows smoke is completed against the produced single-file executable.

Required manual smoke before merge:
1. Launch `WordToExcel.exe` on Windows without relying on the repository checkout.
2. Convert one real `.docx` containing at least one table and surrounding text.
3. Convert one real or corpus-approved `.doc`.
4. Open the generated `.xlsx` in Excel and confirm the workbook is readable and the expected table/context sheets are present.
5. Exercise at least one warning/error path if convenient (for example a document without tables or a protected/invalid input).

Acceptance result must be recorded before the PR is merged.

## Final Build State
Product implementation head before documentation closeout: `dc406eb5d64f57f2d4e037ab0aeb2fbf8a7c4055`  
Branch: `build/phase-10`  
PR: `#1 Phase 10 — Build`  
Pending implementation tasks: 0  
Pending acceptance: interactive Windows smoke  
Latest product-head fast check: PASS — run `34582652988`  
Single-file artifact: `WordToExcel-win-x64-single-exe`  
Known blocking implementation issues: NONE  
Merge gate: INTERACTIVE_SMOKE_REQUIRED

## Handoff
NEXT_PHASE: INTERACTIVE_ACCEPTANCE  
RETURN_TO_PHASE: NONE
