# Agent instructions

## Project

`converter-docs-v-exeal` is a Windows desktop utility project for converting one Word document (`.doc` / `.docx`) into a working `.xlsx` while preventing silent data loss.

## Current lifecycle state

Phase 07 — Bootstrap. Product `REQ-*` behavior must not be implemented during this phase.

## Source of truth

Canonical project documents live under `docs/project/` once Phase 07 materialization is complete. Read the owning document for the question instead of treating chat history as authority.

- intent: `docs/project/01_PROJECT_INTENT.md`
- MVP requirements: `docs/project/02_MVP_SPEC.md`
- permanent rules: `docs/project/03_PROJECT_RULES.md`
- UX: `docs/project/04_PRODUCT_UX_DESIGN.md`
- visual design: `docs/project/05_VISUAL_UI_DESIGN.md`
- technical plan: `docs/project/06_TECHNICAL_PLAN.md`
- bootstrap state: `docs/project/07_BOOTSTRAP.md`

## Critical invariants

- Never claim `ready`, `works`, `PASS`, or `no data loss` without fresh verification evidence.
- Never silently change or discard supported source data.
- Never overwrite the source Word file.
- Do not introduce a mandatory Microsoft Office or internet runtime dependency.
- Do not expand MVP scope without returning to the owning phase.
- Before a persistent write, verify repository/branch/path and current task scope.

## Commands

Commands are not considered verified until `docs/project/07_BOOTSTRAP.md` records them as `PASS`.

## Return rules

- requirement problem → Phase 02
- governance problem → Phase 03
- UX problem → Phase 04
- visual problem → Phase 05
- architecture/dependency/bootstrap-plan problem → Phase 06
