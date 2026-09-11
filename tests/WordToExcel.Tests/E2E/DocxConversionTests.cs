using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using WordToExcel.App.Conversion;
using WordToExcel.App.Excel;
using WordToExcel.App.Model;
using WordToExcel.App.Validation;
using WordToExcel.App.Word;
using WordToExcel.Tests.Fixtures;
using Xunit;

namespace WordToExcel.Tests.E2E;

public sealed class DocxConversionTests
{
    [Fact]
    public void CleanDocxThroughCanonicalizedPathPublishesValidatedWorkbookWithoutChangingSource()
    {
        using var fixture = DocxFixtureFactory.Create(body =>
        {
            body.Append(DocxFixtureFactory.Paragraph("До таблицы"));
            body.Append(DocxFixtureFactory.Table(
                DocxFixtureFactory.Row(
                    DocxFixtureFactory.Cell("001234"),
                    DocxFixtureFactory.Cell("=1+1"),
                    DocxFixtureFactory.Cell("42"))));
            body.Append(DocxFixtureFactory.Paragraph("После таблицы"));
        });
        var before = File.ReadAllBytes(fixture.Path);
        var directory = Path.GetDirectoryName(fixture.Path)!;
        var nonCanonicalPath = Path.Combine(directory, ".", "unused", "..", Path.GetFileName(fixture.Path));

        var result = new ConversionOrchestrator().Convert(nonCanonicalPath);

        Assert.Equal(ConversionStatus.Success, result.Status);
        Assert.NotNull(result.OutputPath);
        Assert.Equal(Path.GetFullPath(result.OutputPath), result.OutputPath);
        Assert.Equal(Path.Combine(directory, "fixture.xlsx"), result.OutputPath);
        Assert.True(File.Exists(result.OutputPath));
        Assert.Equal(before, File.ReadAllBytes(fixture.Path));

        using var workbook = SpreadsheetDocument.Open(result.OutputPath, false);
        var workbookPart = Assert.IsType<WorkbookPart>(workbook.WorkbookPart);
        var sheets = workbookPart.Workbook.Sheets!.Elements<Sheet>().ToArray();
        Assert.Equal(new[] { "Таблица 1", "Контекст" }, sheets.Select(sheet => sheet.Name!.Value).ToArray());

        var tablePart = (WorksheetPart)workbookPart.GetPartById(sheets[0].Id!.Value!);
        Assert.Equal("001234", ReadCellText(workbookPart, tablePart, "A1"));
        Assert.Equal("=1+1", ReadCellText(workbookPart, tablePart, "B1"));
        Assert.Equal("42", ReadCellText(workbookPart, tablePart, "C1"));
        Assert.All(tablePart.Worksheet.Descendants<Cell>(), cell => Assert.Null(cell.CellFormula));
    }

    [Fact]
    public void ValidatorFailureNeverPublishesFinalWorkbook()
    {
        using var fixture = DocxFixtureFactory.Create(body =>
        {
            body.Append(DocxFixtureFactory.Table(
                DocxFixtureFactory.Row(DocxFixtureFactory.Cell("A"))));
        });
        var finalPath = Path.ChangeExtension(fixture.Path, ".xlsx");
        var orchestrator = new ConversionOrchestrator(
            new InputDetector(),
            new DocxDocumentReader(),
            new TableNormalizer(),
            new ValuePolicy(),
            new ExcelWorkbookWriter(),
            new AlwaysFailValidator(),
            new OutputPublisher());

        var result = orchestrator.Convert(fixture.Path);

        Assert.Equal(ConversionStatus.Error, result.Status);
        Assert.Equal(ConversionErrorCategory.Validation, result.ErrorCategory);
        Assert.Null(result.OutputPath);
        Assert.False(File.Exists(finalPath));
    }

    private static string ReadCellText(WorkbookPart workbookPart, WorksheetPart worksheetPart, string reference)
    {
        var cell = worksheetPart.Worksheet.Descendants<Cell>()
            .Single(candidate => string.Equals(candidate.CellReference?.Value, reference, StringComparison.OrdinalIgnoreCase));

        if (cell.DataType?.Value == CellValues.SharedString &&
            int.TryParse(cell.CellValue?.Text, out var index))
        {
            return workbookPart.SharedStringTablePart!.SharedStringTable!
                .Elements<SharedStringItem>()
                .ElementAt(index)
                .InnerText;
        }

        if (cell.DataType?.Value == CellValues.InlineString)
        {
            return cell.InlineString?.InnerText ?? string.Empty;
        }

        return cell.CellValue?.Text ?? string.Empty;
    }

    private sealed class AlwaysFailValidator : IOutputValidator
    {
        public OutputValidationResult Validate(
            string xlsxPath,
            DocumentModel expectedDocument,
            IReadOnlyList<NormalizedTable> expectedTables) =>
            new(false, new[] { new OutputValidationFinding("TEST_FAILURE", "Intentional validation failure.") });
    }
}
