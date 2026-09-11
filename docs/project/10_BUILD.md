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
| Task | Status | Validation | Fast Check | Commit |
|---|---|---|---|---|
| T001 | COMPLETE | build PASS | PASS — run 34523442594 | `175e5b04cb3d9c91a64905ad3e98e335cffa8a49` |
| T002 | COMPLETE | integration tests PASS | PASS — run 34525190440 | `a4061c00decdf45f544f0cba0fe32114c46c3d52` |
| T003 | COMPLETE | unit tests PASS | PASS — run 34564595328 | `95467b68c771b840dc7381fd3f3ab3cb99b60e77` |
| T004 | COMPLETE | unit tests PASS | PASS — run 34564873097 | `9959e70c6dde808efaf096dac75f69f8af0baf69` |
| T005 | COMPLETE | 12-category legacy spike PASS | PASS — run 34571752300 | `7261629630737d7f20c21036b68b46b03d88aa7d` |
| T006 | COMPLETE | integration readback tests PASS | PASS — run 34573733653 | `111356e991e0e9ff243ed281c73816c98fd55a19` |
| T007 | COMPLETE | E2E + publisher tests PASS | PASS — run 34574622457 | `1f82e7af7722612454d5b21ad04cbc21e979233f` |
| T008 | COMPLETE | legacy E2E PASS | PASS — run 34575233210 | `89eb4bbb6aed9d6aebcabe99561d632b5de0744b` |
| T009 | COMPLETE | clean WPF flow PASS | PASS — run 34576464106 | `4f1d9f94e463f78531adc7f99ac217049b062574` |
| T010 | COMPLETE | warning/partial-result flow PASS | PASS — run 34577209657 | `cdaefe70b22d4f2568e0273b585ee6d20337970f` |
| T011 | COMPLETE | input-failure classification PASS | PASS — run 34578073133 | `ed5d44755df6a7767739c659b4d95a97786c9594` |
| T012 | COMPLETE | alternate save recovery PASS | PASS — run 34578888238 | `4e1ee91d6ca71b45f29fa594af9ca13034b28292` |

## Task Records
### T001–T008
Canonical implementation evidence is preserved in the earlier task records and associated commits/runs listed above.

### T009
STATUS: COMPLETE  
IMPLEMENTATION: clean interactive WPF choose/drop → explicit conversion → result flow.  
VALIDATION: `bootstrap-check` run `34576464106` — PASS.  
COMMIT: `4f1d9f94e463f78531adc7f99ac217049b062574`

### T010
STATUS: COMPLETE  
IMPLEMENTATION: warning and partial-result presentation, including no-table and unsupported-object findings.  
VALIDATION: `bootstrap-check` run `34577209657` — PASS.  
COMMIT: `cdaefe70b22d4f2568e0273b585ee6d20337970f`

### T011
STATUS: COMPLETE  
IMPLEMENTATION: typed unsupported/corrupt/protected input failures and protected-DOCX fixture coverage.  
VALIDATION: `bootstrap-check` run `34578073133` — PASS.  
COMMIT: `ed5d44755df6a7767739c659b4d95a97786c9594`

### T012
STATUS: COMPLETE  
IMPLEMENTATION: alternate output-folder recovery with canonical destination handling and no silent overwrite.  
VALIDATION: `bootstrap-check` run `34578888238` — PASS.  
COMMIT: `4e1ee91d6ca71b45f29fa594af9ca13034b28292`

## Interactive Smoke Remediation
A real stress-DOCX acceptance pass preserved all seven table sheets, all 29 context blocks, leading-zero identifiers, long IDs, formula-like text, line breaks, Unicode, empty cells, merge normalization and expected unsupported-image warning behavior. The pass exposed two usability gaps rather than data-loss defects: strict dot-decimal values remained text, and generated sheets required manual formatting for comfortable Excel use.

Remediation on `build/phase-10`:
- strict safe dot-decimals with `.` and at most 15 significant digits are emitted as numeric Excel values while preserving source decimal precision through the number format;
- ambiguous comma decimals, leading-zero decimals, long numeric sequences and formula-like prefixes remain text;
- generated table/context sheets use wrapped text, bold first rows, frozen first rows and bounded auto-fit column widths;
- new unit/integration regression coverage verifies decimal policy, numeric serialization and workbook usability.

Fresh verification: GitHub Actions `bootstrap-check` run `34592875962` — PASS, including restore/audit, build, all 60 tests, format check, bootstrap smoke, single-file Windows publish, canonical docs check and rolling `test-latest` release publication.

## Build Deviations
- T002 added `Properties/AssemblyInfo.cs` solely for test access to internal accepted contracts; no public API introduced.
- T005 used temporary one-shot GitHub Actions workflows solely to author/replace binary test fixtures because the connector cannot write arbitrary binary content. Those workflows were removed before PASS; they are not product/runtime dependencies.
- Post-T012 interactive smoke added narrowly scoped Excel usability remediation without changing the accepted Word→model→normalize→validate→publish architecture.

## Upstream Returns
- NONE

## Observations For Phase 11
- T005's locally-authored image candidate was invalid because it was a linked `INCLUDEPICTURE`; the final corpus uses a provenance-backed embedded-image fixture instead.

## Final Build State
HEAD: current `build/phase-10` head  
Worktree: isolated remote branch  
Pending implementation tasks: 0  
Automated fast check: PASS through T012 plus Excel usability remediation  
Remaining gate: interactive Windows acceptance of the refreshed `test-latest` EXE on the real stress document  
Known non-blocking issues: existing nullable-analysis warnings remain outside this remediation scope.

## Handoff
NEXT_PHASE: interactive acceptance, then PR readiness/merge decision  
RETURN_TO_PHASE: NONE
