using System.IO;
using DocSharp.Binary.DocFileFormat;
using DocSharp.Binary.OpenXmlLib;
using DocSharp.Binary.OpenXmlLib.WordprocessingML;
using DocSharp.Binary.StructuredStorage.Reader;
using DocSharp.Binary.WordprocessingMLMapping;
using WordToExcel.App.Model;

namespace WordToExcel.App.Word;

internal sealed class LegacyDocConverter : ILegacyDocConverter
{
    public string ConvertToDocx(string sourceDocPath, string temporaryDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDocPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(temporaryDirectory);

        var canonicalSource = Path.GetFullPath(sourceDocPath);
        var canonicalTemporaryDirectory = Path.GetFullPath(temporaryDirectory);
        Directory.CreateDirectory(canonicalTemporaryDirectory);
        var outputPath = Path.Combine(canonicalTemporaryDirectory, $"legacy-{Guid.NewGuid():N}.docx");

        try
        {
            using var reader = new StructuredStorageReader(canonicalSource);
            var document = new WordDocument(reader);
            if (document.FIB.fEncrypted)
            {
                throw new LegacyDocConversionException(
                    LegacyDocError.ProtectedDocument,
                    "Старый документ Word защищён или зашифрован.");
            }

            using (var output = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document))
            {
                Converter.Convert(document, output);
            }

            return Path.GetFullPath(outputPath);
        }
        catch (LegacyDocConversionException)
        {
            TryDelete(outputPath);
            throw;
        }
        catch (Exception ex)
        {
            TryDelete(outputPath);
            throw new LegacyDocConversionException(
                LegacyDocError.ConversionFailure,
                "Не удалось преобразовать старый документ .doc во внутренний .docx.",
                ex);
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Cleanup is best-effort; the conversion exception remains the primary failure.
        }
    }
}
