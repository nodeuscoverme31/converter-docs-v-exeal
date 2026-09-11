using System.IO;

namespace WordToExcel.App;

internal static class FileDropPolicy
{
    internal static bool TryGetSingleSupportedWordFile(IReadOnlyList<string>? files, out string? path)
    {
        path = null;
        if (files is null || files.Count != 1)
        {
            return false;
        }

        var candidate = files[0];
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        var extension = Path.GetExtension(candidate);
        if (!string.Equals(extension, ".doc", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        path = candidate;
        return true;
    }
}
