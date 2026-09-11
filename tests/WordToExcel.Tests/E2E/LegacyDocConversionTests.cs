using System.Runtime.CompilerServices;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using WordToExcel.App.Conversion;
using WordToExcel.App.Model;
using WordToExcel.App.Word;
using Xunit;

namespace WordToExcel.Tests.E2E;

public sealed class LegacyDocConversionTests
{
    private static readonly string FixtureDirectory = Path.GetFullPath(
        Path.Combine(Path.GetDirectoryName(ThisSourceFile())!, "..", "Fixtures", "LegacyDoc"));

    [Fact]
    public void ApprovedLegacyDocUsesCommonPipelineAndPreservesSource()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"legacy-e2e-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            var source = Path.Combine(directory, "legacy.doc");
            File.Copy(Fixture("simple-table.doc"), source);
            var before = File.ReadAllBytes(source);
            var nonCanonicalSource = Path.Combine(directory, ".", "unused", "..", "legacy.doc");

            var result = new ConversionOrchestrator().Convert(nonCanonicalSource);

            Assert.Equal(ConversionStatus.Success, result.Status);
            Assert.Equal(Path.Combine(directory, "legacy.xlsx"), result.OutputPath);
            Assert.True(File.Exists(result.OutputPath));
            Assert.Equal(before, File.ReadAllBytes(source));

            using var workbook = SpreadsheetDocument.Open(result.OutputPath!, false);
            var workbookPart = Assert.IsType<WorkbookPart>(workbook.WorkbookPart);
            var sheets = workbookPart.Workbook.Sheets!.Elements<Sheet>().ToArray();
            Assert.Equal(new[] { "Таблица 1", "Контекст" }, sheets.Select(sheet => sheet.Name!.Value).ToArray());
            var tablePart = (WorksheetPart)workbookPart.GetPartById(sheets[0].Id!.Value!);
            Assert.Equal("A", ReadCellText(workbookPart, tablePart, "A1"));
            Assert.Equal("B", ReadCellText(workbookPart, tablePart, "B1"));
            Assert.Equal("C", ReadCellText(workbookPart, tablePart, "A2"));
            Assert.Equal("D", ReadCellText(workbookPart, tablePart, "B2"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void LegacyAdapterRejectsProtectedDocBeforeCreatingDocx()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"legacy-adapter-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            var error = Assert.Throws<LegacyDocConversionException>(() =>
                new LegacyDocConverter().ConvertToDocx(Fixture("protected.doc"), directory));

            Assert.Equal(LegacyDocError.ProtectedDocument, error.Error);
            Assert.Empty(Directory.EnumerateFiles(directory, "*.docx"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
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

    private static string Fixture(string name)
    {
        var path = Path.Combine(FixtureDirectory, name);
        Assert.True(File.Exists(path), $"Missing legacy fixture: {path}");
        return path;
    }

    private static string ThisSourceFile([CallerFilePath] string path = "") => path;
}
