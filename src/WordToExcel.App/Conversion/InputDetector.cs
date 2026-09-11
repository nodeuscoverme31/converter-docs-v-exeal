using System.IO.Compression;

namespace WordToExcel.App.Conversion;

internal enum InputKind
{
    Unsupported,
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
            ".docx" => LooksLikeDocx(canonicalPath) ? InputKind.Docx : InputKind.Unsupported,
            ".doc" => LooksLikeCompoundDoc(canonicalPath) ? InputKind.Doc : InputKind.Unsupported,
            _ => InputKind.Unsupported,
        };
    }

    private static bool LooksLikeDocx(string path)
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

        return archive.GetEntry("[Content_Types].xml") is not null &&
               archive.GetEntry("word/document.xml") is not null;
    }

    private static bool LooksLikeCompoundDoc(string path)
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        Span<byte> actual = stackalloc byte[CompoundFileSignature.Length];
        var read = stream.Read(actual);
        return read == CompoundFileSignature.Length && actual.SequenceEqual(CompoundFileSignature);
    }
}
