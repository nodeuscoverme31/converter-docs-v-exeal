using System.IO;

namespace WordToExcel.App.Conversion;

internal sealed class OutputPublisher
{
    public string GetDefaultOutputPath(string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        var canonicalSource = Path.GetFullPath(sourcePath);
        var directory = Path.GetDirectoryName(canonicalSource)
            ?? throw new IOException("Source document does not have a destination directory.");
        var canonicalDirectory = Path.GetFullPath(directory);
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

    public string Publish(string validatedTempPath, string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(validatedTempPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        var canonicalTemp = Path.GetFullPath(validatedTempPath);
        var canonicalSource = Path.GetFullPath(sourcePath);

        while (true)
        {
            var candidate = GetDefaultOutputPath(canonicalSource);
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
