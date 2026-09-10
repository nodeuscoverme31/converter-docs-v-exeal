# 06 — Technical Plan

PHASE: 06_TECHNICAL_PLAN  
STATUS: COMPLETE  
PROJECT_MODE: GREENFIELD

## Inputs Checked

Проверены и согласованы:

- `01_PROJECT_INTENT.md`
- `02_MVP_SPEC.md`
- `03_PROJECT_RULES.md`
- `04_PRODUCT_UX_DESIGN.md`
- `05_VISUAL_UI_DESIGN.md`
- `05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.png`

Критическая цепочка сохранена:

`REQUIREMENTS → UX → VISUAL → TECHNICAL HOW`

Противоречий, требующих возврата в Фазы 01–05, не обнаружено.

Главные технические ограничения:

- Windows desktop utility;
- один `.doc` / `.docx` за запуск;
- один `.xlsx` на выходе;
- каждая Word-таблица → отдельный Excel-лист;
- обычный текст документа сохраняется отдельно;
- нулевая допустимая молчаливая потеря поддерживаемых данных;
- рабочая Excel-сетка важнее визуального копирования Word;
- значения с ведущими нулями, длинные коды и другие потенциально опасные значения нельзя портить автоматической типизацией;
- картинки/неподдерживаемые объекты не переносятся, но вызывают предупреждение;
- исходный Word не изменяется;
- приложение должно работать локально, без обязательного Word/Excel/Office и без интернета;
- portable/xcopy-поставка;
- UX: одно окно, drag & drop + file picker, явный запуск, success/warning/error;
- visual target: Windows Native / Fluent;
- OCR и batch не входят в MVP.

---

## Technical Requirements

### TR-001 — Полностью локальный процесс

Вся конвертация выполняется внутри локального Windows-процесса.

Запрещено как обязательное runtime-поведение:

- загрузка документа в облако;
- вызов внешнего API;
- наличие аккаунта;
- наличие интернета;
- Automation/Interop с установленным Microsoft Word или Excel.

### TR-002 — Portable runtime

Пользователь получает self-contained Windows build, который включает нужный .NET runtime.

Отдельная установка .NET Desktop Runtime пользователем не требуется.

Основной формат доставки MVP: папка с self-contained `win-x64` build, которую можно копировать на USB и запускать напрямую.

Single-file EXE не является обязательным требованием MVP.

### TR-003 — Единый внутренний путь после чтения Word

`.docx` читается напрямую.

`.doc` сначала преобразуется во временный `.docx`, после чего проходит через тот же DOCX parser.

Это уменьшает количество параллельных парсеров и правил нормализации.

### TR-004 — Source-preserving intermediate model

Между Word parser и Excel writer существует собственная маленькая внутренняя модель документа.

Она обязана хранить:

- точный исходный текст;
- источник/порядок элемента;
- структуру таблицы;
- spans/merge information;
- предупреждения и provenance.

Excel writer не должен читать Word напрямую.

### TR-005 — Безопасная публикация XLSX

Финальный `.xlsx` не публикуется пользователю сразу после записи.

Порядок:

1. записать временный XLSX;
2. повторно открыть его независимым readback/validation path;
3. проверить structural invariants и сохранность поддерживаемых данных;
4. только после PASS переместить/переименовать во финальное безопасное имя;
5. при validation failure удалить/не публиковать временный результат и показать ошибку.

### TR-006 — Никакого silent overwrite

Если `Имя.xlsx` уже существует, создаётся безопасное уникальное имя:

- `Имя (1).xlsx`
- `Имя (2).xlsx`
- и т.д.

Существующий файл не перезаписывается молча.

### TR-007 — Не исполнять активное содержимое документов

Парсер читает структуру файла как данные.

Макросы, embedded executables, OLE automation и внешние команды не исполняются.

---

## Unknowns Resolved

### UI stack

РЕШЕНО: `.NET 10 LTS + C# 14 + WPF`.

Почему:

- продукт только для Windows;
- WPF входит в Windows Desktop stack .NET;
- .NET 9+ имеет встроенный Fluent theme;
- WPF позволяет реализовать file picker, drag & drop, keyboard focus, high-DPI и desktop layout без browser/runtime shell;
- self-contained publish позволяет передать приложение без установленного .NET;
- .NET 10 — LTS до ноября 2028.

### Visual theme

РЕШЕНО: использовать встроенный WPF Fluent resource dictionary, а не отдельную крупную UI-библиотеку.

Базовый ресурс:

`/PresentationFramework.Fluent;component/Themes/Fluent.xaml`

Причина:

- визуально соответствует подтверждённому target;
- не вводит ещё один UI framework;
- не требует зависеть от экспериментального `ThemeMode` API;
- custom XAML используется только там, где built-in controls недостаточно близки target.

### `.docx`

РЕШЕНО: `DocumentFormat.OpenXml 3.5.1`.

Используется для:

- чтения WordprocessingML;
- обнаружения paragraphs/tables/drawings;
- получения table grid/span/merge properties;
- low-level validation;
- независимого readback готового XLSX.

### `.doc`

РЕШЕНО УСЛОВНО: `DocSharp.Binary.Doc 0.21.x` как основной кандидат legacy DOC → DOCX.

Причина:

- pure C#;
- без Office Interop;
- без обязательных native dependencies для этого conversion path;
- специально конвертирует Office 97–2003 `.doc` в `.docx`;
- совместим с современным .NET;
- MIT;
- существенно легче LibreOffice.

Но это НЕ считается доказанной fidelity-гарантией.

`TECHNICAL_SPIKE_REQUIRED: LEGACY-DOC-001`

До принятия `.doc` pipeline в реализацию требуется отдельная проверка на corpus сложных `.doc`.

Fallback при провале spike: LibreOffice portable/headless conversion должен быть технически оценён повторно.

### `.xlsx`

РЕШЕНО: `ClosedXML 0.105.1` для создания workbook.

Причина:

- зрелая .NET abstraction над Open XML;
- не требует Excel;
- MIT;
- существенно уменьшает количество низкоуровневого XLSX-кода;
- Open XML SDK остаётся независимым validation/readback механизмом.

### Persistence

`NO_PERSISTENCE_REQUIRED`

Нет:

- database;
- server state;
- user accounts;
- cloud storage;
- queue.

Состояние одной конвертации живёт только в памяти и во временных файлах.

### Internet

`NO_EXTERNAL_RUNTIME_INTEGRATIONS`

Приложение не имеет runtime-сетевых интеграций.

---

## Technical Prior-Art / Reuse

### CANDIDATE-01 — .NET 10 LTS + WPF

**Что закрывает:**  
Windows desktop runtime, UI, drag & drop, file picker, local execution, self-contained deployment.

**Version/release state:**  
На момент Фазы 06: .NET runtime `10.0.12`, SDK `10.0.401`, C# 14.

**Maintenance:**  
Microsoft active LTS support.

**Support horizon:**  
До 14 ноября 2028.

**License/trust:**  
Microsoft / .NET ecosystem.

**Platform/runtime fit:**  
STRONG FIT для Windows desktop.

**Operational complexity:**  
Низкая: один managed runtime, без browser shell.

**Known limitation:**  
Официальный список поддерживаемых ОС .NET 10 следует lifecycle Windows. Consumer Windows 10 вне актуального lifecycle не должен автоматически считаться официально поддерживаемым.

**Решение:**  
SELECTED.

---

### CANDIDATE-02 — WinUI 3 / Windows App SDK

**Что закрывает:**  
Более прямой современный Windows visual stack.

**Плюсы:**
- очень близкий native Windows 11 visual language;
- modern Windows App SDK.

**Минусы:**
- отдельный Windows App SDK deployment layer;
- self-contained unpackaged build включает дополнительные runtime assets;
- больше deployment complexity для маленького USB utility;
- преимуществ для нашего однооконного MVP недостаточно, чтобы оправдать усложнение.

**Решение:**  
REJECTED FOR MVP.

**REVISIT_WHEN:**  
если WPF не сможет достаточно близко воспроизвести выбранный visual target или появятся Windows-App-SDK-only requirements.

---

### CANDIDATE-03 — Open XML SDK 3.5.1

**Источник:**  
Microsoft/.NET Foundation Open XML SDK.

**Что закрывает:**  
Низкоуровневый доступ к DOCX/XLSX OOXML.

**Release state:**  
3.5.1, март 2026.

**Maintenance:**  
Активный официальный проект.

**License:**  
MIT.

**Trust:**  
Высокий для Open XML access.

**Fit:**  
STRONG FIT.

**Решение:**  
SELECTED.

---

### CANDIDATE-04 — ClosedXML 0.105.1

**Что закрывает:**  
Создание и чтение `.xlsx` через удобную .NET object model.

**Release state:**  
0.105.1, июль 2026.

**Maintenance:**  
Активный проект.

**License:**  
MIT.

**Platform/runtime fit:**  
Совместим с современным .NET.

**Operational complexity:**  
Низкая.

**Known limitation:**  
Публичный API проекта не обещает абсолютную стабильность между версиями; version update должен проходить regression fixtures.

**Решение:**  
SELECTED для XLSX writing.

**REVISIT_WHEN:**  
если writer мешает точной value/style serialization или validation выявляет расхождения, которые проще контролировать raw Open XML writer.

---

### CANDIDATE-05 — DocSharp.Binary.Doc 0.21.x

**Что закрывает:**  
Legacy Word `.doc` → `.docx`.

**Maintenance:**  
Активно обновлялся в 2025–2026.

**License:**  
MIT для основной DocSharp-линейки.

**Platform/runtime fit:**  
Pure C#, заявлена совместимость с .NET 8/9/10.

**Dependencies:**  
Связан с Open XML SDK / DocSharp.Binary components.

**Privacy:**  
Локальная конвертация.

**Operational complexity:**  
Низкая по сравнению с внешним office suite.

**Migration/exit cost:**  
Низкий, потому что он спрятан за `ILegacyDocConverter`.

**Known limitations / risk:**  
Сам maintainer называет DocSharp hobby project; roadmap прямо включает поддержку дополнительных элементов и исправление edge cases. Следовательно, README недостаточно для утверждения «данные сохраняются без потерь».

**Решение:**  
CONDITIONAL SELECTED, только после `LEGACY-DOC-001`.

**REVISIT_WHEN:**  
spike выявляет потерю таблиц, текста, Unicode, merges, nested structures или другие необозначенные расхождения.

---

### CANDIDATE-06 — LibreOffice 26.2.x portable/headless

**Что закрывает:**  
Зрелое чтение `.doc` и конвертацию через CLI `--convert-to`.

**Maintenance:**  
Активный офисный suite.

**License:**  
Open-source; распространение portable-версии возможно согласно проекту.

**Platform fit:**  
Windows portable возможен; headless CLI поддерживается.

**Плюсы:**
- зрелая Word compatibility;
- хорошо известный conversion engine;
- `.doc` поддерживается давно.

**Минусы:**
- огромный runtime относительно нашей утилиты;
- portable-версия включает целый офисный suite;
- больше файлов, startup/profile complexity;
- сложнее security-isolation;
- противоречит принципу минимально достаточной зависимости, хотя формально не нарушает «Office не установлен».

**Решение:**  
FALLBACK, не включать в MVP по умолчанию.

**REVISIT_WHEN:**  
`DocSharp` не проходит legacy fidelity spike.

---

### CANDIDATE-07 — NPOI 2.8.0

**Что закрывает:**  
Часть Office formats в .NET без Microsoft Office.

**Плюсы:**  
Зрелый проект, большой охват Excel.

**Проблемы:**
- официальный README для 2.8.0 перечисляет `xls`, `xlsx`, `docx`, но не `.doc` как основной поддерживаемый формат;
- legacy Word code находится в scratchpad/частичных областях;
- с 2.8.0 binary/NuGet distribution имеет дополнительный maintenance-fee EULA для revenue-generating users.

**Решение:**  
REJECTED.

Причина: не решает критическую `.doc` задачу достаточно чисто и добавляет ненужную лицензионную/операционную неоднозначность.

---

### CANDIDATE-08 — Apache POI HWPF / Java

**Что закрывает:**  
Legacy Word `.doc` в Java.

**Плюсы:**  
Долгая история, Apache ecosystem.

**Проблемы:**  
Сам проект описывает HWPF/XWPF как только “moderately functional”, а HWPF остаётся в scratchpad/ограниченно поддерживаемой области. Кроме того, Java-runtime создаёт второй runtime рядом с .NET UI.

**Решение:**  
REJECTED.

---

### CANDIDATE-09 — docx2csv

**Что закрывает:**  
Извлечение таблиц из `.docx`.

**Плюсы:**  
Простой готовый table-extraction concept.

**Минусы:**  
Не закрывает одновременно:
- `.doc`;
- обычный текст;
- наши loss-control invariants;
- Windows-native UI;
- единый .NET portable runtime.

**Решение:**  
REFERENCE ONLY, runtime dependency не нужен.

---

## Selected Stack

### Runtime / Language

- `.NET 10 LTS`
- target framework: `net10.0-windows`
- C# 14
- primary runtime architecture: `win-x64`

Bootstrap baseline:

- SDK: `10.0.401`
- Runtime/Desktop Runtime: `10.0.12`

Patch versions должны обновляться только после проверки build/tests; .NET 10 minor line остаётся `10.0`.

### Desktop UI

- WPF
- built-in `.NET 9+` Fluent resource dictionary
- обычный XAML
- без стороннего enterprise UI kit

### Word / Office parsing

- `DocumentFormat.OpenXml 3.5.1` — `.docx`
- `DocSharp.Binary.Doc 0.21.x` — `.doc → temporary .docx`, условно после spike

### Excel writing

- `ClosedXML 0.105.1`

### Output verification

- `DocumentFormat.OpenXml 3.5.1`
- собственные semantic invariants поверх readback

### Tests

Предпочтение для Bootstrap:

- xUnit v3 line, pinned version after Phase 07 restore verification;
- `Microsoft.NET.Test.Sdk`;
- без FluentAssertions/Verify/AutoFixture на старте.

Причина: стандартных assertions достаточно; дополнительные test abstraction libraries не нужны до доказанной пользы.

### Icons

Предпочтение:

- небольшой фиксированный набор локальных vector/SVG/path assets;
- Microsoft Fluent System Icons — reuse candidate, лицензию конкретных выбранных assets повторно проверить в Phase 07 и сохранить notice;
- НЕ распространять Segoe Fluent icon font.

### Typography

- использовать системный `Segoe UI Variable` там, где он доступен;
- fallback: системный `Segoe UI`;
- не включать font files в portable package.

---

## Architecture

Тип:

`LOCAL MODULAR MONOLITH`

Один desktop process, один production project, один test project.

Высокоуровневый pipeline:

```text
UI
 ↓
Conversion Orchestrator
 ↓
Input Detector
 ↓
┌────────────────────────────┐
│ .docx → OpenXml Reader     │
│ .doc  → DocSharp → .docx   │
└────────────────────────────┘
 ↓
Document Model
 ↓
Table Normalizer + Value Policy
 ↓
ClosedXML Writer
 ↓
Temporary XLSX
 ↓
OpenXml Readback Validator
 ↓
Publish final XLSX
 ↓
Conversion Report → UI
```

### COMPONENT: UI

**RESPONSIBILITY:**  
Показывать Phase-04 states и принимать одно действие пользователя.

**INPUTS:**  
selected file, conversion state, report.

**OUTPUTS:**  
select/start/retry/open-folder intents.

**DEPENDS_ON:**  
Conversion Orchestrator.

**MUST_NOT_DEPEND_ON:**  
Open XML SDK, ClosedXML, DocSharp internals.

**FAILURE_BOUNDARY:**  
UI exception не должна изменять исходный Word.

---

### COMPONENT: Conversion Orchestrator

**RESPONSIBILITY:**  
Управлять одной conversion session.

**INPUTS:**  
source path, destination policy.

**OUTPUTS:**  
`ConversionResult`.

**DEPENDS_ON:**  
Input Detector, Word Reader, Normalizer, Writer, Validator, Publisher.

**MUST_NOT_DEPEND_ON:**  
WPF visual controls.

**FAILURE_BOUNDARY:**  
Любая ошибка приводит к controlled ERROR/PARTIAL result и cleanup временных файлов.

---

### COMPONENT: Input Detector

**RESPONSIBILITY:**  
Проверить extension/тип контейнера/базовую читаемость.

**INPUTS:**  
file path.

**OUTPUTS:**  
`InputKind = Doc | Docx | Unsupported` либо typed failure.

**DEPENDS_ON:**  
filesystem + lightweight format inspection.

**MUST_NOT_DEPEND_ON:**  
Excel writer.

**FAILURE_BOUNDARY:**  
Неверный/повреждённый input не проходит дальше как valid.

---

### COMPONENT: Legacy DOC Converter

**RESPONSIBILITY:**  
Конвертировать `.doc` в temporary `.docx`.

**INPUTS:**  
read-only `.doc`.

**OUTPUTS:**  
temporary `.docx` либо typed error.

**DEPENDS_ON:**  
DocSharp.Binary.Doc.

**MUST_NOT_DEPEND_ON:**  
UI, ClosedXML.

**FAILURE_BOUNDARY:**  
Провал conversion не создаёт final XLSX.

---

### COMPONENT: DOCX Reader

**RESPONSIBILITY:**  
Превратить WordprocessingML в `DocumentModel`.

**INPUTS:**  
`.docx`.

**OUTPUTS:**  
paragraphs, tables, unsupported-object records, provenance.

**DEPENDS_ON:**  
Open XML SDK.

**MUST_NOT_DEPEND_ON:**  
WPF, ClosedXML.

**FAILURE_BOUNDARY:**  
Parse corruption/unsupported ambiguity становится typed warning/error.

---

### COMPONENT: Table Normalizer

**RESPONSIBILITY:**  
Преобразовать Word table layout в прямоугольную logical grid без потери source-cell content.

**INPUTS:**  
source table model.

**OUTPUTS:**  
normalized table + normalization warnings.

**DEPENDS_ON:**  
pure domain model only.

**MUST_NOT_DEPEND_ON:**  
Open XML SDK/ClosedXML/UI.

**FAILURE_BOUNDARY:**  
Неоднозначность не угадывается — возвращается warning/error.

---

### COMPONENT: Value Policy

**RESPONSIBILITY:**  
Решать, как записать каждое значение в Excel, не меняя значимое исходное содержимое.

**INPUTS:**  
`SourceText`, optional source hints.

**OUTPUTS:**  
`ExcelCellValuePlan`.

**DEPENDS_ON:**  
pure domain code.

**MUST_NOT_DEPEND_ON:**  
UI.

**FAILURE_BOUNDARY:**  
При сомнении выбирает lossless text representation.

---

### COMPONENT: XLSX Writer

**RESPONSIBILITY:**  
Создать temporary workbook.

**INPUTS:**  
normalized model.

**OUTPUTS:**  
temporary `.xlsx`.

**DEPENDS_ON:**  
ClosedXML.

**MUST_NOT_DEPEND_ON:**  
Word parser.

**FAILURE_BOUNDARY:**  
Writer failure не публикует final output.

---

### COMPONENT: Output Validator

**RESPONSIBILITY:**  
Открыть созданный XLSX независимо от writer-level state и проверить invariants.

**INPUTS:**  
temporary `.xlsx`, expected `DocumentModel`.

**OUTPUTS:**  
PASS / FAIL + validation findings.

**DEPENDS_ON:**  
Open XML SDK + domain comparison.

**MUST_NOT_DEPEND_ON:**  
ClosedXML object graph, использованный при записи.

**FAILURE_BOUNDARY:**  
FAIL блокирует публикацию как успешного результата.

---

### COMPONENT: Output Publisher

**RESPONSIBILITY:**  
Выбрать безопасное уникальное имя и опубликовать только validated file.

**INPUTS:**  
validated temp file + desired destination.

**OUTPUTS:**  
final path.

**DEPENDS_ON:**  
filesystem.

**MUST_NOT_DEPEND_ON:**  
parsers.

**FAILURE_BOUNDARY:**  
save/move failure сохраняет source untouched и возвращает recoverable save error.

---

## Data Model

Persistence:

`NO_PERSISTENCE_REQUIRED`

Минимальная внутренняя модель:

```text
DocumentModel
  SourceDocument
  Blocks[]
  Tables[]
  UnsupportedObjects[]
  Findings[]

DocumentBlock
  SourceOrder
  Kind = Paragraph | TableReference | UnsupportedObject
  SourceLocation

ParagraphBlock
  SourceTextExact

TableModel
  TableId
  SourceOrder
  ParentTableId?        // для nested table
  ParentCellId?
  Rows[]
  SourceGridHints
  Findings[]

SourceCell
  CellId
  SourceTextExact
  RowIndex
  SourceCellIndex
  GridSpan
  VerticalMerge
  NestedTableIds[]
  SourceLocation

NormalizedTable
  TableId
  Width
  Height
  Cells[]
  Findings[]

NormalizedCell
  SourceCellId?
  Row
  Column
  SourceTextExact
  OutputValuePlan

OutputValuePlan
  Mode = Text | SafeNumber | SafeDate
  ExpectedSourceText
  ExcelValue
  NumberFormat?
```

### Invariants

1. Каждый поддерживаемый source text fragment имеет provenance.
2. Один `SourceCell` не исчезает без finding.
3. Значение source cell не дублируется автоматически в несколько normalized cells только из-за merge.
4. Covered cells после merge normalization могут быть пустыми; исходное значение остаётся ровно в anchor cell.
5. Любая generated metadata строка должна быть отличима от исходных данных.
6. Числовая/датовая типизация допускается только если verifier может доказать сохранность значимого значения.
7. При сомнении `Mode = Text`.

---

## Word Parsing / Normalization Rules

### DOCX block order

Reader проходит document body в исходном порядке и различает:

- paragraphs;
- tables;
- drawings/unsupported objects.

Обычный текст не смешивается внутрь table sheets.

### Multiple tables

Верхнеуровневые таблицы получают стабильные logical IDs:

- `Table-001`
- `Table-002`
- ...

Excel sheet names по умолчанию:

- `Таблица 1`
- `Таблица 2`
- ...

Это избегает угадывания названия по содержимому и ограничений Excel sheet names.

### Context sheet

Обычный текст хранится в отдельном листе:

`Контекст`

Минимальные колонки:

- `Порядок`
- `Тип`
- `Содержимое`

Для места таблицы может использоваться generated reference `Таблица 1`, чтобы сохранить относительный порядок текста и таблиц.

Generated metadata не выдаётся за исходный текст.

### Merged cells

Word merge/grid-span нормализуется в rectangular occupancy grid.

Правило:

- source content хранится в anchor logical cell;
- covered logical cells остаются пустыми;
- значение не размножается по всем покрытым ячейкам;
- если topology невозможно определить однозначно, создаётся warning.

### Nested tables

Reader должен проходить таблицы рекурсивно.

Nested table:
- получает отдельный `TableId` и отдельный Excel sheet;
- связь `ParentTableId + ParentCellId` сохраняется в internal model / context metadata;
- её cell content не должен молча пропадать или считаться обычным текстом родительской ячейки.

Конкретная fidelity nested tables входит в parser integration tests.

### Empty cells / rows

Пустая source cell остаётся пустой логической ячейкой; отсутствие значения не должно сдвигать соседние значения.

### Formatting

MVP не обязан воспроизводить Word formatting один в один.

Разрешено сохранять только простые visual properties, если это:
- не меняет data;
- не мешает working-grid;
- не усложняет core pipeline.

Formatting не является blocking requirement первой реализации.

---

## Value Safety Policy

Приоритет:

`EXACT SOURCE VALUE > CONVENIENT EXCEL TYPE`

### Всегда как text при риске изменения

Примеры:

- `001234`
- цифровая последовательность длиннее безопасной Excel numeric precision;
- идентификатор/код;
- значение, где decimal/date parsing зависит от locale;
- строка с значимыми leading/trailing characters;
- неизвестный/неоднозначный формат.

### Safe number

Можно записать как numeric только если:

1. parsing однозначен по утверждённым правилам;
2. Excel способен представить значение без потери;
3. обратная semantic check подтверждает эквивалентность исходнику.

### Safe date

Дата типизируется только при однозначной интерпретации.

Например, строка вроде `10.09.2026` не должна интерпретироваться через случайную системную locale.

Первый безопасный MVP может сознательно хранить неоднозначные даты как text.

### Formula safety

Исходный Word text, начинающийся с `=`, `+`, `-`, `@`, не должен автоматически становиться Excel formula только потому, что похож на неё.

Если это source text, он сохраняется как text.

Это одновременно:
- предотвращает изменение данных;
- снижает риск formula injection при открытии XLSX.

---

## Interfaces / Contracts

### CONTRACT-01 — IWordDocumentReader

**CONSUMER:** Conversion Orchestrator  
**PROVIDER:** DOCX Reader

**INPUT:** readable `.docx` path/stream  
**OUTPUT:** `DocumentModel`  
**ERROR MODEL:** `CorruptDocument`, `ProtectedDocument`, `UnsupportedStructure`, `ReadFailure`  
**VERSIONING NEED:** internal only; no public versioning.

---

### CONTRACT-02 — ILegacyDocConverter

**CONSUMER:** Conversion Orchestrator  
**PROVIDER:** DocSharp adapter

**INPUT:** `.doc` path/stream  
**OUTPUT:** temporary `.docx`  
**ERROR MODEL:** `UnsupportedLegacyDoc`, `ProtectedDocument`, `ConversionFailure`  
**VERSIONING NEED:** internal.

Зависимость DocSharp находится только за этим boundary.

---

### CONTRACT-03 — ITableNormalizer

**CONSUMER:** Conversion Orchestrator  
**PROVIDER:** domain normalizer

**INPUT:** `TableModel`  
**OUTPUT:** `NormalizedTable` + findings  
**ERROR MODEL:** `AmbiguousLayout`  
**VERSIONING NEED:** internal.

---

### CONTRACT-04 — IExcelWorkbookWriter

**CONSUMER:** Conversion Orchestrator  
**PROVIDER:** ClosedXML adapter

**INPUT:** normalized document model  
**OUTPUT:** temporary XLSX  
**ERROR MODEL:** `WriteFailure`  
**VERSIONING NEED:** internal.

---

### CONTRACT-05 — IOutputValidator

**CONSUMER:** Conversion Orchestrator  
**PROVIDER:** OpenXml validation adapter

**INPUT:** temporary XLSX + expected document model  
**OUTPUT:** validation report  
**ERROR MODEL:** `InvalidPackage`, `DataMismatch`, `StructureMismatch`  
**VERSIONING NEED:** internal.

---

### CONTRACT-06 — ConversionResult

UI получает только продуктовую модель:

```text
Status = Success | Warning | Error
OutputPath?
Warnings[]
ErrorCategory?
UserMessage
```

UI не должен интерпретировать parser-specific exceptions.

---

## External Integrations

`NO_EXTERNAL_RUNTIME_INTEGRATIONS`

Нет:
- API;
- OAuth;
- database;
- cloud;
- telemetry backend;
- network service.

NuGet используется только при разработке/build, не во время пользовательской конвертации.

---

## Security Boundaries

### Untrusted input

Любой `.doc` / `.docx` считается недоверенным файлом.

### Защиты

1. Исходный файл открывается read-only.
2. Не использовать Office COM Automation.
3. Не исполнять VBA/macros.
4. Не запускать embedded objects.
5. Не переходить по внешним hyperlinks/resources ради конвертации.
6. File path canonicalize перед чтением/записью.
7. Не писать поверх исходного файла.
8. Temp files создаются в уникальной per-run директории.
9. Temp cleanup выполняется best-effort и на success, и на handled failure.
10. Финальный XLSX публикуется только после validation.
11. Source text, похожий на Excel formula, не активируется как formula автоматически.
12. Runtime не делает сетевых запросов.

### Resource exhaustion

Точный максимальный размер Word-файла неизвестен.

Не вводить выдуманный продуктовый лимит на Фазе 06.

В Phase 07/11 необходимо измерить:
- память;
- время;
- поведение на больших tables/documents.

Если потребуется limit, он должен быть основан на measurement и отражён в requirements/UX, если станет видимым пользователю.

### Supply chain

Минимизировать dependencies.

Перед bootstrap:
- pin versions;
- проверить current package metadata;
- проверить known advisories;
- сохранить third-party license notices для реально включённых библиотек/assets.

---

## Error / Recovery Technical Model

### ERROR-INPUT-UNSUPPORTED

Причина:
не `.doc/.docx` либо container не соответствует ожидаемому формату.

UX:
`Этот файл не поддерживается`.

Recovery:
выбрать другой файл.

### ERROR-PROTECTED

Причина:
encrypted/password-protected input, который library не может прочитать.

UX:
`Документ защищён и не может быть преобразован`.

Recovery:
выбрать другой/предварительно разблокированный файл.

### ERROR-CORRUPT

Причина:
повреждённый package/OLE/container.

UX:
`Не удалось прочитать документ`.

Recovery:
другой файл.

### ERROR-LEGACY-CONVERT

Причина:
`.doc → .docx` adapter failure.

UX:
не выдавать ложный success.

Recovery:
другой файл; diagnostics для разработки.

### WARNING-UNSUPPORTED-OBJECT

Причина:
drawing/image/OLE/другой объект не переносится.

UX:
`Готово с предупреждениями`.

### WARNING-AMBIGUOUS-TABLE

Причина:
table topology не нормализована однозначно.

UX:
`Готово с предупреждениями` только если supported data сохранены; иначе ERROR validation.

### ERROR-OUTPUT-WRITE

Причина:
нет прав / destination недоступен / носитель отключён.

Recovery:
выбрать другое место и повторить save/conversion.

### ERROR-VALIDATION

Причина:
готовый XLSX не проходит structural/data readback.

UX:
`Не удалось преобразовать`.

Критично:
файл НЕ публикуется как успешный.

### Idempotency

Повторный запуск на том же source:
- не изменяет source;
- создаёт новый безопасный unique output name;
- не зависит от старой незавершённой session.

### Cancellation

В MVP отдельная cancellation infrastructure не проектируется.

`REVISIT_WHEN`:
реальные замеры показывают длительные операции, где отсутствие Cancel ухудшает UX.

---

## Performance / Scale

**CURRENT_EXPECTED_SCALE:**  
Один локальный пользователь, один документ за один запуск, последовательная обработка.

**KNOWN_LIMIT:**  
Не установлен.

**PERFORMANCE_REQUIREMENT:**  
Конкретное время не задано пользователем.

**MEASUREMENT:**
- обычный маленький fixture;
- multiple-table fixture;
- сложный merged fixture;
- существующий synthetic large Word fixture;
- legacy `.doc` fixtures;
- измерять elapsed time и peak working set.

Не добавлять:
- queue;
- parallel document processing;
- worker pool;
- caching.

Пока нет evidence, что они нужны.

---

## Observability

Для MVP:

- нет telemetry;
- нет analytics;
- нет network logging;
- UI получает typed result/warnings/errors;
- tests/build сохраняют обычный console/test output.

Runtime diagnostic file logging не является обязательным.

`REVISIT_WHEN`:
ошибки на реальных документах невозможно диагностировать без сохраняемого технического отчёта.

Если такой отчёт добавится:
- не логировать cell/document text по умолчанию;
- не логировать secrets;
- явно отделить diagnostics от пользовательского результата.

---

## Testing / Validation Strategy

### Unit

#### Parser-independent domain tests

- table occupancy/grid normalization;
- horizontal span;
- vertical merge;
- irregular rows;
- empty cells;
- nested-table relationship model;
- output naming;
- unique-name collision policy.

#### Value Policy

Обязательные fixtures:

- `001234`;
- длинный цифровой код > Excel safe precision;
- обычное integer;
- decimal with comma;
- decimal with dot;
- `10.09.2026`;
- ambiguous date;
- Unicode/Cyrillic;
- line breaks;
- strings starting `=`, `+`, `-`, `@`;
- leading/trailing spaces where significant.

### Integration — DOCX

Проверить Open XML reader на:

- one table;
- many tables;
- text before/between/after tables;
- merged cells;
- irregular grid;
- nested table;
- images;
- empty cells;
- Unicode;
- malformed package;
- password/encryption behavior where applicable.

### Integration — Legacy DOC

`TECHNICAL_SPIKE_REQUIRED: LEGACY-DOC-001`

Corpus минимум по категориям:

1. простая `.doc` таблица;
2. несколько таблиц;
3. merged cells;
4. split/irregular rows;
5. Cyrillic/Unicode;
6. leading-zero codes;
7. long numeric identifiers;
8. surrounding text;
9. nested table;
10. embedded image;
11. protected `.doc`;
12. damaged `.doc`.

Spike procedure:

```text
.doc
  ↓ DocSharp.Binary.Doc
temporary .docx
  ↓ OpenXml Reader
DocumentModel
  ↓ compare with fixture truth
PASS / FAIL
```

Pass criteria:

- все ожидаемые таблицы найдены;
- source text/cell values не исчезли и не изменились без finding;
- Cyrillic корректен;
- merged structure достаточно восстанавливается для normalizer;
- images/unsupported objects обнаруживаются;
- ни Office, ни native external process не требуются.

Если FAIL:
1. не чинить requirement обходом;
2. оценить LibreOffice headless fallback;
3. если fallback неприемлем по размеру/поведению — `RETURN_TO_PHASE: 06_TECHNICAL_PLAN`, а при невозможности поддержать `.doc` в заданных ограничениях — `RETURN_TO_PHASE: 02_MVP_SPEC`.

### Integration — XLSX writer/readback

После ClosedXML save:

- Open XML package валиден;
- ожидаемое число table sheets;
- sheet names deterministic;
- `Контекст` присутствует при наличии ordinary text;
- cell values соответствуют `OutputValuePlan`;
- text values сохраняются exact;
- no accidental formulas;
- no silent overwrite.

### End-to-End

Минимум:
- clean `.docx`;
- clean `.doc`;
- multi-table;
- merged/irregular;
- warning image;
- corrupt/protected;
- leading-zero / long numeric values.

### Portable environment

Проверка на clean Windows machine/VM:

- без Microsoft Office;
- без установленного .NET runtime;
- internet disabled;
- app стартует с self-contained publish;
- `.docx` конвертируется;
- после legacy spike — `.doc` конвертируется;
- output открывается в независимом XLSX reader и при наличии тестовой машины — Excel/LibreOffice как дополнительная совместимость, не runtime dependency.

### Visual / UI validation

Сравнить:

`05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.png ↔ rendered WPF UI`

Проверить:
- hierarchy;
- drop zone;
- primary button;
- selected-file block;
- processing;
- success;
- warning;
- error;
- long filename;
- keyboard focus;
- Windows text scaling/high DPI.

Pixel-perfect identity не требуется; UX hierarchy и выбранный visual language должны сохраниться.

---

## End-to-End Validation

### VALIDATION-01 — Clean DOCX, exact values

**Prerequisites:**  
Self-contained Windows build; clean test machine; Office not required.

**Input:**  
DOCX fixture с:
- обычным текстом;
- 2 таблицами;
- `001234`;
- длинным цифровым кодом;
- обычным числом;
- Cyrillic.

**Action:**  
Выбрать файл → `Преобразовать в Excel`.

**Expected observable result:**
- status `Готово`;
- создан один XLSX;
- есть `Таблица 1`, `Таблица 2`;
- ordinary text доступен в `Контекст`;
- `001234` не изменён;
- длинный код не изменён;
- исходный DOCX неизменён;
- XLSX проходит Open XML readback validation.

**Related:**  
SC-001/002/003/005; REQ-001…007; QREQ-001…005.

**Required services:**  
NONE.

**Evidence to capture:**  
test result + output validator report + source/output canonical comparison.

---

### VALIDATION-02 — Warning path

**Input:**  
DOCX fixture с нормальной таблицей и embedded image.

**Expected:**
- XLSX создан;
- таблица сохранена;
- status `Готово с предупреждениями`;
- warning явно сообщает о найденном неподдерживаемом объекте;
- image omission не маскируется.

**Related:**  
SC-004, REQ-008/009.

---

### VALIDATION-03 — Legacy DOC

**Input:**  
`.doc` fixture, прошедший corpus baseline.

**Expected:**
- legacy conversion path не требует Office;
- промежуточный DOCX временный;
- output проходит те же invariants, что DOCX path.

**Related:**  
REQ-001, QREQ-001/003/005.

**Status:**  
BLOCKED UNTIL `LEGACY-DOC-001` SPIKE PASSES.

---

## Visual Implementation Notes

### UI technology

WPF + built-in Fluent theme.

Не включать:
- Electron;
- browser shell;
- WebView как основной UI;
- отдельный third-party Fluent component suite без доказанной необходимости.

### Theme

Начальный MVP:
- Light visual target;
- built-in Fluent resource dictionary;
- system accent может быть использован только если не ломает target hierarchy.

Dark mode:
- не является requirement;
- не проектировать отдельную тему в Bootstrap;
- `REVISIT_WHEN` пользователь потребует её или system-theme support окажется почти бесплатным без визуальных регрессий.

### Mica/backdrop

Mica не является requirement.

Если built-in WPF/window backdrop на целевой Windows версии работает без extra dependency и соответствует target — допустимо.

Если нет — использовать обычную neutral background.

Нельзя делать Mica blocker для MVP.

### Fonts

Не bundle font files.

Font stack:
- Segoe UI Variable;
- Segoe UI fallback.

### Icons

Использовать малый набор local vector assets.

Обязательная Phase-07 проверка:
- точная лицензия;
- NOTICE;
- корректность SVG/path import.

Не использовать icon font как обязательное условие cross-version UI.

---

## Windows Compatibility Decision

### Primary supported target

`Windows 11 x64`

Причина:
- выбран Windows-11/Fluent visual target;
- .NET 10 имеет актуальную официальную поддержку Windows 11;
- минимизирует ложные promises.

### Windows 10

`COMPATIBILITY TARGET — NOT YET PROMISED`

.NET 10 official supported-OS matrix следует Windows lifecycle и не включает обычный consumer Windows 10 22H2 как текущую поддерживаемую ОС.

Phase 07 должен выполнить smoke probe на реально доступной Windows 10 x64 машине/VM, если поддержку Windows 10 предполагается заявлять пользователю.

Если приложение работает — можно оставить best-effort compatibility.

Если не работает, не менять stack молча; решить, требуется ли продуктово поддерживать Windows 10.

---

## Technical Decisions

### DECISION-001 — .NET 10 + WPF

**Chosen:**  
.NET 10 LTS / C# 14 / WPF.

**Why:**  
Лучшее сочетание Windows-native UI, self-contained portable delivery и небольшой architectural complexity.

**Alternatives:**  
WinUI 3; web-shell desktop stack.

**Trade-offs:**  
WPF старше WinUI 3, но для одного desktop utility зрелость и простой deployment важнее modern API surface.

**Revisit when:**  
WPF не позволяет воспроизвести visual target или требуется API, доступное только Windows App SDK.

---

### DECISION-002 — Built-in WPF Fluent theme

**Chosen:**  
`PresentationFramework.Fluent` resource dictionary.

**Why:**  
Соответствует target без дополнительной UI dependency.

**Alternatives:**  
WinUI 3 styling; third-party Fluent UI kits; fully custom controls.

**Trade-offs:**  
Некоторые детали target всё равно потребуют локальных styles/templates.

**Revisit when:**  
visual convergence показывает существенный разрыв, который невозможно закрыть разумным объёмом XAML.

---

### DECISION-003 — OpenXML direct parser for DOCX

**Chosen:**  
DocumentFormat.OpenXml.

**Why:**  
Нужен полный контроль над block order, table grid, merge/span и unsupported-object detection.

**Alternatives:**  
docx2csv; high-level generic converter.

**Trade-offs:**  
Больше собственного parsing/normalization кода, зато loss/provenance rules контролируемы.

**Revisit when:**  
найден зрелый parser, который доказуемо сохраняет все нужные invariants и уменьшает код.

---

### DECISION-004 — DocSharp for legacy DOC, gated by spike

**Chosen:**  
DocSharp.Binary.Doc behind adapter.

**Why:**  
Pure C#, маленький, Office-free, переводит legacy DOC в тот же DOCX pipeline.

**Alternatives:**  
LibreOffice headless; Apache POI/HWPF; NPOI.

**Trade-offs:**  
Меньшая зрелость и adoption, поэтому нельзя доверять без corpus validation.

**Revisit when:**  
любой unmarked data/table mismatch в `LEGACY-DOC-001`.

---

### DECISION-005 — ClosedXML writer + OpenXML verifier

**Chosen:**  
ClosedXML for writing, Open XML SDK for independent readback.

**Why:**  
Writer остаётся простым, но success не зависит только от той же abstraction, которая создала файл.

**Alternatives:**  
полностью raw Open XML writing; ClosedXML write+read only.

**Trade-offs:**  
Две Office OpenXML libraries в package, но каждая решает отдельную роль и существенно снижает risk silent success.

**Revisit when:**  
package size или incompatibility превышает пользу независимого validation.

---

### DECISION-006 — Self-contained folder, not installer

**Chosen:**  
`win-x64`, self-contained, xcopy/USB folder.

**Why:**  
Не требует .NET/Office install и максимально соответствует portable scenario.

**Alternatives:**  
single-file; MSIX; installer.

**Trade-offs:**  
Несколько файлов в папке вместо одного EXE.

**Revisit when:**  
пользователь явно потребует один файл или распространение через Store.

---

### DECISION-007 — No database / no service / no queue

**Chosen:**  
In-memory session + temp files.

**Why:**  
MVP обрабатывает один локальный файл.

**Alternatives:**  
SQLite/state store; background worker.

**Trade-offs:**  
Нет resume после закрытия приложения — это уже соответствует UX Phase 04.

**Revisit when:**  
batch/resume/history становятся подтверждёнными requirements.

---

## Rejected Complexity

Не включать в MVP:

- database;
- web server;
- local HTTP API;
- microservices;
- message queue;
- background Windows service;
- updater;
- account/auth;
- telemetry backend;
- plugin system;
- OCR engine;
- batch scheduler;
- automatic Office installation;
- COM Interop;
- embedded LibreOffice по умолчанию;
- installer/MSIX как единственный способ запуска;
- enterprise UI component library;
- custom design-system framework;
- one-file executable как обязательный gate;
- aggressive trimming/AOT до доказанной необходимости;
- automatic header inference;
- automatic Excel ListObject/Table creation, если это может менять пустые/дублирующиеся заголовки;
- formula generation из Word text;
- hidden source-data worksheet только ради аудита.

---

## Bootstrap Contract

### Runtime

- .NET SDK baseline: `10.0.401`
- .NET runtime baseline: `10.0.12`
- target: `net10.0-windows`
- RID: `win-x64`
- language: C# 14
- UI: WPF

Phase 07 должна перепроверить эти версии перед созданием проекта.

### Dependency manager

NuGet / `dotnet restore`.

Pinned initial candidates:

- `DocumentFormat.OpenXml` `3.5.1`
- `ClosedXML` `0.105.1`
- `DocSharp.Binary.Doc` `0.21.x` — exact current stable version verify in Phase 07
- test framework: xUnit v3 stable line — exact selected version verify at bootstrap
- `Microsoft.NET.Test.Sdk` — exact stable version verify at bootstrap

Не добавлять дополнительные packages без конкретной необходимости.

### Planned solution structure

```text
/
├─ src/
│  └─ WordToExcel.App/
│     ├─ UI/
│     ├─ Conversion/
│     ├─ Model/
│     ├─ Word/
│     ├─ Excel/
│     ├─ Validation/
│     └─ Assets/
├─ tests/
│  └─ WordToExcel.Tests/
│     ├─ Unit/
│     ├─ Integration/
│     ├─ E2E/
│     └─ Fixtures/
├─ 01_PROJECT_INTENT.md
├─ 02_MVP_SPEC.md
├─ 03_PROJECT_RULES.md
├─ 04_PRODUCT_UX_DESIGN.md
├─ 05_VISUAL_UI_DESIGN.md
├─ 05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.png
└─ 06_TECHNICAL_PLAN.md
```

Один production project + один test project.

Не создавать дополнительные class-library projects, пока границы не окажутся реально слишком тяжёлыми для одного assembly.

### Initial modules

- `UI`
- `Conversion`
- `Model`
- `Word`
- `Excel`
- `Validation`
- `Assets`

### Env / config names

`NO_ENVIRONMENT_VARIABLES_REQUIRED`

`NO_SECRETS_REQUIRED`

Если появятся config values:
- только product-safe local settings;
- никаких API keys.

### Local services

`NONE`

### Planned commands

Все команды ниже:

`PLANNED_COMMAND — NOT YET VERIFIED`

```powershell
dotnet restore
```

```powershell
dotnet build -c Release
```

```powershell
dotnet test -c Release
```

```powershell
dotnet publish src/WordToExcel.App/WordToExcel.App.csproj -c Release -r win-x64 --self-contained true
```

Первый publish target:
- self-contained folder;
- `PublishSingleFile=false`;
- `PublishTrimmed=false`.

Single-file/trim разрешены только после отдельной проверки dependencies и запуска на clean machine.

### Checks

Phase 07 должна подготовить минимальные mechanical checks:

- build;
- tests;
- formatting verification;
- Open XML output validation;
- dependency/license check;
- no silent source overwrite;
- portable smoke test.

Планируемый formatter check:

`PLANNED_COMMAND — NOT YET VERIFIED`

```powershell
dotnet format --verify-no-changes
```

### Assets

- selected visual target PNG;
- только реально используемые local vector icons;
- license/NOTICE для third-party assets;
- никаких bundled font files.

### Operational instructions

После реального bootstrap создать один короткий repo-root operational instruction file, подходящий используемому агенту/среде.

Он должен:
- указывать source-of-truth 01–06;
- содержать только реально проверенные build/test commands;
- запрещать claims без verification;
- не дублировать весь project canon.

Конкретный filename/runtime projection определяет Phase 07 по реальной рабочей среде.

### CI / Deploy

Для Bootstrap MVP:

- CI не обязателен до появления repository workflow;
- deployment service отсутствует;
- publish — локальный self-contained build.

---

## Required Technical Spike

### TECHNICAL_SPIKE_REQUIRED: LEGACY-DOC-001

**Цель:**  
Доказать, что pure-.NET `.doc → .docx` path через DocSharp достаточно надёжен для требований нулевой молчаливой потери.

**Минимальный probe:**  
Создать/собрать fixture corpus, перечисленный в разделе Testing, и прогнать legacy conversion с последующим OpenXML parse/comparison.

**Обязательный PASS:**  
Нет необозначенной потери ожидаемого текстового/table content.

**Если FAIL:**  
Не выдавать `.doc` поддержку как готовую. Вернуться к DECISION-004 и проверить LibreOffice fallback.

**Статус:**  
NOT EXECUTED IN PHASE 06.

---

## Assumptions / Unknowns

### Assumptions

- Большинство реальных файлов пользователя — Office 97+ `.doc` или современный `.docx`.
- Один process достаточно для ожидаемой нагрузки.
- `win-x64` покрывает основной реальный сценарий.
- Basic WPF Fluent theme + local XAML достаточно близки выбранному visual target.

### Unknowns

- Реальная fidelity DocSharp на сложных `.doc`.
- Реальная скорость на больших документах.
- Peak memory.
- Нужен ли пользователю формальный Windows 10 support.
- Нужно ли Cancel после измерений.
- Насколько много unsupported objects встречается в реальных документах.
- Нужна ли долговременная diagnostic logging.
- Возможны ли отдельные legacy `.doc` варианты до Word 97 — текущий MVP их не обещает до evidence.
- Точный current stable DocSharp package version должен быть rechecked перед install.
- Exact icon asset selection/license notice должен быть rechecked.

Эти неизвестные явно передаются Phase 07/11 и не считаются уже решёнными фактами.

---

## Checked Sources

### Microsoft / .NET

- .NET 10 downloads:
  https://dotnet.microsoft.com/en-us/download/dotnet/10.0

- .NET support policy:
  https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core

- .NET 10 supported OS:
  https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md

- WPF Fluent theme:
  https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net90
  https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/styles-templates-overview

- .NET self-contained / single-file deployment:
  https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview
  https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/publish-first-app

### Open XML

- Open XML SDK:
  https://github.com/dotnet/Open-XML-SDK
  https://www.nuget.org/packages/DocumentFormat.OpenXml

### XLSX

- ClosedXML:
  https://github.com/ClosedXML/ClosedXML
  https://www.nuget.org/packages/ClosedXML

### Legacy DOC

- DocSharp:
  https://github.com/manfromarce/DocSharp
  https://www.nuget.org/packages/DocSharp.Binary.Doc

- LibreOffice:
  https://www.libreoffice.org/download-other/
  https://help.libreoffice.org/latest/en-US/text/shared/guide/start_parameters.html

- Apache POI HWPF:
  https://poi.apache.org/components/document/index.html

- NPOI:
  https://github.com/nissl-lab/npoi
  https://www.nuget.org/packages/NPOI

### Icons

- Microsoft Fluent System Icons:
  https://github.com/microsoft/fluentui-system-icons

---

## Handoff

NEXT_PHASE: 07_BOOTSTRAP  
RETURN_TO_PHASE: NONE

Передать в Фазу 07:

- `01_PROJECT_INTENT.md`
- `02_MVP_SPEC.md`
- `03_PROJECT_RULES.md`
- `04_PRODUCT_UX_DESIGN.md`
- `05_VISUAL_UI_DESIGN.md`
- `05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.png`
- `06_TECHNICAL_PLAN.md`
- selected dependency candidates;
- rejected alternatives;
- checked sources;
- Bootstrap Contract;
- `TECHNICAL_SPIKE_REQUIRED: LEGACY-DOC-001`.

PHASE_06_COMPLETE
