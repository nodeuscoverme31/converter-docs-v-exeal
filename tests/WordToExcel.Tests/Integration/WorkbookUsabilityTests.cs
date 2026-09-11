using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using WordToExcel.App.Excel;
using WordToExcel.App.Model;
using Xunit;

namespace WordToExcel.Tests.Integration;

public sealed class WorkbookUsabilityTests
{
    [Fact]
    public void TableSheet_HasReadableColumnsWrappedCellsBoldFirstRowAndFrozenHeader()
    {
        var source = new SourceLocation("word/document.xml", 0);
        var document = new DocumentModel(
            "fixture.docx",
            new DocumentBlock[] { new TableReferenceBlock(0, "Table-001", source) },
            Array.Empty<TableModel>(),
            Array.Empty<UnsupportedObjectRecord>(),
            Array.Empty<ConversionFinding>());
        var table = new NormalizedTable(
            "Table-001",
            2,
            2,
            new NormalizedCell[]
            {
                Cell("H1", 0, 0, "Очень длинный заголовок столбца", ValueMode.Text, "Очень длинный заголовок столбца"),
                Cell("H2", 0, 1, "Комментарий", ValueMode.Text, "Комментарий"),
                Cell("D1", 1, 0, "Значение", ValueMode.Text, "Значение"),
                Cell("D2", 1, 1, "Длинный текст, который должен переноситься внутри ячейки и не растягивать лист бесконечно", ValueMode.Text, "Длинный текст, который должен переноситься внутри ячейки и не растягивать лист бесконечно"),
            },
            Array.Empty<ConversionFinding>());
        var path = TempXlsx();

        try
        {
            new ExcelWorkbookWriter().Write(document, new[] { table }, path);

            using var package = SpreadsheetDocument.Open(path, false);
            var workbookPart = package.WorkbookPart!;
            var sheet = workbookPart.Workbook.Sheets!.Elements<Sheet>().First();
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);

            var columns = worksheetPart.Worksheet.Elements<Columns>().SingleOrDefault();
            Assert.NotNull(columns);
            Assert.Contains(columns!.Elements<Column>(), column => column.CustomWidth?.Value == true && column.Width?.Value > 15D);
            Assert.All(columns.Elements<Column>(), column => Assert.True((column.Width?.Value ?? 0D) <= 45D));

            var sheetView = worksheetPart.Worksheet.SheetViews!.Elements<SheetView>().Single();
            Assert.NotNull(sheetView.Pane);
            Assert.Equal(PaneStateValues.Frozen, sheetView.Pane!.State?.Value);
            Assert.Equal(1D, sheetView.Pane.VerticalSplit?.Value);
            Assert.Equal("A2", sheetView.Pane.TopLeftCell?.Value);

            var headerFormat = GetCellFormat(workbookPart, FindCell(worksheetPart, "A1"));
            var headerFont = GetFont(workbookPart, headerFormat);
            Assert.NotNull(headerFont.Bold);
            Assert.True(headerFormat.Alignment?.WrapText?.Value == true);

            var bodyFormat = GetCellFormat(workbookPart, FindCell(worksheetPart, "B2"));
            Assert.True(bodyFormat.Alignment?.WrapText?.Value == true);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SafeDecimal_IsSerializedAsNumberWithSourcePrecisionFormat()
    {
        var source = new SourceLocation("word/document.xml", 0);
        var document = new DocumentModel(
            "fixture.docx",
            new DocumentBlock[] { new TableReferenceBlock(0, "Table-001", source) },
            Array.Empty<TableModel>(),
            Array.Empty<UnsupportedObjectRecord>(),
            Array.Empty<ConversionFinding>());
        var table = new NormalizedTable(
            "Table-001",
            1,
            1,
            new[] { Cell("D1", 0, 0, "799.90", ValueMode.SafeNumber, 799.90m, "0.00") },
            Array.Empty<ConversionFinding>());
        var path = TempXlsx();

        try
        {
            new ExcelWorkbookWriter().Write(document, new[] { table }, path);

            using var package = SpreadsheetDocument.Open(path, false);
            var workbookPart = package.WorkbookPart!;
            var sheet = workbookPart.Workbook.Sheets!.Elements<Sheet>().First();
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var cell = FindCell(worksheetPart, "A1");

            Assert.Null(cell.CellFormula);
            Assert.True(double.TryParse(cell.CellValue?.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value));
            Assert.Equal(799.90D, value, 10);

            var format = GetCellFormat(workbookPart, cell);
            Assert.NotEqual(0U, format.NumberFormatId?.Value ?? 0U);
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

    private static Cell FindCell(WorksheetPart worksheetPart, string reference) =>
        worksheetPart.Worksheet.Descendants<Cell>().Single(cell => cell.CellReference?.Value == reference);

    private static CellFormat GetCellFormat(WorkbookPart workbookPart, Cell cell)
    {
        var formats = workbookPart.WorkbookStylesPart!.Stylesheet.CellFormats!.Elements<CellFormat>().ToArray();
        return formats[(int)(cell.StyleIndex?.Value ?? 0U)];
    }

    private static Font GetFont(WorkbookPart workbookPart, CellFormat format)
    {
        var fonts = workbookPart.WorkbookStylesPart!.Stylesheet.Fonts!.Elements<Font>().ToArray();
        return fonts[(int)(format.FontId?.Value ?? 0U)];
    }

    private static string TempXlsx() => Path.Combine(Path.GetTempPath(), $"writer-usability-{Guid.NewGuid():N}.xlsx");
}
