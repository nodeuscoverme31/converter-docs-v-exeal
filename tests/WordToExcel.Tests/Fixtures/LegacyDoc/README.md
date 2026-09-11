# Legacy DOC spike corpus

This corpus exists only for `LEGACY-DOC-001` (T005).

The `.doc` fixtures were authored from explicit test documents and exported to Word 97-2003 binary format. Each fixture has one focused truth target:

- `simple-table.doc` — one 2x2 table with A/B/C/D.
- `multiple-tables.doc` — two distinct tables plus surrounding markers.
- `merged-cells.doc` — horizontal merged cell containing `MERGED`.
- `irregular-rows.doc` — rows with differing horizontal spans containing `WIDE`, `R1C*`, `ALL`.
- `unicode-cyrillic.doc` — `Привет`, `ёж — Москва`.
- `leading-zero-codes.doc` — exact `001234`.
- `long-numeric-identifiers.doc` — exact `12345678901234567890`.
- `surrounding-text.doc` — `Текст до таблицы`, table value `ЦЕНТР`, `Текст после таблицы`.
- `nested-table.doc` — parent value `Родитель` and nested table value `Вложенные данные`.
- `embedded-image.doc` — table text plus an embedded image; conversion must leave an unsupported-object finding after T002 parsing.
- `protected.doc` — Word binary FIB protection/encryption flag set; spike must reject it rather than claim readable content.
- `damaged.doc` — deliberately truncated compound file; spike must reject it.

Fixture creation tooling is not a runtime dependency. The tested conversion path is DocSharp only; no Word, LibreOffice, native helper process or network call is allowed in the spike execution.
