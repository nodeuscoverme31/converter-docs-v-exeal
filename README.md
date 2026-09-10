# converter-docs-v-exeal

Portable Windows utility project for converting one Microsoft Word document (`.doc` / `.docx`) into a working Excel workbook (`.xlsx`) while preventing silent loss or alteration of supported data.

## Bootstrap status

Phase 07 repository bootstrap is materialized. Product feature implementation has not started.

## Toolchain

- .NET SDK: `10.0.401`
- target: `net10.0-windows`
- C# 14 / WPF
- primary runtime: Windows 11 x64

## Setup

```powershell
dotnet restore WordToExcel.sln --locked-mode
```

## Start

Interactive desktop start:

```powershell
dotnet run --project src/WordToExcel.App/WordToExcel.App.csproj -c Release
```

Non-interactive bootstrap smoke:

```powershell
dotnet run --project src/WordToExcel.App/WordToExcel.App.csproj -c Release --no-build -- --bootstrap-smoke
```

## Fast check

After setup:

```powershell
dotnet build WordToExcel.sln -c Release --no-restore; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet test tests/WordToExcel.Tests/WordToExcel.Tests.csproj -c Release --no-build; if ($LASTEXITCODE) { exit $LASTEXITCODE }; dotnet format WordToExcel.sln --verify-no-changes --no-restore
```

The same baseline is exercised by `.github/workflows/bootstrap-check.yml` on a clean GitHub Actions checkout.

## Portable publish

```powershell
dotnet publish src/WordToExcel.App/WordToExcel.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false
```

This creates a self-contained folder; Microsoft Office and a separately installed .NET runtime are not required by the planned end-user runtime.

## Project documentation

Canonical project knowledge lives in `docs/project/`:

- `01_PROJECT_INTENT.md` — intent
- `02_MVP_SPEC.md` — MVP requirements
- `03_PROJECT_RULES.md` — permanent rules
- `04_PRODUCT_UX_DESIGN.md` — UX
- `05_VISUAL_UI_DESIGN.md` — visual design
- `06_TECHNICAL_PLAN.md` — technical HOW and Bootstrap Contract
- `07_BOOTSTRAP.md` — verified repository/environment state

Selected visual reference: `docs/design/05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.jpg`.

Agent operational instructions: [`AGENTS.md`](AGENTS.md).

The unresolved legacy `.doc` fidelity check is `TECHNICAL_SPIKE_REQUIRED: LEGACY-DOC-001`; it is not evidence that `.doc` product behavior is already implemented.
