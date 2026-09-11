using System.Runtime.CompilerServices;
using WordToExcel.App.Conversion;
using WordToExcel.App.Model;
using Xunit;

namespace WordToExcel.Tests.E2E;

public sealed class InputFailureTests
{
    private static readonly string InvalidFixtureDirectory = Path.GetFullPath(
        Path.Combine(Path.GetDirectoryName(ThisSourceFile())!, "..", "Fixtures", "Invalid"));

    private static readonly string LegacyFixtureDirectory = Path.GetFullPath(
        Path.Combine(Path.GetDirectoryName(ThisSourceFile())!, "..", "Fixtures", "LegacyDoc"));

    [Fact]
    public void ExtensionOnlyDocx_IsUnsupportedRatherThanCorruptOrSuccess()
    {
        var source = InvalidFixture("not-word.docx");
        var before = File.ReadAllBytes(source);

        var result = new ConversionOrchestrator().Convert(source);

        Assert.Equal(ConversionStatus.Error, result.Status);
        Assert.Equal(ConversionErrorCategory.UnsupportedFormat, result.ErrorCategory);
        Assert.Null(result.OutputPath);
        Assert.Equal(before, File.ReadAllBytes(source));
        AssertNoPublishedWorkbook(source);
    }

    [Fact]
    public void BrokenZipDocx_IsReportedAsCorrupt()
    {
        var source = InvalidFixture("corrupt.docx");
        var before = File.ReadAllBytes(source);

        var result = new ConversionOrchestrator().Convert(source);

        Assert.Equal(ConversionStatus.Error, result.Status);
        Assert.Equal(ConversionErrorCategory.CorruptDocument, result.ErrorCategory);
        Assert.Null(result.OutputPath);
        Assert.Equal(before, File.ReadAllBytes(source));
        AssertNoPublishedWorkbook(source);
    }

    [Fact]
    public void EncryptedOoxmlDocx_IsReportedAsProtected()
    {
        var source = InvalidFixture("protected.docx");
        var before = File.ReadAllBytes(source);

        var result = new ConversionOrchestrator().Convert(source);

        Assert.Equal(ConversionStatus.Error, result.Status);
        Assert.Equal(ConversionErrorCategory.ProtectedDocument, result.ErrorCategory);
        Assert.Null(result.OutputPath);
        Assert.Equal(before, File.ReadAllBytes(source));
        AssertNoPublishedWorkbook(source);
    }

    [Fact]
    public void ProtectedLegacyDoc_IsReportedAsProtected()
    {
        var source = LegacyFixture("protected.doc");
        var before = File.ReadAllBytes(source);

        var result = new ConversionOrchestrator().Convert(source);

        Assert.Equal(ConversionStatus.Error, result.Status);
        Assert.Equal(ConversionErrorCategory.ProtectedDocument, result.ErrorCategory);
        Assert.Null(result.OutputPath);
        Assert.Equal(before, File.ReadAllBytes(source));
        AssertNoPublishedWorkbook(source);
    }

    [Fact]
    public void DamagedLegacyDoc_IsNeverPublishedAsSuccess()
    {
        var source = LegacyFixture("damaged.doc");
        var before = File.ReadAllBytes(source);

        var result = new ConversionOrchestrator().Convert(source);

        Assert.Equal(ConversionStatus.Error, result.Status);
        Assert.True(result.ErrorCategory is ConversionErrorCategory.CorruptDocument or ConversionErrorCategory.LegacyConversion);
        Assert.Null(result.OutputPath);
        Assert.Equal(before, File.ReadAllBytes(source));
        AssertNoPublishedWorkbook(source);
    }

    private static string InvalidFixture(string name)
    {
        var path = Path.Combine(InvalidFixtureDirectory, name);
        Assert.True(File.Exists(path), $"Missing invalid-input fixture: {path}");
        return path;
    }

    private static string LegacyFixture(string name)
    {
        var path = Path.Combine(LegacyFixtureDirectory, name);
        Assert.True(File.Exists(path), $"Missing legacy fixture: {path}");
        return path;
    }

    private static void AssertNoPublishedWorkbook(string sourcePath)
    {
        var directory = Path.GetDirectoryName(sourcePath)!;
        var baseName = Path.GetFileNameWithoutExtension(sourcePath);
        Assert.Empty(Directory.GetFiles(directory, $"{baseName}*.xlsx", SearchOption.TopDirectoryOnly));
    }

    private static string ThisSourceFile([CallerFilePath] string path = "") => path;
}
