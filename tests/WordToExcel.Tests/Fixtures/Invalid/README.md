# Invalid input fixtures

- `not-word.docx` — plain text carrying only a `.docx` extension. Expected: unsupported format; extension alone must never be accepted.
- `corrupt.docx` — starts with `PK` but is not a readable ZIP/OOXML package. Expected: corrupt document.
- `protected.docx` — encrypted OOXML fixture copied byte-for-byte from Apache POI `test-data/poifs/protected_agile.docx` at commit `671a20eb38ec9ed1a84cce80483d790000514ba9`; upstream blob SHA `a7de3ebe43b9f3da765370d6a6a1d353c970e5a5`, size 19,456 bytes. Expected: protected document, no password prompt or decryption attempt.

These fixtures are test data only. The product remains offline and does not fetch external resources at runtime.
