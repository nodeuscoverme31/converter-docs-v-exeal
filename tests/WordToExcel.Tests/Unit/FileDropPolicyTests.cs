using WordToExcel.App;
using Xunit;

namespace WordToExcel.Tests.Unit;

public sealed class FileDropPolicyTests
{
    [Theory]
    [InlineData("sample.doc")]
    [InlineData("sample.docx")]
    [InlineData("SAMPLE.DOCX")]
    public void SingleSupportedWordFileIsAccepted(string path)
    {
        var accepted = FileDropPolicy.TryGetSingleSupportedWordFile(new[] { path }, out var selected);

        Assert.True(accepted);
        Assert.Equal(path, selected);
    }

    [Fact]
    public void MultipleFilesAreRejected()
    {
        var accepted = FileDropPolicy.TryGetSingleSupportedWordFile(new[] { "a.docx", "b.docx" }, out var selected);

        Assert.False(accepted);
        Assert.Null(selected);
    }

    [Theory]
    [InlineData("sample.xlsx")]
    [InlineData("sample.pdf")]
    [InlineData("")]
    public void UnsupportedOrEmptyFileIsRejected(string path)
    {
        var accepted = FileDropPolicy.TryGetSingleSupportedWordFile(new[] { path }, out var selected);

        Assert.False(accepted);
        Assert.Null(selected);
    }
}
