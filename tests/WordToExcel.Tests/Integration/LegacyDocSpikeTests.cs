using System.Runtime.CompilerServices;
using DocSharp.Binary.DocFileFormat;
using DocSharp.Binary.OpenXmlLib;
using DocSharp.Binary.OpenXmlLib.WordprocessingML;
using DocSharp.Binary.StructuredStorage.Reader;
using DocSharp.Binary.WordprocessingMLMapping;
using WordToExcel.App.Model;
using WordToExcel.App.Word;
using Xunit;

namespace WordToExcel.Tests.Integration;

public sealed class LegacyDocSpikeTests
{
    private static readonly string FixtureDirectory = Path.GetFullPath(
        Path.Combine(Path.GetDirectoryName(ThisSourceFile())!, "..", "Fixtures", "LegacyDoc"));

    [Fact]
    public void SimpleTable_RoundTripsExpectedCells()
    {
        var model = ConvertAndRead("simple-table.doc");
        Assert.Equal(new[] { "A", "B", "C", "D" }, CellTexts(model));
    }

    [Fact]
    public void MultipleTables_AreBothRecovered()
    {
        var model = ConvertAndRead("multiple-tables.doc");
        Assert.Equal(2, model.Tables.Count);
        Assert.Contains("T1A", CellTexts(model));
        Assert.Contains("T2A", CellTexts(model));
    }

    [Fact]
    public void MergedCells_PreserveAnchorAndUsableSpanHint()
    {
        var model = ConvertAndRead("merged-cells.doc");
        Assert.Contains("MERGED", CellTexts(model));
        Assert.Contains(model.Tables.SelectMany(t => t.Rows).SelectMany(r => r.Cells), cell => cell.GridSpan > 1);
    }

    [Fact]
    public void IrregularRows_PreserveExpectedAnchorsAndSpanHints()
    {
        var model = ConvertAndRead("irregular-rows.doc");
        var texts = CellTexts(model);
        Assert.Contains("WIDE", texts);
        Assert.Contains("R1C0", texts);
        Assert.Contains("R1C1", texts);
        Assert.Contains("R1C2", texts);
        Assert.Contains("ALL", texts);
        Assert.Contains(model.Tables.SelectMany(t => t.Rows).SelectMany(r => r.Cells), cell => cell.GridSpan > 1);
    }

    [Fact]
    public void UnicodeCyrillic_IsExact()
    {
        var texts = CellTexts(ConvertAndRead("unicode-cyrillic.doc"));
        Assert.Contains("Привет", texts);
        Assert.Contains("ёж — Москва", texts);
    }

    [Fact]
    public void LeadingZeroCode_IsExact()
    {
        Assert.Contains("001234", CellTexts(ConvertAndRead("leading-zero-codes.doc")));
    }

    [Fact]
    public void LongNumericIdentifier_IsExact()
    {
        Assert.Contains("12345678901234567890", CellTexts(ConvertAndRead("long-numeric-identifiers.doc")));
    }

    [Fact]
    public void SurroundingText_PreservesOrderAndTableValue()
    {
        var model = ConvertAndRead("surrounding-text.doc");
        var paragraphs = model.Blocks.OfType<ParagraphBlock>().Select(p => p.SourceTextExact).ToArray();
        Assert.Contains("Текст до таблицы", paragraphs);
        Assert.Contains("Текст после таблицы", paragraphs);
        Assert.Contains("ЦЕНТР", CellTexts(model));
    }

    [Fact]
    public void NestedTable_IsRecoveredSeparately()
    {
        var model = ConvertAndRead("nested-table.doc");
        Assert.True(model.Tables.Count >= 2);
        Assert.Contains("Родитель", CellTexts(model));
        Assert.Contains("Вложенные данные", CellTexts(model));
        Assert.Contains(model.Tables, table => table.ParentTableId is not null);
    }

    [Fact]
    public void EmbeddedImage_BecomesExplicitUnsupportedObjectFinding()
    {
        var model = ConvertAndRead("embedded-image.doc");
        Assert.NotEmpty(model.UnsupportedObjects);
        Assert.Contains(model.Findings, finding => finding.Code == "UNSUPPORTED_OBJECT");
    }

    [Fact]
    public void ProtectedDocument_IsDetectableBeforeConversion()
    {
        var path = Fixture("protected.doc");
        using var reader = new StructuredStorageReader(path);
        var document = new WordDocument(reader);
        Assert.True(document.FIB.fEncrypted);
    }

    [Fact]
    public void DamagedDocument_IsRejected()
    {
        var path = Fixture("damaged.doc");
        Assert.ThrowsAny<Exception>(() =>
        {
            using var reader = new StructuredStorageReader(path);
            _ = new WordDocument(reader);
        });
    }

    private static DocumentModel ConvertAndRead(string fixtureName)
    {
        var inputPath = Fixture(fixtureName);
        var outputPath = Path.Combine(Path.GetTempPath(), $"legacy-doc-spike-{Guid.NewGuid():N}.docx");

        try
        {
            using (var reader = new StructuredStorageReader(inputPath))
            {
                var document = new WordDocument(reader);
                Assert.False(document.FIB.fEncrypted);

                using var output = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
                Converter.Convert(document, output);
            }

            Assert.True(File.Exists(outputPath));
            return new DocxDocumentReader().Read(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    private static string[] CellTexts(DocumentModel model) =>
        model.Tables
            .SelectMany(table => table.Rows)
            .SelectMany(row => row.Cells)
            .Select(cell => cell.SourceTextExact)
            .ToArray();

    private static string Fixture(string name)
    {
        var path = Path.Combine(FixtureDirectory, name);
        Assert.True(File.Exists(path), $"Missing legacy spike fixture: {path}");
        return path;
    }

    private static string ThisSourceFile([CallerFilePath] string path = "") => path;
}
