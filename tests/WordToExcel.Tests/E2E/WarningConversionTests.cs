using DocumentFormat.OpenXml.Wordprocessing;
using WordToExcel.App.Conversion;
using WordToExcel.App.Model;
using WordToExcel.Tests.Fixtures;
using Xunit;

namespace WordToExcel.Tests.E2E;

public sealed class WarningConversionTests
{
    [Fact]
    public void UnsupportedObject_ProducesUsableWorkbookWithExplicitWarning()
    {
        using var fixture = DocxFixtureFactory.Create(body =>
        {
            body.Append(DocxFixtureFactory.Table(
                DocxFixtureFactory.Row(DocxFixtureFactory.Cell("Сохранить это"))));
            body.Append(new Paragraph(new Run(new Drawing())));
        });

        var result = new ConversionOrchestrator().Convert(fixture.Path);

        Assert.Equal(ConversionStatus.Warning, result.Status);
        Assert.Contains(result.EffectiveWarnings, finding => finding.Code == "UNSUPPORTED_OBJECT");
        Assert.NotNull(result.OutputPath);
        Assert.True(File.Exists(result.OutputPath));
    }

    [Fact]
    public void AmbiguousVerticalMerge_ProducesWarningWithoutDroppingSourceText()
    {
        using var fixture = DocxFixtureFactory.Create(body =>
        {
            body.Append(DocxFixtureFactory.Table(
                DocxFixtureFactory.Row(
                    DocxFixtureFactory.Cell("ORPHAN", verticalMerge: MergedCellValues.Continue))));
        });

        var result = new ConversionOrchestrator().Convert(fixture.Path);

        Assert.Equal(ConversionStatus.Warning, result.Status);
        Assert.Contains(result.EffectiveWarnings, finding => finding.Code == "TABLE_VERTICAL_MERGE_AMBIGUOUS");
        Assert.NotNull(result.OutputPath);
        Assert.True(File.Exists(result.OutputPath));
    }

    [Fact]
    public void NoTables_IsWarningRatherThanFalseFullSuccess()
    {
        using var fixture = DocxFixtureFactory.Create(body =>
            body.Append(DocxFixtureFactory.Paragraph("Только обычный текст")));

        var result = new ConversionOrchestrator().Convert(fixture.Path);

        Assert.Equal(ConversionStatus.Warning, result.Status);
        Assert.Contains(result.EffectiveWarnings, finding => finding.Code == "NO_TABLES");
        Assert.NotNull(result.OutputPath);
        Assert.True(File.Exists(result.OutputPath));
    }
}
