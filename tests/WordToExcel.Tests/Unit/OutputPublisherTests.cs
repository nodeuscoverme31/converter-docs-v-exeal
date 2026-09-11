using WordToExcel.App.Conversion;
using Xunit;

namespace WordToExcel.Tests.Unit;

public sealed class OutputPublisherTests
{
    [Fact]
    public void PublishesBesideCanonicalSourceWithCollisionSafeNameAndNoOverwrite()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"publisher-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            var sourcePath = Path.Combine(directory, "source.docx");
            var tempPath = Path.Combine(directory, "validated-temp.xlsx");
            var existingPath = Path.Combine(directory, "source.xlsx");
            File.WriteAllText(sourcePath, "source");
            File.WriteAllText(tempPath, "new workbook");
            File.WriteAllText(existingPath, "existing workbook");
            var nonCanonicalSource = Path.Combine(directory, ".", "unused", "..", "source.docx");

            var published = new OutputPublisher().Publish(tempPath, nonCanonicalSource);

            Assert.Equal(Path.Combine(directory, "source (1).xlsx"), published);
            Assert.Equal(Path.GetFullPath(published), published);
            Assert.Equal("existing workbook", File.ReadAllText(existingPath));
            Assert.Equal("new workbook", File.ReadAllText(published));
            Assert.Equal("source", File.ReadAllText(sourcePath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void EquivalentSourcePathFormsResolveToSameDefaultDestination()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"publisher-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            var sourcePath = Path.Combine(directory, "source.docx");
            File.WriteAllText(sourcePath, "source");
            var alternateForm = Path.Combine(directory, ".", "unused", "..", "source.docx");
            var publisher = new OutputPublisher();

            var canonicalCandidate = publisher.GetDefaultOutputPath(sourcePath);
            var alternateCandidate = publisher.GetDefaultOutputPath(alternateForm);

            Assert.Equal(canonicalCandidate, alternateCandidate);
            Assert.Equal(Path.Combine(directory, "source.xlsx"), canonicalCandidate);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void AlternateDestinationIsCanonicalizedAndKeepsCollisionPolicy()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"publisher-tests-{Guid.NewGuid():N}");
        var alternateDirectory = Path.Combine(directory, "alternate");
        Directory.CreateDirectory(alternateDirectory);

        try
        {
            var sourcePath = Path.Combine(directory, "source.docx");
            var tempPath = Path.Combine(directory, "validated-temp.xlsx");
            var existingPath = Path.Combine(alternateDirectory, "source.xlsx");
            File.WriteAllText(sourcePath, "source");
            File.WriteAllText(tempPath, "new workbook");
            File.WriteAllText(existingPath, "existing workbook");
            var nonCanonicalAlternate = Path.Combine(directory, ".", "unused", "..", "alternate");

            var published = new OutputPublisher().Publish(tempPath, sourcePath, nonCanonicalAlternate);

            Assert.Equal(Path.Combine(alternateDirectory, "source (1).xlsx"), published);
            Assert.Equal(Path.GetFullPath(published), published);
            Assert.Equal("existing workbook", File.ReadAllText(existingPath));
            Assert.Equal("new workbook", File.ReadAllText(published));
            Assert.Equal("source", File.ReadAllText(sourcePath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
