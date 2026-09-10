# 05 — Visual / UI Design

PHASE: 05_VISUAL_UI_DESIGN  
STATUS: COMPLETE

## Входы

- `01_PROJECT_INTENT.md`
- `02_MVP_SPEC.md`
- `03_PROJECT_RULES.md`
- `04_PRODUCT_UX_DESIGN.md`
- Visual comparison board with OPTION 1 / 2 / 3
- Confirmed OPTION 1 mockup
- Microsoft Windows / Fluent design guidance

## Visual Prior-Art / References

### Reference 1 — Windows 11 / Fluent design principles

Источник:  
https://learn.microsoft.com/en-us/windows/apps/design/design-principles

Что сильного:
- спокойная, ненавязчивая визуальная среда;
- знакомая пользователю Windows-иерархия;
- цвет используется прежде всего для фокуса и статуса;
- мягкая геометрия и layering без декоративного перегруза.

Что подходит проекту:
- локальная Windows-утилита должна ощущаться знакомой и простой;
- интерфейс не должен конкурировать с основной задачей;
- один главный action должен быть визуально очевиден.

Что не переносим:
- сложную навигацию и многостраничную структуру;
- декоративные элементы ради «Windows-похожести».

### Reference 2 — Windows typography

Источник:  
https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography

Что сильного:
- Segoe UI Variable как системная визуальная опора;
- Regular для обычного текста, Semibold для заголовков;
- sentence case;
- приоритет читаемости и ясной иерархии.

Что подходит проекту:
- короткие русские labels;
- ясные статусы;
- длинные имена файлов без визуального хаоса.

### Reference 3 — Windows color guidance

Источник:  
https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/color

Что сильного:
- нейтральная база;
- акцентный цвет используется экономно;
- цвет не является единственным носителем смысла;
- light/dark themes возможны как системное направление.

Что подходит проекту:
- синяя primary action;
- зелёный / жёлтый / красный только как semantic support для result states;
- текстовый статус обязателен вместе с цветом.

### Reference 4 — Windows command / layout guidance

Источники:  
https://learn.microsoft.com/en-us/windows/apps/design/basics/commanding-basics  
https://learn.microsoft.com/en-us/windows/apps/design/basics/navigation-basics

Что сильного:
- главное действие должно находиться непосредственно на рабочей поверхности;
- меньше навигации — меньше когнитивной нагрузки;
- стандартные и знакомые controls предпочтительнее custom-patterns без необходимости.

Что подходит проекту:
- одно окно;
- крупная primary button `Преобразовать в Excel`;
- drag & drop + обычный file picker;
- отсутствие sidebar-navigation / tabs / wizard.

## Reuse Candidates

### Fluent visual language
Статус: REUSE-КАНДИДАТ

Польза:
- соответствует Windows-target продукта;
- знакомая иерархия;
- зрелые status / control patterns;
- доступность и keyboard behavior можно строить на ожидаемых Windows-паттернах.

Передать Фазе 06 на проверку:
- какой технический UI stack способен достаточно точно воспроизвести target;
- доступны ли Mica / Fluent materials в выбранном runtime;
- как обеспечить portable-поставку без скрытой системной зависимости;
- какие icon assets можно использовать легально и локально.

### Segoe / Windows system typography
Статус: REUSE-КАНДИДАТ

Польза:
- визуально соответствует Windows;
- хорошая читаемость русского UI.

Передать Фазе 06:
- наличие и fallback на целевых версиях Windows;
- не требуется ли bundled font;
- как избежать лицензирования/распространения шрифта внутри portable-пакета.

### Fluent / Windows iconography
Статус: REUSE-КАНДИДАТ

Польза:
- знакомые действия: открыть папку, выбрать файл, warning, success, error.

Передать Фазе 06:
- допустимый источник и лицензия;
- способ использования без копирования proprietary assets.

## Visual Options

### OPTION 1 — Windows Native / Fluent

Artifact:  
`05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.png`

Идея:  
Спокойная Windows-native утилита: светлая нейтральная база, мягкие поверхности, системная типографика, один явный синий primary action, знакомые Word/Excel associations, минимальная декоративность.

Почему подходит:
- продукт Windows-only;
- используется редко и должен быть понятен без обучения;
- UX уже сводится к одному окну и одному главному действию;
- визуальная привычность важнее «брендовости».

Главный риск:
- можно случайно превратить интерфейс в слишком большой «информационный плакат». При реализации нужно сохранить компактность и не добавлять вторичные блоки без UX-необходимости.

### OPTION 2 — Compact Utility

Artifact:  
`wide_ui_graphic_design_comparison_poster_clean_mo.png` — центральная колонка OPTION 2.

Идея:  
Более плотный профессиональный desktop-tool, меньше whitespace, source/destination расположены ближе друг к другу.

Плюсы:
- высокая информационная плотность;
- эффективен для частого пользователя.

Минусы:
- выглядит технически сложнее;
- хуже подходит пользователю, которому нужна одна простая операция.

Решение:
НЕ ВЫБРАН.

### OPTION 3 — Focused Drop Zone

Artifact:  
`wide_ui_graphic_design_comparison_poster_clean_mo.png` — правая колонка OPTION 3.

Идея:  
Большая центральная drop-zone, много свободного пространства, крупные статусы.

Плюсы:
- очень прост;
- хорошо читается при редком использовании.

Минусы:
- менее нативно воспринимается как Windows utility;
- при развитии warnings/details может стать слишком «плакатным».

Решение:
СНАЧАЛА ПОНРАВИЛСЯ ПОЛЬЗОВАТЕЛЮ, ЗАТЕМ ЗАМЕНЁН НА OPTION 1.

## User Selection

SELECTED_OPTION: OPTION 1 — Windows Native / Fluent

Хронология выбора:
1. Пользователь сначала выбрал OPTION 3.
2. После дополнительного визуального прохода пользователь переключил выбор на OPTION 1.
3. Итоговый OPTION 1 mockup был показан отдельно.
4. Пользователь явно подтвердил его как final visual target.

## Visual Target

VISUAL_TARGET_ID: `VT-05-001-WINDOWS-NATIVE`

SELECTED_OPTION: `OPTION 1`

SOURCE_ARTIFACT: `05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.png`

SOURCE_LOCATION: `/mnt/data/05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.png`

SUPPORTING_ARTIFACTS:
- `/mnt/data/wide_ui_graphic_design_comparison_poster_clean_mo.png`
- `/mnt/data/варианты_интерфейса_конвертера_word_в_excel.png`

KEY_SURFACES:
- одно главное окно;
- системный file picker;
- раскрываемые details для warnings.

KEY_STATES:
- initial / файл не выбран;
- file selected;
- processing;
- success;
- success with warnings;
- error.

RESPONSIVE_TARGETS:
- Windows desktop — основной target;
- узкое окно должно сохранять primary action, status и имя файла;
- отдельный mobile layout не требуется.

## Visual Language

### Hierarchy / Composition

Главная последовательность чтения:

1. `Word → Excel`
2. зона выбора / выбранный файл;
3. основная кнопка `Преобразовать в Excel`;
4. статус процесса или результата;
5. secondary actions;
6. вспомогательный текст.

Primary action всегда визуально сильнее остальных действий.

В окне не должно быть permanent navigation, dashboard-layout или нескольких равноправных карточек.

### Typography

Направление:
- системная Windows / Segoe-подобная типографика;
- sentence case;
- обычный текст — Regular;
- заголовки и primary labels — Semibold;
- длинные имена файлов допускают ellipsis/reflow, но полное имя остаётся доступным.

Не использовать декоративные display-fonts.

### Color

Основная логика:
- светлая нейтральная Windows-поверхность;
- синий — основной interactive accent;
- зелёный — success;
- жёлтый/amber — warning;
- красный — error;
- смысл всегда дублируется текстом и icon/state treatment.

В target не используется gradient как самостоятельный декоративный эффект.

Dark mode не является обязательным visual target MVP. Возможность системной темы может быть проверена позже, но её отсутствие не меняет текущий target.

### Spacing / Density

Направление: balanced / calm.

- больше воздуха вокруг главного действия;
- внутри file/result blocks — умеренная плотность;
- минимальная дистанция между связанными элементами;
- secondary help не должен конкурировать с conversion flow.

### Component Language

Нужны только MVP-компоненты:
- title bar;
- drag & drop area;
- primary button;
- secondary text button/link;
- selected-file block;
- result block;
- inline success/warning/error status;
- expandable warning details;
- file/folder/status icons;
- disabled / hover / focus / pressed states.

Не нужен enterprise component catalog.

### Selected-file state

Файл показывается отдельным спокойным блоком:
- Word icon;
- filename;
- путь вторичным текстом;
- control для замены/очистки;
- primary conversion button ниже.

### Processing state

- сохраняется та же композиция окна;
- primary content заменяется на ясный processing-status;
- допускается спокойный indeterminate indicator;
- не показывается выдуманный процент;
- текст `Исходный Word-файл не изменяется` остаётся вторичным.

### Success state

- сильный success icon + `Готово`;
- фактическое имя `.xlsx`;
- путь/расположение;
- primary/secondary actions:
  - `Открыть папку`;
  - `Преобразовать другой файл`.

### Warning state

- amber warning icon + `Готово с предупреждениями`;
- Excel-result остаётся явно доступным;
- краткое объяснение;
- `Показать детали`;
- warning не выглядит как полный failure.

### Error state

- red error icon + `Не удалось преобразовать`;
- понятная причина обычным языком;
- исходник не изменён;
- recovery action `Выбрать другой файл`.

## Imagery / Iconography / Motion

### Imagery

Иллюстрации не нужны.

Word/Excel associations используются только как небольшие функциональные file-type cues, без копирования чужого брендинга как центрального декоративного элемента.

### Iconography

Стиль:
- simple Fluent-like line/filled icons;
- единый stroke/weight;
- icon поддерживает текст, а не заменяет его.

### Motion

Минимально:
- hover/pressed/focus feedback;
- спокойный processing indicator;
- короткие state transitions допустимы.

Motion не является обязательным носителем статуса. Reduced motion не должен ломать понимание интерфейса.

## Accessibility / Responsive Notes

- Статусы `Готово / С предупреждениями / Ошибка` всегда имеют текст.
- Контраст primary action и текста должен быть достаточным.
- Focus state должен быть явно различим.
- Drag & drop не является единственным входом.
- Primary button доступна keyboard.
- При увеличении системного text scaling главная кнопка и статус не исчезают.
- Длинное имя файла не ломает layout.
- Warning/error state не определяется только цветом.
- Узкое окно может переносить вторичный контент ниже, но не скрывает primary action.
- Отдельный mobile/tablet target не нужен.

## Technical Questions For Phase 06

1. Какой UI stack может воспроизвести подтверждённый Windows-native target без потери portable-требования?
2. Можно ли использовать Mica/Fluent materials без обязательной установки дополнительных runtime-компонентов пользователем?
3. Какие версии Windows реально смогут поддерживать выбранный visual language?
4. Какой системный шрифт/fallback использовать без включения font-файлов в дистрибутив?
5. Какой источник Fluent-like icons имеет подходящую лицензию и не требует proprietary asset copying?
6. Как реализовать system file picker, drag & drop, keyboard focus и high-DPI/text scaling в выбранном техническом стеке?
7. Как обеспечить success/warning/error states и expandable details без усложнения portable-пакета?
8. Нужна ли поддержка system dark theme технически, если она не является требованием MVP?

Эти вопросы НЕ выбирают stack; они передаются owning technical phase.

## Подтверждено / Наблюдено

### ПОДТВЕРЖДЕНО

- Пользователь выбрал OPTION 1 — Windows Native / Fluent.
- Пользователь подтвердил отдельный OPTION 1 mockup как final visual target.
- UX из Фазы 04 не меняется.
- Один main window остаётся основой продукта.
- Главный flow: выбрать Word → преобразовать → увидеть status/result.
- Visual fidelity Word не является целью.
- OCR/batch не должны появляться в MVP UI.

### НАБЛЮДЕНО

- Microsoft описывает Windows 11 visual direction как calm, familiar, coherent и focused.
- Microsoft рекомендует использовать color для hierarchy и emphasis, а не как единственный смысловой носитель.
- Windows typography guidance использует Segoe UI Variable, Regular/Semibold и sentence case.
- Windows command guidance поддерживает размещение часто нужного primary action непосредственно на рабочей поверхности.
- Windows navigation guidance подчёркивает simplicity и clarity; для нашего single-purpose utility отдельная постоянная navigation structure не нужна.

## Гипотезы / Неизвестно / Не проверено

### ГИПОТЕЗЫ

- Нативный Windows-подобный target будет восприниматься понятнее, чем более кастомный converter UI.
- Синяя primary action будет достаточно заметной без дополнительной декоративности.
- Right-side explanatory panel из generated target при реальной реализации может быть сокращён или убран, если он ухудшает компактность; UX-функцию он не несёт.

### НЕИЗВЕСТНО

- точный минимальный размер окна;
- необходимость dark mode;
- точный набор icons;
- точный системный backdrop;
- конкретный visual behavior на старых версиях Windows.

### НЕ ПРОВЕРЕНО

- реальное восприятие target пользователями кроме текущего выбора;
- contrast/focus values в production-render;
- text scaling на реальном implementation;
- pixel-level соответствие target будущей реализации.

## Проверенные источники

- Microsoft — Windows 11 design principles:
  https://learn.microsoft.com/en-us/windows/apps/design/design-principles

- Microsoft — Typography in Windows:
  https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography

- Microsoft — Color in Windows:
  https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/color

- Microsoft — Commanding basics:
  https://learn.microsoft.com/en-us/windows/apps/design/basics/commanding-basics

- Microsoft — Navigation basics:
  https://learn.microsoft.com/en-us/windows/apps/design/basics/navigation-basics

- Microsoft — Windows app design overview:
  https://learn.microsoft.com/en-us/windows/apps/design/

## Handoff

NEXT_PHASE: 06_TECHNICAL_PLAN  
RETURN_TO_PHASE: NONE

Передать в Фазу 06:
- `01_PROJECT_INTENT.md`
- `02_MVP_SPEC.md`
- `03_PROJECT_RULES.md`
- `04_PRODUCT_UX_DESIGN.md`
- `05_VISUAL_UI_DESIGN.md`
- `05_VISUAL_TARGET_OPTION1_WINDOWS_NATIVE.png`
- comparison/supporting mockups
- visual references
- reuse candidates
- technical questions из этой фазы

PHASE_05_COMPLETE
