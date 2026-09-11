using System.IO;

namespace WordToExcel.App.Conversion;

internal sealed class OutputPublisher
{
    public string GetDefaultOutputPath(string sourcePath) =>
        GetOutputPath(sourcePath, destinationDirectory: null);

    public string GetOutputPath(string sourcePath, string? destinationDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        var canonicalSource = Path.GetFullPath(sourcePath);
        var canonicalDirectory = string.IsNullOrWhiteSpace(destinationDirectory)
            ? Path.GetDirectoryName(canonicalSource)
                ?? throw new IOException("Source document does not have a destination directory.")
            : Path.GetFullPath(destinationDirectory);
        canonicalDirectory = Path.GetFullPath(canonicalDirectory);
        var baseName = Path.GetFileNameWithoutExtension(canonicalSource);

        for (var index = 0; ; index++)
        {
            var fileName = index == 0
                ? $"{baseName}.xlsx"
                : $"{baseName} ({index}).xlsx";
            var candidate = Path.GetFullPath(Path.Combine(canonicalDirectory, fileName));

            if (string.Equals(candidate, canonicalSource, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }
    }

    public string Publish(string validatedTempPath, string sourcePath) =>
        Publish(validatedTempPath, sourcePath, destinationDirectory: null);

    public string Publish(string validatedTempPath, string sourcePath, string? destinationDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(validatedTempPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        var canonicalTemp = Path.GetFullPath(validatedTempPath);
        var canonicalSource = Path.GetFullPath(sourcePath);
        var canonicalDestinationDirectory = string.IsNullOrWhiteSpace(destinationDirectory)
            ? null
            : Path.GetFullPath(destinationDirectory);

        while (true)
        {
            var candidate = GetOutputPath(canonicalSource, canonicalDestinationDirectory);
            try
            {
                File.Copy(canonicalTemp, candidate, overwrite: false);
                return candidate;
            }
            catch (IOException) when (File.Exists(candidate))
            {
                // Another writer won the collision race. Re-resolve a new unique name.
            }
        }
    }
}
