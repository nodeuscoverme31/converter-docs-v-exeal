using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using WordToExcel.App.Excel;
using WordToExcel.App.Model;
using WordToExcel.App.Validation;
using Xunit;

namespace WordToExcel.Tests.Integration;

public sealed class XlsxWriteReadbackTests
{
    [Fact]
    public void Writer_CreatesSeparateTableSheetsAndOrderedContext_WithoutAccidentalFormula()
    {
        var source = new SourceLocation("word/document.xml", 0);
        var document = new DocumentModel(
            "fixture.docx",
            new DocumentBlock[]
            {
                new ParagraphBlock(0, "До таблицы", source),
                new TableReferenceBlock(1, "Table-001", source),
                new ParagraphBlock(2, "После таблицы", source),
            },
            Array.Empty<TableModel>(),
            Array.Empty<UnsupportedObjectRecord>(),
            Array.Empty<ConversionFinding>());

        var tables = new[]
        {
            new NormalizedTable(
                "Table-001",
                3,
                2,
                new NormalizedCell[]
                {
                    Cell("C1", 0, 0, "001234", ValueMode.Text, "001234"),
                    Cell("C2", 0, 1, "42", ValueMode.SafeNumber, 42L),
                    Cell("C3", 0, 2, "=1+1", ValueMode.Text, "=1+1"),
                    Cell("C4", 1, 0, "ёж — Москва", ValueMode.Text, "ёж — Москва"),
                    Cell("C5", 1, 1, "2026-09-11", ValueMode.SafeDate, new DateTime(2026, 9, 11), "yyyy-mm-dd"),
                    Cell("C6", 1, 2, "12345678901234567890", ValueMode.Text, "12345678901234567890"),
                },
                Array.Empty<ConversionFinding>()),
            new NormalizedTable(
                "Table-002",
                1,
                1,
                new[] { Cell("D1", 0, 0, "Вторая", ValueMode.Text, "Вторая") },
                Array.Empty<ConversionFinding>()),
        };

        var path = TempXlsx();
        try
        {
            new ExcelWorkbookWriter().Write(document, tables, path);

            using var package = SpreadsheetDocument.Open(path, false);
            var sheets = package.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().ToArray();
            Assert.Equal(new[] { "Таблица 1", "Таблица 2", "Контекст" }, sheets.Select(s => s.Name!.Value));

            var firstWorksheet = (WorksheetPart)package.WorkbookPart.GetPartById(sheets[0].Id!);
            var formulaCell = FindCell(firstWorksheet, "C1");
            Assert.Null(formulaCell.CellFormula);
            Assert.Equal("=1+1", ReadCellText(package.WorkbookPart, formulaCell));

            var contextWorksheet = (WorksheetPart)package.WorkbookPart.GetPartById(sheets[2].Id!);
            Assert.Equal("Порядок", ReadCellText(package.WorkbookPart, FindCell(contextWorksheet, "A1")));
            Assert.Equal("Тип", ReadCellText(package.WorkbookPart, FindCell(contextWorksheet, "B1")));
            Assert.Equal("Содержимое", ReadCellText(package.WorkbookPart, FindCell(contextWorksheet, "C1")));
            Assert.Equal("До таблицы", ReadCellText(package.WorkbookPart, FindCell(contextWorksheet, "C2")));
            Assert.Equal("Таблица 1", ReadCellText(package.WorkbookPart, FindCell(contextWorksheet, "C3")));
            Assert.Equal("После таблицы", ReadCellText(package.WorkbookPart, FindCell(contextWorksheet, "C4")));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Validator_PassesExactSerializedWorkbook_AndRejectsTamperedValue()
    {
        var source = new SourceLocation("word/document.xml", 0);
        var document = new DocumentModel(
            "fixture.docx",
            new DocumentBlock[] { new ParagraphBlock(0, "Контекст", source) },
            Array.Empty<TableModel>(),
            Array.Empty<UnsupportedObjectRecord>(),
            Array.Empty<ConversionFinding>());
        var table = new NormalizedTable(
            "Table-001",
            2,
            1,
            new NormalizedCell[]
            {
                Cell("C1", 0, 0, "001234", ValueMode.Text, "001234"),
                Cell("C2", 0, 1, "42", ValueMode.SafeNumber, 42L),
            },
            Array.Empty<ConversionFinding>());
        var tables = new[] { table };
        var path = TempXlsx();

        try
        {
            new ExcelWorkbookWriter().Write(document, tables, path);
            var validator = new OpenXmlOutputValidator();

            var good = validator.Validate(path, document, tables);
            Assert.True(good.IsValid, string.Join(Environment.NewLine, good.Findings.Select(f => $"{f.Code}: {f.Message}")));

            using (var workbook = SpreadsheetDocument.Open(path, true))
            {
                var sheet = workbook.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().First();
                var worksheet = (WorksheetPart)workbook.WorkbookPart.GetPartById(sheet.Id!);
                var cell = FindCell(worksheet, "A1");
                cell.DataType = CellValues.InlineString;
                cell.CellValue = null;
                cell.InlineString = new InlineString(new Text("999999"));
                worksheet.Worksheet.Save();
            }

            var bad = validator.Validate(path, document, tables);
            Assert.False(bad.IsValid);
            Assert.Contains(bad.Findings, finding => finding.Code == "CELL_VALUE_MISMATCH");
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static NormalizedCell Cell(
        string id,
        int row,
        int column,
        string source,
        ValueMode mode,
        object value,
        string? numberFormat = null) =>
        new(id, row, column, source, new OutputValuePlan(mode, source, value, numberFormat));

    private static string TempXlsx() => Path.Combine(Path.GetTempPath(), $"writer-{Guid.NewGuid():N}.xlsx");

    private static Cell FindCell(WorksheetPart worksheetPart, string reference) =>
        worksheetPart.Worksheet.Descendants<Cell>().Single(cell => cell.CellReference?.Value == reference);

    private static string ReadCellText(WorkbookPart workbookPart, Cell cell)
    {
        if (cell.DataType?.Value == CellValues.InlineString)
        {
            return cell.InlineString?.Text?.Text ?? cell.InlineString?.InnerText ?? string.Empty;
        }

        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var index = int.Parse(cell.CellValue!.Text, System.Globalization.CultureInfo.InvariantCulture);
            return workbookPart.SharedStringTablePart!.SharedStringTable!.Elements<SharedStringItem>().ElementAt(index).InnerText;
        }

        return cell.CellValue?.Text ?? string.Empty;
    }
}
