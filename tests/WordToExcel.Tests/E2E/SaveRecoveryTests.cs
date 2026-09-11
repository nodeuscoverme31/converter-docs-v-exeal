using DocumentFormat.OpenXml.Wordprocessing;
using WordToExcel.App.Conversion;
using WordToExcel.App.Model;
using WordToExcel.Tests.Fixtures;
using Xunit;

namespace WordToExcel.Tests.E2E;

public sealed class SaveRecoveryTests
{
    [Fact]
    public void FailedDestination_CanRecoverToCanonicalAlternateDirectoryWithoutTouchingSource()
    {
        using var fixture = DocxFixtureFactory.Create(body =>
        {
            body.Append(DocxFixtureFactory.Table(
                DocxFixtureFactory.Row(
                    DocxFixtureFactory.Cell("001234"),
                    DocxFixtureFactory.Cell("Тест"))));
        });

        var sourceBytes = File.ReadAllBytes(fixture.Path);
        var root = Path.GetDirectoryName(fixture.Path)!;
        var invalidDestination = Path.Combine(root, "not-a-directory");
        File.WriteAllText(invalidDestination, "occupied by a file");

        var orchestrator = new ConversionOrchestrator();
        var failed = orchestrator.Convert(fixture.Path, invalidDestination);

        Assert.Equal(ConversionStatus.Error, failed.Status);
        Assert.Equal(ConversionErrorCategory.OutputWrite, failed.ErrorCategory);
        Assert.Null(failed.OutputPath);
        Assert.Equal(sourceBytes, File.ReadAllBytes(fixture.Path));
        Assert.False(File.Exists(Path.Combine(root, "fixture.xlsx")));

        var alternateDirectory = Path.Combine(root, "alternate");
        Directory.CreateDirectory(alternateDirectory);
        var existing = Path.Combine(alternateDirectory, "fixture.xlsx");
        File.WriteAllText(existing, "existing workbook");
        var nonCanonicalAlternate = Path.Combine(root, ".", "unused", "..", "alternate");

        var recovered = orchestrator.Convert(fixture.Path, nonCanonicalAlternate);

        Assert.Equal(ConversionStatus.Success, recovered.Status);
        Assert.Equal(Path.Combine(alternateDirectory, "fixture (1).xlsx"), recovered.OutputPath);
        Assert.Equal(Path.GetFullPath(recovered.OutputPath!), recovered.OutputPath);
        Assert.Equal("existing workbook", File.ReadAllText(existing));
        Assert.Equal(sourceBytes, File.ReadAllBytes(fixture.Path));
        Assert.True(File.Exists(recovered.OutputPath));
    }
}
