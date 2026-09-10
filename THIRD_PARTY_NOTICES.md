# Third-Party Notices

This file records third-party packages present in the locked **application runtime dependency graph** for `converter-docs-v-exeal`.

Source of package/version truth: `src/WordToExcel.App/packages.lock.json`.
License metadata was checked against the corresponding NuGet Gallery package pages on 2026-09-10.

## Runtime packages

| Package | Version | Relationship | License | Upstream / license metadata |
|---|---:|---|---|---|
| ClosedXML | 0.105.1 | direct | MIT | https://www.nuget.org/packages/ClosedXML/0.105.1 |
| DocSharp.Binary.Doc | 0.21.0 | direct | MIT | https://www.nuget.org/packages/DocSharp.Binary.Doc/0.21.0 |
| DocumentFormat.OpenXml | 3.5.1 | direct | MIT | https://www.nuget.org/packages/DocumentFormat.OpenXml/3.5.1 |
| ClosedXML.Parser | 2.0.0 | transitive | MIT | https://www.nuget.org/packages/ClosedXML.Parser/2.0.0 |
| DocSharp.Binary.Common | 0.21.0 | transitive | MIT | https://www.nuget.org/packages/DocSharp.Binary.Common/0.21.0 |
| DocumentFormat.OpenXml.Framework | 3.5.1 | transitive | MIT | https://www.nuget.org/packages/DocumentFormat.OpenXml.Framework/3.5.1 |
| ExcelNumberFormat | 1.1.0 | transitive | MIT | https://www.nuget.org/packages/ExcelNumberFormat/1.1.0 |
| RBush.Signed | 4.0.0 | transitive | MIT | https://www.nuget.org/packages/RBush.Signed/4.0.0 |
| SixLabors.Fonts | 1.0.0 | transitive | Apache-2.0 | https://www.nuget.org/packages/SixLabors.Fonts/1.0.0 |

## Distribution obligations

The copyright and license notices remain the property of their respective upstream authors and projects.

- MIT-licensed components must retain their applicable copyright and permission notices when redistributed.
- `SixLabors.Fonts 1.0.0` is licensed under Apache License 2.0; redistribution must preserve the Apache license and any applicable upstream notices.
- Release packaging must not strip license/notice material supplied by the resolved packages. Release Prep must verify the final distributable against this locked graph and copy the exact upstream license/NOTICE texts required by the packages that are actually shipped.

This inventory is a bootstrap/supply-chain record, not a claim that every future dependency is covered. If the locked runtime graph changes, this file must be rechecked against the new graph.

## Not part of the shipped application runtime graph

`xunit.v3 4.0.0` is a test dependency in `tests/WordToExcel.Tests` and is not part of the portable application runtime publish. Its package license remains governed by its upstream package metadata.

No third-party icon package or bundled font file is currently materialized in the application scaffold.
