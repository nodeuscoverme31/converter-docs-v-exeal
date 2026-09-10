# Agent instructions

## Project

`converter-docs-v-exeal` is a Windows desktop utility project for converting one Word document (`.doc` / `.docx`) into a working `.xlsx` while preventing silent data loss.

## Source of truth

Canonical project documents live under `docs/project/`. Read the owning document for the question instead of treating chat history as authority.

- intent: `docs/project/01_PROJECT_INTENT.md`
- MVP requirements: `docs/project/02_MVP_SPEC.md`
- permanent rules: `docs/project/03_PROJECT_RULES.md`
- UX: `docs/project/04_PRODUCT_UX_DESIGN.md`
- visual design: `docs/project/05_VISUAL_UI_DESIGN.md`
- technical plan: `docs/project/06_TECHNICAL_PLAN.md`
- bootstrap state: `docs/project/07_BOOTSTRAP.md`
- task decomposition: `docs/project/08_TASK_BREAKDOWN.md`
- latest plan-check gate: `docs/project/09_PLAN_CHECK.md`
- selected visual reference: `docs/design/05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.jpg`

Do not infer lifecycle state from an old chat or stale status sentence. Before product implementation, read the latest `09_PLAN_CHECK.md`. Build may start only when it says `PLAN_CHECK_STATUS: READY_FOR_BUILD`; otherwise follow its `RETURN_TO_PHASE` route. During Build, execute work from `08_TASK_BREAKDOWN.md` unless a newer canonical task artifact explicitly replaces it.

## Critical invariants

- Never claim `ready`, `works`, `PASS`, or `no data loss` without fresh verification evidence.
- Never silently change or discard supported source data.
- Never overwrite the source Word file.
- Do not introduce a mandatory Microsoft Office or internet runtime dependency.
- Do not expand MVP scope without returning to the owning phase.
- Before a persistent write, verify repository, branch/path target, and current task scope.

## Verified bootstrap commands

Setup:

```powershell
dotnet restore WordToExcel.sln --locked-mode
```

Build:

```powershell
dotnet build WordToExcel.sln -c Release --no-restore
```

Test:

```powershell
dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build
```

Format check:

```powershell
dotnet format WordToExcel.sln --verify-no-changes --no-restore
```

Bootstrap smoke:

```powershell
dotnet run --project src/WordToExcel.App/WordToExcel.App.csproj -c Release --no-build -- --bootstrap-smoke
```

Portable publish:

```powershell
dotnet publish src/WordToExcel.App/WordToExcel.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false
```

Fast check after setup:

```powershell
dotnet build WordToExcel.sln -c Release --no-restore; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet format WordToExcel.sln --verify-no-changes --no-restore
```

Dependency advisory check used by CI:

```powershell
dotnet restore WordToExcel.sln --locked-mode -p:AuditPipeline=true
dotnet package list --project WordToExcel.sln --vulnerable --include-transitive --no-restore
```

## Agent runtime profile

Primary portable instruction mechanism: this root `AGENTS.md`.

No nested/path-specific agent instruction files are required at bootstrap. Do not create duplicates for other AI tools unless the active runtime actually requires different behavior.

## Return rules

- requirement problem → Phase 02
- governance problem → Phase 03
- UX problem → Phase 04
- visual problem → Phase 05
- architecture/dependency decision → Phase 06
- repository/bootstrap reproducibility or agent-readiness problem → Phase 07
- task decomposition/order/coverage problem → Phase 08
- plan-check methodology/result problem → Phase 09
