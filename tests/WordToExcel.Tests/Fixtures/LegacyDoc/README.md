# Legacy DOC spike corpus

This corpus exists only for `LEGACY-DOC-001` (T005).

The project-authored `.doc` fixtures were exported to Word 97-2003 binary format from explicit source documents. Each fixture has one focused truth target:

- `simple-table.doc` — one 2x2 table with A/B/C/D.
- `multiple-tables.doc` — two distinct tables plus surrounding markers.
- `merged-cells.doc` — horizontal merged cell containing `MERGED`.
- `irregular-rows.doc` — rows with differing horizontal spans containing `WIDE`, `R1C*`, `ALL`.
- `unicode-cyrillic.doc` — `Привет`, `ёж — Москва`.
- `leading-zero-codes.doc` — exact `001234`.
- `long-numeric-identifiers.doc` — exact `12345678901234567890`.
- `surrounding-text.doc` — `Текст до таблицы`, table value `ЦЕНТР`, `Текст после таблицы`.
- `nested-table.doc` — parent value `Родитель` and nested table value `Вложенные данные`.
- `protected.doc` — Word binary FIB protection/encryption flag set; spike must reject it rather than claim readable content.
- `damaged.doc` — deliberately truncated compound file; spike must reject it.

`embedded-image.doc` is the Apache POI test fixture `test-data/document/PngPicture.doc` from commit `671a20eb38ec9ed1a84cce80483d790000514ba9`, renamed locally. Apache POI's own `TestPictures.testPictureDetectionWithPNG` asserts that this document contains exactly one picture. It is used under the Apache License 2.0 as test-only provenance for the embedded-image category.

The first locally-authored image candidate was rejected because inspection showed an `INCLUDEPICTURE` link to the authoring-time `pixel.png` path rather than an embedded binary image; it was therefore not valid evidence for this spike.

Fixture authoring tooling is not a runtime dependency. The tested conversion path is DocSharp only; no Word, LibreOffice, native helper process or network call is allowed in the spike execution.
