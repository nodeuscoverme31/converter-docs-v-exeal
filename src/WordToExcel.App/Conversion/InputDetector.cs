using System.IO;
using System.IO.Compression;
using DocSharp.Binary.StructuredStorage.Reader;

namespace WordToExcel.App.Conversion;

internal enum InputKind
{
    Unsupported,
    Protected,
    Docx,
    Doc,
}

internal sealed class InputDetector
{
    private static readonly byte[] CompoundFileSignature =
        { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

    public InputKind Detect(string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        var canonicalPath = Path.GetFullPath(sourcePath);
        var extension = Path.GetExtension(canonicalPath).ToLowerInvariant();

        return extension switch
        {
            ".docx" => DetectDocx(canonicalPath),
            ".doc" => LooksLikeCompoundFile(canonicalPath) ? InputKind.Doc : InputKind.Unsupported,
            _ => InputKind.Unsupported,
        };
    }

    private static InputKind DetectDocx(string path)
    {
        var prefix = ReadPrefix(path, CompoundFileSignature.Length);
        if (prefix.AsSpan().SequenceEqual(CompoundFileSignature))
        {
            return LooksLikeEncryptedOoxml(path) ? InputKind.Protected : InputKind.Unsupported;
        }

        if (prefix.Length < 2 || prefix[0] != (byte)'P' || prefix[1] != (byte)'K')
        {
            return InputKind.Unsupported;
        }

        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

        return archive.GetEntry("[Content_Types].xml") is not null &&
               archive.GetEntry("word/document.xml") is not null
            ? InputKind.Docx
            : InputKind.Unsupported;
    }

    private static bool LooksLikeEncryptedOoxml(string path)
    {
        try
        {
            using var reader = new StructuredStorageReader(path);
            var streamNames = reader.FullNameOfAllStreamEntries
                .Select(GetCompoundEntryName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return streamNames.Contains("EncryptionInfo") && streamNames.Contains("EncryptedPackage");
        }
        catch (IOException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("The compound Word container is not readable.", ex);
        }
    }

    private static bool LooksLikeCompoundFile(string path) =>
        ReadPrefix(path, CompoundFileSignature.Length).AsSpan().SequenceEqual(CompoundFileSignature);

    private static byte[] ReadPrefix(string path, int length)
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var buffer = new byte[length];
        var read = stream.Read(buffer, 0, buffer.Length);
        return read == buffer.Length ? buffer : buffer[..read];
    }

    private static string GetCompoundEntryName(string entryPath)
    {
        var normalized = entryPath.Replace('/', '\\').TrimEnd('\\');
        var separator = normalized.LastIndexOf('\\');
        return separator >= 0 ? normalized[(separator + 1)..] : normalized;
    }
}
