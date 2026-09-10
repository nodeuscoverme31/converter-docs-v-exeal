# 07 — Bootstrap / Repository & Agent Readiness

PHASE: 07_BOOTSTRAP  
STATUS: COMPLETE  
PROJECT_MODE: GREENFIELD

## Target Verified

Workspace: GitHub repository  
Repository: `nodeuscoverme31/converter-docs-v-exeal`  
HEAD: `5f0252cf05f8191d971981cc89eaff435b2cfc96` — verified bootstrap baseline immediately before this final Phase-07 artifact commit  
Branch: `main`  
Remote: `https://github.com/nodeuscoverme31/converter-docs-v-exeal`  
Baseline: repository started empty; bootstrap commits created the minimal WPF solution, tests, canonical docs, lockfiles, agent instructions and clean-checkout CI. No product conversion feature has been implemented.

Exact target was verified before persistent writes. The connected GitHub identity has write/admin permission to this private repository.

## Canonical Repository Knowledge

REPO_CANONICAL_DOCS: YES  
Docs root: `docs/project/`  
Intent: `docs/project/01_PROJECT_INTENT.md`  
Spec: `docs/project/02_MVP_SPEC.md`  
Rules: `docs/project/03_PROJECT_RULES.md`  
UX: `docs/project/04_PRODUCT_UX_DESIGN.md`  
Visual: `docs/project/05_VISUAL_UI_DESIGN.md`  
Technical plan: `docs/project/06_TECHNICAL_PLAN.md`  
Bootstrap: `docs/project/07_BOOTSTRAP.md`  
Visual artifact: `docs/design/05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.jpg`

`README.md` is the human entry point. `AGENTS.md` is the lean operational map for coding agents. Neither replaces the canonical project documents.

The repo-local visual artifact is an optimized JPEG derivative of the user-confirmed Phase-05 PNG target. It is used as the stable repository reference because the GitHub connector used during bootstrap could not directly materialize the original conversation PNG into the repository. The visual direction/content is unchanged; the original Phase-05 PNG remains the source artifact from the design phase.

## Agent Runtime Profile

PRIMARY_AGENT_RUNTIME: OpenAI coding-agent workflow operating on the GitHub repository  
OTHER_SUPPORTED_RUNTIMES: NOT CONFIGURED  
PRIMARY_INSTRUCTION_MECHANISM: `/AGENTS.md`  
PATH_SPECIFIC_INSTRUCTIONS_USED: NO  
DISCOVERY_VERIFIED: NOT_AVAILABLE

Instruction files:
- `/AGENTS.md` — repo-wide operational map.

Conflicts: NONE FOUND. Repository tree contains no nested `AGENTS.md`, `CLAUDE.md`, `.github/copilot-instructions.md` or path-specific instruction files competing with the root instructions.

`DISCOVERY_VERIFIED` is not called PASS because this chat does not expose an independent coding runtime that can prove automatic instruction loading. File existence, scope and conflict state were verified directly from repository evidence.

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

NETWORK_REQUIRED: YES for a fresh NuGet restore when packages are not already cached; NO for the planned self-contained end-user runtime.  
REQUIRED_ENV_NAMES: NONE  
LOCAL_SERVICES: NONE  
WORKING_DIRECTORY: repository root

## Actual Environment

Verified on clean GitHub Actions checkout:

OS: Microsoft Windows Server 2025, `10.0.26100`, x64  
Runtime host: `.NET 10.0.12`  
SDK: `.NET SDK 10.0.401`  
MSBuild: `18.9.11+e34a38d2a`  
Package manager: NuGet via `dotnet restore`  
Git: `2.55.0.windows.5` in the verification runner  
Target framework: `net10.0-windows`  
Runtime identifier declared by app: `win-x64`  
Required env names: NONE for project behavior  
Local services: NONE

Primary product target remains Windows 11 x64. The CI environment proves reproducible Windows build/bootstrap, not consumer Windows-11 visual rendering.

## Files Created / Changed

Bootstrap materialized:

- `README.md`
- `AGENTS.md`
- `.gitignore`
- `.gitattributes`
- `.editorconfig`
- `Directory.Build.props`
- `global.json`
- `WordToExcel.sln`
- `.github/workflows/bootstrap-check.yml`
- minimal neutral WPF app shell under `src/WordToExcel.App/`
- minimal test harness under `tests/WordToExcel.Tests/`
- `src/WordToExcel.App/packages.lock.json`
- `tests/WordToExcel.Tests/packages.lock.json`
- canonical project docs `01`–`07` under `docs/project/`
- selected visual reference under `docs/design/`

No Word→Excel product behavior, feature tasks, OCR, batch processing or other Phase-08+ work was added.

## Dependencies Materialized

Direct application dependencies, pinned/restored:

- `DocumentFormat.OpenXml` `3.5.1`
- `ClosedXML` `0.105.1`
- `DocSharp.Binary.Doc` `0.21.0`

Test dependency:

- `xunit.v3` `4.0.0`

Dependency lockfiles are versioned. `DocSharp.Binary.Doc` being materialized does **not** mean legacy `.doc` fidelity is proven; it remains gated by `TECHNICAL_SPIKE_REQUIRED: LEGACY-DOC-001` from Phase 06.

## Verified Commands

Verification evidence: clean GitHub Actions run on commit `5f0252cf05f8191d971981cc89eaff435b2cfc96`, workflow `bootstrap-check`, run `34510326538`.

SETUP: PASS

```powershell
dotnet restore WordToExcel.sln --locked-mode
```

START: PASS for non-interactive bootstrap-safe application entry path

```powershell
dotnet run --project src/WordToExcel.App/WordToExcel.App.csproj -c Release --no-build -- --bootstrap-smoke
```

FAST_CHECK: PASS as the same build/test/format sequence executed successfully in the clean checkout.

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

TYPECHECK: PASS through successful C# compilation/build.

SMOKE: PASS.

```powershell
dotnet run --project src/WordToExcel.App/WordToExcel.App.csproj -c Release --no-build -- --bootstrap-smoke
```

PORTABLE PUBLISH: PASS; `WordToExcel.App.exe` was present in the self-contained publish folder.

```powershell
dotnet publish src/WordToExcel.App/WordToExcel.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false
```

Interactive GUI rendering itself was not asserted by the headless/bootstrap-safe CI smoke and is not treated as verified product UI behavior.

## Clean-Checkout Reproduction

CLEAN_CHECKOUT_REPRODUCTION: PASS  
Method: GitHub Actions created a clean hosted Windows workspace with `actions/checkout`, installed the pinned SDK, restored versioned lockfiles in locked mode, then executed build, tests, format check, bootstrap-safe app start, self-contained publish and canonical-doc existence checks.  
Result: PASS on workflow run `34510326538` for commit `5f0252cf05f8191d971981cc89eaff435b2cfc96`.  
Hidden dependencies: no project secrets, env variables or local services. Fresh build setup requires network access for NuGet restore and GitHub Actions themselves; the planned end-user self-contained runtime does not.

This final phase-artifact commit changes documentation/guard coverage only. The executable baseline used for readiness remains the clean-checkout-verified parent.

## New-Agent Cold-Start

NEW_AGENT_COLD_START: NOT_EXECUTED  
Runtime/subagent: no independent repository-aware coding subagent is exposed inside this conversation for a genuine behavioral cold-start test.  
Fallback used: YES — deterministic repository-readiness audit, explicitly not represented as an executed behavioral test.

Questions/result from repository evidence:

1. Цель проекта находится в `README.md` и `docs/project/01_PROJECT_INTENT.md` — PASS.
2. MVP scope находится в `docs/project/02_MVP_SPEC.md` — PASS.
3. Canonical requirements path discoverable from `AGENTS.md` — PASS.
4. Project rules path discoverable from `AGENTS.md` — PASS.
5. UX, visual and technical-plan paths discoverable from `AGENTS.md` — PASS.
6. Setup/start/fast-check commands discoverable from `README.md` and `AGENTS.md` — PASS.
7. Write-target/scope guard present in `AGENTS.md` — PASS.
8. Return-to-phase ownership map present in `AGENTS.md` — PASS.
9. Safe mechanical bootstrap check is documented and independently exercised by clean-checkout CI — PASS.

Repository evidence used: `README.md`, `AGENTS.md`, `docs/project/01_PROJECT_INTENT.md` through `06_TECHNICAL_PLAN.md`, repository tree, lockfiles and workflow result.

## Agent Context Readiness

AGENT_CONTEXT_READY: PASS  
Dead links: NONE FOUND in the bootstrap entry map after materialization.  
Competing sources: NONE FOUND; project canon is `docs/project/`, while README/AGENTS are entry maps.  
Missing context: no blocking project context required for Phase 08 depends on the old chat.

A new agent can discover project goal, MVP boundaries, permanent rules, UX, visual target, technical plan, actual scaffold and verified commands from the repository alone.

## Differences From Technical Plan

- Phase 06 named xUnit v3 as the bootstrap test line; actual restored package is `xunit.v3 4.0.0`. This is a version materialization detail, not a test architecture change.
- `RuntimeIdentifiers=win-x64` was added explicitly to the WPF project after the first locked-mode clean-checkout exposed that the committed RID-aware lockfile and project metadata were inconsistent. This aligns the project with the already selected Phase-06 `win-x64` runtime target.
- Canonical docs live under `docs/project/` instead of repository root. Phase-07 skill explicitly permits this greenfield canonical location and it keeps root entry points lean.
- Repo-local selected visual reference is stored as optimized `.jpg` rather than the original generated `.png`; the selected Option-1 visual direction is unchanged.
- No installer/single-file publish was added; self-contained folder publish remains the selected bootstrap behavior.

No major runtime family, architecture, persistence, security boundary or integration changed relative to Phase 06.

## Not Verified / Blockers

Non-blocking for Phase 08:

- `TECHNICAL_SPIKE_REQUIRED: LEGACY-DOC-001` is NOT EXECUTED. It blocks claiming reliable legacy `.doc` feature support, but does not block task breakdown; Phase 08 must preserve this gate before `.doc` implementation is treated as ready.
- Windows 10 compatibility smoke was not executed. Windows 10 support is not promised by the current technical plan.
- Interactive WPF visual convergence against the selected target is not verified in Phase 07. Phase 07 only materializes the neutral UI foundation; visual implementation/testing belongs to later lifecycle phases.
- Automatic `AGENTS.md` discovery by an independent coding runtime was not behaviorally testable here; the repository instruction surface and absence of conflicts were verified statically.

Blocking issues for Phase 08: NONE.

## Bootstrap Corrections During Phase 07

Two self-created bootstrap issues were detected mechanically and corrected before completion:

1. Initial xUnit smoke source omitted `using Xunit;`; CI build failed. The missing import was added and the next run passed build/test.
2. The first locked-mode reproduction found a runtime-identifier mismatch between the committed app lockfile and project metadata. Root cause: the lockfile contained the Phase-06 `win-x64` publish graph while the project did not declare its selected RID. `RuntimeIdentifiers=win-x64` was added; the next clean-checkout locked restore passed.

Neither issue was hidden or treated as success before verification.

## Final Baseline

Worktree status: remote `main`; no separate local user worktree was modified.  
Relevant diff: greenfield bootstrap only — neutral scaffold, dependencies/lockfiles, canonical docs, agent instructions, CI/readiness guard and visual reference.  
Verified executable baseline HEAD: `5f0252cf05f8191d971981cc89eaff435b2cfc96`.  
Readiness: PASS for Phase 08 task breakdown.  
Product implementation status: NOT STARTED.

## Handoff

NEXT_PHASE: 08_TASK_BREAKDOWN  
RETURN_TO_PHASE: NONE

Phase 08 must use real repository state and the verified commands recorded here, not the planned commands from Phase 06 or old chat history.

PHASE_07_COMPLETE
