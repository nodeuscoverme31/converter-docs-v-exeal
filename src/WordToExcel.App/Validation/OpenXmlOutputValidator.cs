using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using WordToExcel.App.Excel;
using WordToExcel.App.Model;

namespace WordToExcel.App.Validation;

internal sealed class OpenXmlOutputValidator : IOutputValidator
{
    public OutputValidationResult Validate(
        string xlsxPath,
        DocumentModel expectedDocument,
        IReadOnlyList<NormalizedTable> expectedTables)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xlsxPath);
        ArgumentNullException.ThrowIfNull(expectedDocument);
        ArgumentNullException.ThrowIfNull(expectedTables);

        var findings = new List<OutputValidationFinding>();

        try
        {
            using var package = SpreadsheetDocument.Open(xlsxPath, false);
            var workbookPart = package.WorkbookPart;
            var sheets = workbookPart?.Workbook.Sheets?.Elements<Sheet>().ToArray();
            if (workbookPart is null || sheets is null)
            {
                return Failure("WORKBOOK_STRUCTURE_MISSING", "Workbook or sheet collection is missing.");
            }

            ValidateSheets(workbookPart, sheets, expectedDocument, expectedTables, findings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OpenXmlPackageException or InvalidDataException)
        {
            findings.Add(new OutputValidationFinding("OUTPUT_NOT_READABLE", ex.Message));
        }

        return new OutputValidationResult(findings.Count == 0, findings);
    }

    private static void ValidateSheets(
        WorkbookPart workbookPart,
        IReadOnlyList<Sheet> sheets,
        DocumentModel expectedDocument,
        IReadOnlyList<NormalizedTable> expectedTables,
        ICollection<OutputValidationFinding> findings)
    {
        var expectedNames = expectedTables
            .Select((_, index) => ExcelWorkbookWriter.TableSheetName(index))
            .Append("Контекст")
            .ToArray();
        var actualNames = sheets.Select(sheet => sheet.Name?.Value ?? string.Empty).ToArray();

        if (!expectedNames.SequenceEqual(actualNames, StringComparer.Ordinal))
        {
            findings.Add(new OutputValidationFinding(
                "SHEET_LAYOUT_MISMATCH",
                $"Expected sheets [{string.Join(", ", expectedNames)}], got [{string.Join(", ", actualNames)}]."));
        }

        for (var index = 0; index < expectedTables.Count; index++)
        {
            var sheetName = ExcelWorkbookWriter.TableSheetName(index);
            var sheet = sheets.FirstOrDefault(candidate => string.Equals(candidate.Name?.Value, sheetName, StringComparison.Ordinal));
            if (sheet is null)
            {
                continue;
            }

            var worksheetPart = GetWorksheetPart(workbookPart, sheet, findings);
            if (worksheetPart is null)
            {
                continue;
            }

            ValidateTable(workbookPart, worksheetPart, expectedTables[index], sheetName, findings);
        }

        var contextSheet = sheets.FirstOrDefault(sheet => string.Equals(sheet.Name?.Value, "Контекст", StringComparison.Ordinal));
        if (contextSheet is not null)
        {
            var contextPart = GetWorksheetPart(workbookPart, contextSheet, findings);
            if (contextPart is not null)
            {
                ValidateContext(workbookPart, contextPart, expectedDocument, expectedTables, findings);
            }
        }
    }

    private static WorksheetPart? GetWorksheetPart(
        WorkbookPart workbookPart,
        Sheet sheet,
        ICollection<OutputValidationFinding> findings)
    {
        if (sheet.Id?.Value is not string relationId ||
            !workbookPart.TryGetPartById(relationId, out var part) ||
            part is not WorksheetPart worksheetPart)
        {
            findings.Add(new OutputValidationFinding(
                "WORKSHEET_PART_MISSING",
                $"Worksheet part for '{sheet.Name?.Value}' is missing."));
            return null;
        }

        return worksheetPart;
    }

    private static void ValidateTable(
        WorkbookPart workbookPart,
        WorksheetPart worksheetPart,
        NormalizedTable expectedTable,
        string sheetName,
        ICollection<OutputValidationFinding> findings)
    {
        var cells = worksheetPart.Worksheet.Descendants<Cell>()
            .Where(cell => cell.CellReference?.Value is not null)
            .ToDictionary(cell => cell.CellReference!.Value!, StringComparer.OrdinalIgnoreCase);

        foreach (var expectedCell in expectedTable.Cells)
        {
            var plan = expectedCell.OutputValuePlan;
            if (plan is null && expectedCell.SourceCellId is null && expectedCell.SourceTextExact.Length == 0)
            {
                continue;
            }

            plan ??= new OutputValuePlan(ValueMode.Text, expectedCell.SourceTextExact, expectedCell.SourceTextExact);
            var reference = CellReference(expectedCell.Row, expectedCell.Column);

            if (!cells.TryGetValue(reference, out var actualCell))
            {
                findings.Add(new OutputValidationFinding(
                    "CELL_VALUE_MISMATCH",
                    $"{sheetName}!{reference} is missing; expected '{plan.ExpectedSourceText}'."));
                continue;
            }

            if (actualCell.CellFormula is not null)
            {
                findings.Add(new OutputValidationFinding(
                    "UNEXPECTED_FORMULA",
                    $"{sheetName}!{reference} contains a formula although source data is literal."));
                continue;
            }

            if (!CellMatches(workbookPart, actualCell, plan))
            {
                findings.Add(new OutputValidationFinding(
                    "CELL_VALUE_MISMATCH",
                    $"{sheetName}!{reference} does not preserve expected source value '{plan.ExpectedSourceText}'."));
            }
        }
    }

    private static bool CellMatches(WorkbookPart workbookPart, Cell cell, OutputValuePlan plan)
    {
        switch (plan.Mode)
        {
            case ValueMode.Text:
                return string.Equals(ReadText(workbookPart, cell), plan.ExpectedSourceText, StringComparison.Ordinal);

            case ValueMode.SafeNumber:
                return double.TryParse(cell.CellValue?.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var actualNumber) &&
                       Math.Abs(actualNumber - Convert.ToDouble(plan.ExcelValue, CultureInfo.InvariantCulture)) < double.Epsilon;

            case ValueMode.SafeDate:
                if (plan.ExcelValue is not DateTime expectedDate ||
                    !double.TryParse(cell.CellValue?.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var oaDate))
                {
                    return false;
                }

                var actualDate = DateTime.FromOADate(oaDate).Date;
                return actualDate == expectedDate.Date;

            default:
                return false;
        }
    }

    private static void ValidateContext(
        WorkbookPart workbookPart,
        WorksheetPart worksheetPart,
        DocumentModel expectedDocument,
        IReadOnlyList<NormalizedTable> expectedTables,
        ICollection<OutputValidationFinding> findings)
    {
        var expectedRows = new List<(string Order, string Type, string Content)>
        {
            ("Порядок", "Тип", "Содержимое"),
        };

        var tableNames = expectedTables
            .Select((table, index) => (table.TableId, Name: ExcelWorkbookWriter.TableSheetName(index)))
            .ToDictionary(pair => pair.TableId, pair => pair.Name, StringComparer.Ordinal);

        foreach (var block in expectedDocument.Blocks.OrderBy(block => block.SourceOrder))
        {
            expectedRows.Add(block switch
            {
                ParagraphBlock paragraph => (block.SourceOrder.ToString(CultureInfo.InvariantCulture), "Текст", paragraph.SourceTextExact),
                TableReferenceBlock table => (block.SourceOrder.ToString(CultureInfo.InvariantCulture), "Таблица", tableNames.TryGetValue(table.TableId, out var name) ? name : table.TableId),
                UnsupportedObjectBlock unsupported => (block.SourceOrder.ToString(CultureInfo.InvariantCulture), "Неподдерживаемый объект", unsupported.ObjectType),
                _ => throw new InvalidDataException($"Unsupported document block type: {block.GetType().Name}."),
            });
        }

        for (var rowIndex = 0; rowIndex < expectedRows.Count; rowIndex++)
        {
            var row = expectedRows[rowIndex];
            ValidateContextCell(workbookPart, worksheetPart, rowIndex, 0, row.Order, findings);
            ValidateContextCell(workbookPart, worksheetPart, rowIndex, 1, row.Type, findings);
            ValidateContextCell(workbookPart, worksheetPart, rowIndex, 2, row.Content, findings);
        }
    }

    private static void ValidateContextCell(
        WorkbookPart workbookPart,
        WorksheetPart worksheetPart,
        int zeroBasedRow,
        int zeroBasedColumn,
        string expected,
        ICollection<OutputValidationFinding> findings)
    {
        var reference = CellReference(zeroBasedRow, zeroBasedColumn);
        var cell = worksheetPart.Worksheet.Descendants<Cell>()
            .FirstOrDefault(candidate => string.Equals(candidate.CellReference?.Value, reference, StringComparison.OrdinalIgnoreCase));

        if (cell is null)
        {
            findings.Add(new OutputValidationFinding("CONTEXT_MISMATCH", $"Контекст!{reference} is missing."));
            return;
        }

        var actual = cell.DataType?.Value is CellValues.Number or null
            ? cell.CellValue?.Text ?? string.Empty
            : ReadText(workbookPart, cell);

        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            findings.Add(new OutputValidationFinding(
                "CONTEXT_MISMATCH",
                $"Контекст!{reference} expected '{expected}', got '{actual}'."));
        }
    }

    private static string ReadText(WorkbookPart workbookPart, Cell cell)
    {
        if (cell.DataType?.Value == CellValues.SharedString &&
            int.TryParse(cell.CellValue?.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            return workbookPart.SharedStringTablePart?.SharedStringTable?.Elements<SharedStringItem>().ElementAtOrDefault(index)?.InnerText
                ?? string.Empty;
        }

        if (cell.DataType?.Value == CellValues.InlineString)
        {
            return cell.InlineString?.InnerText ?? string.Empty;
        }

        if (cell.DataType?.Value == CellValues.String)
        {
            return cell.CellValue?.Text ?? string.Empty;
        }

        return cell.CellValue?.Text ?? string.Empty;
    }

    private static string CellReference(int zeroBasedRow, int zeroBasedColumn)
    {
        var column = zeroBasedColumn + 1;
        Span<char> buffer = stackalloc char[8];
        var position = buffer.Length;

        while (column > 0)
        {
            column--;
            buffer[--position] = (char)('A' + (column % 26));
            column /= 26;
        }

        return $"{new string(buffer[position..])}{zeroBasedRow + 1}";
    }

    private static OutputValidationResult Failure(string code, string message) =>
        new(false, new[] { new OutputValidationFinding(code, message) });
}
