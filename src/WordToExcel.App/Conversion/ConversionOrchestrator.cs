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
    private readonly ITableNormalizer tableNormalizer;
    private readonly ValuePolicy valuePolicy;
    private readonly IExcelWorkbookWriter workbookWriter;
    private readonly IOutputValidator outputValidator;
    private readonly OutputPublisher outputPublisher;

    public ConversionOrchestrator()
        : this(
            new InputDetector(),
            new DocxDocumentReader(),
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
    {
        this.inputDetector = inputDetector ?? throw new ArgumentNullException(nameof(inputDetector));
        this.wordReader = wordReader ?? throw new ArgumentNullException(nameof(wordReader));
        this.tableNormalizer = tableNormalizer ?? throw new ArgumentNullException(nameof(tableNormalizer));
        this.valuePolicy = valuePolicy ?? throw new ArgumentNullException(nameof(valuePolicy));
        this.workbookWriter = workbookWriter ?? throw new ArgumentNullException(nameof(workbookWriter));
        this.outputValidator = outputValidator ?? throw new ArgumentNullException(nameof(outputValidator));
        this.outputPublisher = outputPublisher ?? throw new ArgumentNullException(nameof(outputPublisher));
    }

    public ConversionResult Convert(string sourcePath)
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

        if (inputKind != InputKind.Docx)
        {
            return Error(
                ConversionErrorCategory.UnsupportedFormat,
                inputKind == InputKind.Doc
                    ? "Поддержка старого формата .doc ещё не подключена к основному конвейеру."
                    : "Поддерживается файл Word формата .docx.");
        }

        DocumentModel document;
        try
        {
            document = wordReader.Read(canonicalSource);
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
            .Distinct()
            .ToArray();

        var tempDirectory = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), "WordToExcel", Guid.NewGuid().ToString("N")));
        var tempXlsx = Path.Combine(tempDirectory, "validated-output.xlsx");

        try
        {
            Directory.CreateDirectory(tempDirectory);
            workbookWriter.Write(document, normalizedTables, tempXlsx);

            var validation = outputValidator.Validate(tempXlsx, document, normalizedTables);
            if (!validation.IsValid)
            {
                return Error(
                    ConversionErrorCategory.Validation,
                    "Созданный Excel не прошёл проверку сохранности данных.");
            }

            var outputPath = outputPublisher.Publish(tempXlsx, canonicalSource);
            return new ConversionResult(
                warnings.Length == 0 ? ConversionStatus.Success : ConversionStatus.Warning,
                warnings.Length == 0 ? "Готово." : "Готово с предупреждениями.",
                outputPath,
                warnings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Error(ConversionErrorCategory.OutputWrite, "Не удалось сохранить Excel-файл.");
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
