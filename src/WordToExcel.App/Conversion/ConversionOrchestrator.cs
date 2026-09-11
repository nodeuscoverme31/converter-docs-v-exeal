using System.IO;
using WordToExcel.App.Excel;
using WordToExcel.App.Model;
using WordToExcel.App.Validation;
using WordToExcel.App.Word;

namespace WordToExcel.App.Conversion;

internal sealed class ConversionOrchestrator
{
    private readonly InputDetector inputDetector;
    private readonly IWordDocumentReader wordReader;
    private readonly ILegacyDocConverter legacyDocConverter;
    private readonly ITableNormalizer tableNormalizer;
    private readonly ValuePolicy valuePolicy;
    private readonly IExcelWorkbookWriter workbookWriter;
    private readonly IOutputValidator outputValidator;
    private readonly OutputPublisher outputPublisher;

    public ConversionOrchestrator()
        : this(
            new InputDetector(),
            new DocxDocumentReader(),
            new LegacyDocConverter(),
            new TableNormalizer(),
            new ValuePolicy(),
            new ExcelWorkbookWriter(),
            new OpenXmlOutputValidator(),
            new OutputPublisher())
    {
    }

    internal ConversionOrchestrator(
        InputDetector inputDetector,
        IWordDocumentReader wordReader,
        ITableNormalizer tableNormalizer,
        ValuePolicy valuePolicy,
        IExcelWorkbookWriter workbookWriter,
        IOutputValidator outputValidator,
        OutputPublisher outputPublisher)
        : this(
            inputDetector,
            wordReader,
            new LegacyDocConverter(),
            tableNormalizer,
            valuePolicy,
            workbookWriter,
            outputValidator,
            outputPublisher)
    {
    }

    internal ConversionOrchestrator(
        InputDetector inputDetector,
        IWordDocumentReader wordReader,
        ILegacyDocConverter legacyDocConverter,
        ITableNormalizer tableNormalizer,
        ValuePolicy valuePolicy,
        IExcelWorkbookWriter workbookWriter,
        IOutputValidator outputValidator,
        OutputPublisher outputPublisher)
    {
        this.inputDetector = inputDetector ?? throw new ArgumentNullException(nameof(inputDetector));
        this.wordReader = wordReader ?? throw new ArgumentNullException(nameof(wordReader));
        this.legacyDocConverter = legacyDocConverter ?? throw new ArgumentNullException(nameof(legacyDocConverter));
        this.tableNormalizer = tableNormalizer ?? throw new ArgumentNullException(nameof(tableNormalizer));
        this.valuePolicy = valuePolicy ?? throw new ArgumentNullException(nameof(valuePolicy));
        this.workbookWriter = workbookWriter ?? throw new ArgumentNullException(nameof(workbookWriter));
        this.outputValidator = outputValidator ?? throw new ArgumentNullException(nameof(outputValidator));
        this.outputPublisher = outputPublisher ?? throw new ArgumentNullException(nameof(outputPublisher));
    }

    public ConversionResult Convert(string sourcePath) =>
        Convert(sourcePath, destinationDirectory: null);

    public ConversionResult Convert(string sourcePath, string? destinationDirectory)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return Error(ConversionErrorCategory.ReadFailure, "Не удалось прочитать исходный файл.");
        }

        string canonicalSource;
        try
        {
            canonicalSource = Path.GetFullPath(sourcePath);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Error(ConversionErrorCategory.ReadFailure, "Не удалось определить путь к исходному файлу.");
        }

        string? canonicalDestinationDirectory = null;
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            try
            {
                canonicalDestinationDirectory = Path.GetFullPath(destinationDirectory);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return Error(ConversionErrorCategory.OutputWrite, "Не удалось определить папку для сохранения Excel-файла.");
            }
        }

        InputKind inputKind;
        try
        {
            inputKind = inputDetector.Detect(canonicalSource);
        }
        catch (InvalidDataException)
        {
            return Error(ConversionErrorCategory.CorruptDocument, "Файл Word повреждён или имеет неверную структуру.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Error(ConversionErrorCategory.ReadFailure, "Не удалось прочитать исходный файл.");
        }

        if (inputKind == InputKind.Unsupported)
        {
            return Error(ConversionErrorCategory.UnsupportedFormat, "Поддерживаются файлы Word форматов .doc и .docx.");
        }

        if (inputKind == InputKind.Protected)
        {
            return Error(ConversionErrorCategory.ProtectedDocument, "Документ Word защищён паролем. Откройте его в Word, снимите защиту и попробуйте снова.");
        }

        var tempDirectory = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), "WordToExcel", Guid.NewGuid().ToString("N")));
        var tempXlsx = Path.Combine(tempDirectory, "validated-output.xlsx");

        try
        {
            Directory.CreateDirectory(tempDirectory);

            var docxPath = canonicalSource;
            if (inputKind == InputKind.Doc)
            {
                try
                {
                    docxPath = legacyDocConverter.ConvertToDocx(canonicalSource, tempDirectory);
                }
                catch (LegacyDocConversionException ex)
                {
                    return Error(MapLegacyError(ex.Error), ex.Message);
                }
            }

            DocumentModel document;
            try
            {
                document = wordReader.Read(docxPath);
            }
            catch (DocumentReadException ex)
            {
                return Error(MapReadError(ex.Error), ex.Message);
            }

            var normalizedTables = document.Tables
                .Select(tableNormalizer.Normalize)
                .Select(ApplyValuePlans)
                .ToArray();

            var warnings = document.Findings
                .Concat(normalizedTables.SelectMany(table => table.Findings))
                .Where(finding => finding.Severity == FindingSeverity.Warning)
                .ToList();

            if (document.Tables.Count == 0)
            {
                warnings.Add(new ConversionFinding(
                    "NO_TABLES",
                    "Таблицы не найдены. Обычный текст сохранён на листе «Контекст».",
                    FindingSeverity.Warning));
            }

            var distinctWarnings = warnings
                .Distinct()
                .ToArray();

            workbookWriter.Write(document, normalizedTables, tempXlsx);

            var validation = outputValidator.Validate(tempXlsx, document, normalizedTables);
            if (!validation.IsValid)
            {
                return Error(
                    ConversionErrorCategory.Validation,
                    "Созданный Excel не прошёл проверку сохранности данных.");
            }

            var outputPath = outputPublisher.Publish(tempXlsx, canonicalSource, canonicalDestinationDirectory);
            return new ConversionResult(
                distinctWarnings.Length == 0 ? ConversionStatus.Success : ConversionStatus.Warning,
                distinctWarnings.Length == 0 ? "Готово." : "Готово с предупреждениями.",
                outputPath,
                distinctWarnings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Error(
                ConversionErrorCategory.OutputWrite,
                "Не удалось сохранить Excel-файл. Выберите другую папку и попробуйте снова.");
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private NormalizedTable ApplyValuePlans(NormalizedTable table)
    {
        var cells = table.Cells
            .Select(cell => cell.SourceCellId is not null
                ? cell with { OutputValuePlan = valuePolicy.Plan(cell.SourceTextExact) }
                : cell)
            .ToArray();

        return table with { Cells = cells };
    }

    private static ConversionErrorCategory MapReadError(DocumentReadError error) => error switch
    {
        DocumentReadError.CorruptDocument => ConversionErrorCategory.CorruptDocument,
        DocumentReadError.ProtectedDocument => ConversionErrorCategory.ProtectedDocument,
        DocumentReadError.UnsupportedStructure => ConversionErrorCategory.UnsupportedFormat,
        _ => ConversionErrorCategory.ReadFailure,
    };

    private static ConversionErrorCategory MapLegacyError(LegacyDocError error) => error switch
    {
        LegacyDocError.ProtectedDocument => ConversionErrorCategory.ProtectedDocument,
        LegacyDocError.UnsupportedLegacyDoc => ConversionErrorCategory.UnsupportedFormat,
        _ => ConversionErrorCategory.LegacyConversion,
    };

    private static ConversionResult Error(ConversionErrorCategory category, string message) =>
        new(ConversionStatus.Error, message, ErrorCategory: category);

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Cleanup is best-effort and must not replace the conversion result.
        }
    }
}
