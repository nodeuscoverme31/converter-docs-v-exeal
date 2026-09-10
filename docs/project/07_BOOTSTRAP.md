# 07 — Bootstrap / Repository & Agent Readiness

PHASE: 07_BOOTSTRAP  
STATUS: COMPLETE  
PROJECT_MODE: GREENFIELD  
REMEDIATION_TRIGGER: `09_PLAN_CHECK.md` findings `F09-001`, `F09-002`

## Target Verified

Workspace: GitHub repository  
Repository: `nodeuscoverme31/converter-docs-v-exeal`  
PRE_WRITE_HEAD: `176edd6657fe8b7549663942edcee53c90d4e7b9`  
VERIFIED_REMEDIATION_HEAD: `ff30165d26eb9e1dca08b9f349c2a3ae13773968`  
Branch: `main`  
Remote: `https://github.com/nodeuscoverme31/converter-docs-v-exeal`  
Worktree model: remote GitHub state; no local user worktree modified.  
Expected remediation scope: bootstrap/agent-readiness + dependency audit/license evidence only; no Phase-08 task edits and no product implementation.

Exact live `main` was checked before remediation writes. The pre-write repository already contained canonical artifacts `01`–`09`; product behavior was still unimplemented.

## Canonical Repository Knowledge

REPO_CANONICAL_DOCS: YES  
Docs root: `docs/project/`

- Intent: `docs/project/01_PROJECT_INTENT.md`
- Spec: `docs/project/02_MVP_SPEC.md`
- Rules: `docs/project/03_PROJECT_RULES.md`
- UX: `docs/project/04_PRODUCT_UX_DESIGN.md`
- Visual: `docs/project/05_VISUAL_UI_DESIGN.md`
- Technical plan: `docs/project/06_TECHNICAL_PLAN.md`
- Bootstrap: `docs/project/07_BOOTSTRAP.md`
- Task decomposition: `docs/project/08_TASK_BREAKDOWN.md`
- Plan-check gate: `docs/project/09_PLAN_CHECK.md`
- Visual artifact: `docs/design/05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.jpg`
- Third-party runtime notice inventory: `THIRD_PARTY_NOTICES.md`

`README.md` remains the human entry point. `AGENTS.md` is the lean coding-agent map. Neither replaces canonical project documents.

## Agent Runtime Profile

PRIMARY_AGENT_RUNTIME: OpenAI coding-agent workflow operating on the GitHub repository  
OTHER_SUPPORTED_RUNTIMES: NOT CONFIGURED  
PRIMARY_INSTRUCTION_MECHANISM: `/AGENTS.md`  
PATH_SPECIFIC_INSTRUCTIONS_USED: NO  
DISCOVERY_VERIFIED: NOT_AVAILABLE

Instruction files:
- `/AGENTS.md` — repo-wide operational map.

Conflicts: NONE FOUND in the live repository instruction surface.

The previous transient sentence `next owning phase is Phase 08` was removed from `AGENTS.md`. The file now uses stable lifecycle pointers instead:
- `08_TASK_BREAKDOWN.md` is the task-decomposition source;
- `09_PLAN_CHECK.md` is the pre-Build gate;
- product implementation may start only when the latest Plan Check says `READY_FOR_BUILD`;
- otherwise the agent follows the recorded `RETURN_TO_PHASE`.

This closes the bootstrap root cause of Phase-09 `F09-001` without putting current PR/branch/task transient state into permanent agent instructions.

## Agent Execution Environment

AGENT_SETUP_COMMAND:

```powershell
dotnet restore WordToExcel.sln --locked-mode
```

AGENT_START_COMMAND / bootstrap-safe start:

```powershell
dotnet run --project src/WordToExcel.App/WordToExcel.App.csproj -c Release --no-build -- --bootstrap-smoke
```

AGENT_FAST_CHECK_COMMAND after setup:

```powershell
dotnet build WordToExcel.sln -c Release --no-restore; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet format WordToExcel.sln --verify-no-changes --no-restore
```

DEPENDENCY_AUDIT_COMMANDS:

```powershell
dotnet restore WordToExcel.sln --locked-mode -p:AuditPipeline=true
dotnet package list --project WordToExcel.sln --vulnerable --include-transitive --no-restore
```

NETWORK_REQUIRED: YES for fresh NuGet restore/audit; NO for the planned self-contained end-user runtime.  
REQUIRED_ENV_NAMES: NONE for product behavior. `AuditPipeline` is an explicit CI/MSBuild property, not a product secret/config requirement.  
LOCAL_SERVICES: NONE  
WORKING_DIRECTORY: repository root

## Actual Environment

Fresh clean-checkout verification on GitHub Actions run `34516104779`:

- OS: Microsoft Windows Server 2025, `10.0.26100`, x64
- .NET SDK: `10.0.401`
- .NET host/runtime: `10.0.12`
- MSBuild: `18.9.11+e34a38d2a`
- Git: `2.55.0.windows.5`
- Target framework: `net10.0-windows`
- Runtime identifier: `win-x64`
- Package manager: NuGet through .NET CLI
- Local services: NONE

Primary product target remains Windows 11 x64; the hosted runner proves reproducible Windows build/bootstrap, not consumer UI rendering.

## Files Created / Changed In Remediation

Relative to pre-write HEAD `176edd6657fe8b7549663942edcee53c90d4e7b9`, verified remediation commit `ff30165d26eb9e1dca08b9f349c2a3ae13773968` changes only:

- `.github/workflows/bootstrap-check.yml`
- `AGENTS.md`
- `Directory.Build.props`
- `THIRD_PARTY_NOTICES.md` (new)

This canonical `07_BOOTSTRAP.md` is then updated as the required Phase-07 artifact.

No product source/test feature behavior, task card, dependency version, lockfile, architecture, UX or visual target was changed.

## Dependencies Materialized

Application direct dependencies remain pinned/restored:

- `DocumentFormat.OpenXml` `3.5.1`
- `ClosedXML` `0.105.1`
- `DocSharp.Binary.Doc` `0.21.0`

Locked runtime graph also contains:

- `ClosedXML.Parser` `2.0.0`
- `DocSharp.Binary.Common` `0.21.0`
- `DocumentFormat.OpenXml.Framework` `3.5.1`
- `ExcelNumberFormat` `1.1.0`
- `RBush.Signed` `4.0.0`
- `SixLabors.Fonts` `1.0.0`

Test-only dependency: `xunit.v3` `4.0.0`.

Lockfiles remain versioned and unchanged by remediation.

`DocSharp.Binary.Doc` being present does not prove `.doc` fidelity; `LEGACY-DOC-001` remains mandatory before production legacy support.

## Supply-Chain / License Remediation

`F09-002` required a fresh known-advisory check and notice inventory.

### NuGet audit configuration

Repository-level MSBuild configuration now explicitly sets:

- `NuGetAudit=true`
- `NuGetAuditMode=all`
- `NuGetAuditLevel=low`
- `NU1900`–`NU1905` as errors when `AuditPipeline=true`

The CI restore invokes `-p:AuditPipeline=true`, so audit communication failures and known vulnerability warnings fail that audit restore rather than being silently ignored.

### Fresh advisory result

GitHub Actions run `34516104779`, commit `ff30165d26eb9e1dca08b9f349c2a3ae13773968`:

```text
The given project `WordToExcel.App` has no vulnerable packages given the current sources.
The given project `WordToExcel.Tests` has no vulnerable packages given the current sources.
```

Audit/list source included `https://api.nuget.org/v3/index.json`.

This is a point-in-time result, not a permanent security guarantee. CI now repeats the audit path on future runs.

### Notice inventory

`THIRD_PARTY_NOTICES.md` records the locked application runtime graph and checked license metadata:

- MIT: ClosedXML, DocSharp.Binary.Doc, DocumentFormat.OpenXml, ClosedXML.Parser, DocSharp.Binary.Common, DocumentFormat.OpenXml.Framework, ExcelNumberFormat, RBush.Signed
- Apache-2.0: SixLabors.Fonts

No third-party icon package or bundled font is currently present in the scaffold.

Release Prep must re-check the actual final publish graph and preserve exact upstream license/NOTICE texts required by packages actually shipped; the bootstrap inventory is not a substitute for a final release-license audit.

## Verified Commands

Verification evidence: clean GitHub Actions workflow `bootstrap-check`, run `34516104779`, on commit `ff30165d26eb9e1dca08b9f349c2a3ae13773968`, conclusion `success`.

SETUP: PASS

```powershell
dotnet restore WordToExcel.sln --locked-mode -p:AuditPipeline=true
```

DEPENDENCY AUDIT: PASS — both application and test projects reported no vulnerable packages from current configured sources.

```powershell
dotnet package list --project WordToExcel.sln --vulnerable --include-transitive --no-restore
```

BUILD: PASS — 0 warnings, 0 errors.

```powershell
dotnet build WordToExcel.sln -c Release --no-restore
```

TEST: PASS — 1 bootstrap test succeeded, 0 failed.

```powershell
dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build
```

LINT / FORMAT: PASS.

```powershell
dotnet format WordToExcel.sln --verify-no-changes --no-restore
```

TYPECHECK: PASS through successful C# build.

START / SMOKE: PASS for the non-interactive bootstrap-safe entry path.

```powershell
dotnet run --project src/WordToExcel.App/WordToExcel.App.csproj -c Release --no-build -- --bootstrap-smoke
```

PORTABLE PUBLISH: PASS; the workflow found `WordToExcel.App.exe` in the self-contained `win-x64` publish folder.

```powershell
dotnet publish src/WordToExcel.App/WordToExcel.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false
```

CANONICAL LIFECYCLE DOC/NOTICE CHECK: PASS for `AGENTS.md`, `THIRD_PARTY_NOTICES.md`, artifacts `01`–`09`, and selected visual target.

## Clean-Checkout Reproduction

CLEAN_CHECKOUT_REPRODUCTION: PASS  
Method: GitHub Actions checked out commit `ff30165d26eb9e1dca08b9f349c2a3ae13773968` into a fresh hosted Windows workspace, installed the pinned SDK, restored locked dependencies with NuGet audit enforcement, listed vulnerable direct/transitive packages, built, tested, format-checked, ran bootstrap smoke, published the self-contained app and verified canonical lifecycle docs/notices.  
Result: PASS — workflow run `34516104779`.  
Hidden dependencies: no project secrets, hidden local files or local services. Fresh build/audit requires network access to NuGet; the user runtime remains planned as offline/self-contained.

## New-Agent Cold-Start

NEW_AGENT_COLD_START: NOT_EXECUTED  
Runtime/subagent: no independent repository-aware coding subagent is exposed inside this conversation for a genuine behavioral cold-start execution.  
Fallback used: YES — deterministic repository-readiness audit only; it is not represented as a behavioral test.

Repository-readiness questions after remediation:

1. Goal discoverable from `AGENTS.md` → PASS.
2. MVP scope pointer → `docs/project/02_MVP_SPEC.md` → PASS.
3. Canonical requirements path discoverable → PASS.
4. Permanent project rules path discoverable → PASS.
5. UX/visual/technical paths discoverable → PASS.
6. Setup/start/fast-check commands discoverable → PASS.
7. Write-target/scope guard present → PASS.
8. Architecture/requirement/UX/task-plan return routes present → PASS.
9. Task decomposition path `08_TASK_BREAKDOWN.md` discoverable → PASS.
10. Pre-Build Plan Check gate `09_PLAN_CHECK.md` discoverable and prevents Build while status is `NOT_READY` → PASS.

Repository evidence used: live `AGENTS.md`, canonical docs `01`–`09`, selected visual artifact, workflow, manifests and lockfiles.

## Agent Context Readiness

AGENT_CONTEXT_READY: PASS  
Dead links in root operational map: NONE FOUND.  
Competing sources: NONE FOUND; canonical artifacts own their domains and `AGENTS.md` is a map only.  
Transient lifecycle sentence in root instructions: REMOVED.  
Missing context requiring old chat: NONE for understanding the project, current task source or Plan Check gate.

A fresh agent can now discover that the current Plan Check is `NOT_READY` and therefore must not start Build; it can also find the owning return path without old conversation context.

## Differences From Technical Plan

- Added explicit repository-level NuGet Audit configuration and CI enforcement to materialize the already required Phase-06 supply-chain check. No dependency version or architecture changed.
- Added `THIRD_PARTY_NOTICES.md` because Phase 06 explicitly required license/notice handling for materialized dependencies/assets; this is remediation of a real finding, not speculative documentation.
- Root `AGENTS.md` now points to lifecycle artifacts 08/09 using stable ownership/gate wording instead of transient `next phase` text.
- Bootstrap workflow now verifies artifacts `08`/`09` and third-party notice presence because those artifacts now exist in the repository and are required for a self-contained Build handoff.

No major runtime family, persistence model, integration, security boundary, UI stack or product behavior changed.

## Not Verified / Blockers

Non-blocking for return to Phase 08:

- `LEGACY-DOC-001` is NOT EXECUTED; it remains a hard gate before reliable `.doc` implementation/support claim.
- Windows 10 compatibility is not promised and was not smoke-tested.
- Interactive WPF visual convergence is not a Phase-07 proof.
- Automatic runtime discovery/loading of `AGENTS.md` was not behaviorally tested; `DISCOVERY_VERIFIED: NOT_AVAILABLE` remains honest.
- Current `09_PLAN_CHECK.md` remains `NOT_READY` because Phase-08 finding `F09-003` still belongs to Task Breakdown. Phase 07 does not remediate or close that downstream finding.

Blocking issues owned by Phase 07 after this remediation: NONE.

## Bootstrap Corrections / Remediation History

Original bootstrap corrections retained conceptually:
1. initial xUnit smoke missing `using Xunit;` was corrected before original completion;
2. RID/lockfile mismatch was corrected by declaring `win-x64`.

Phase-09 remediation:
3. stale lifecycle status in `AGENTS.md` replaced by stable 08/09 pointers and Build gate;
4. current NuGet audit made explicit/enforced in CI;
5. application runtime license inventory added;
6. bootstrap workflow now verifies current canonical lifecycle handoff artifacts/notices.

No remediation altered product code.

## Final Baseline

Worktree status: remote `main`; no local user worktree modified.  
Verified remediation baseline: `ff30165d26eb9e1dca08b9f349c2a3ae13773968`.  
Relevant diff: bootstrap/instructions/audit/notice only.  
Readiness: PASS for returning to Phase 08 remediation.  
Product implementation status: NOT STARTED.

## Handoff

NEXT_PHASE: 08_TASK_BREAKDOWN  
RETURN_TO_PHASE: NONE

Phase 08 must use the current live repository state, preserve `LEGACY-DOC-001`, and remediate its own Plan-Check findings. Phase 07 must not edit the task plan itself.

PHASE_07_COMPLETE
