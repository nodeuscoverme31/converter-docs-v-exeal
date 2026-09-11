using System.Globalization;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using WordToExcel.App.Model;
using ModelTableRow = WordToExcel.App.Model.TableRow;
using WordTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;

namespace WordToExcel.App.Word;

internal sealed class DocxDocumentReader : IWordDocumentReader
{
    private static readonly HashSet<string> UnsupportedElementNames =
        new(StringComparer.OrdinalIgnoreCase) { "drawing", "pict", "object" };

    public DocumentModel Read(string docxPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(docxPath);

        try
        {
            using var package = WordprocessingDocument.Open(docxPath, false);
            var body = package.MainDocumentPart?.Document?.Body;
            if (body is null)
            {
                throw new DocumentReadException(
                    DocumentReadError.UnsupportedStructure,
                    "Word document does not contain a readable document body.");
            }

            var blocks = new List<DocumentBlock>();
            var tablesBySequence = new Dictionary<int, TableModel>();
            var unsupportedObjects = new List<UnsupportedObjectRecord>();
            var findings = new List<ConversionFinding>();
            var blockOrder = 0;
            var tableSequence = 0;

            for (var bodyIndex = 0; bodyIndex < body.ChildElements.Count; bodyIndex++)
            {
                var element = body.ChildElements[bodyIndex];
                var location = new SourceLocation("word/document.xml", bodyIndex);

                if (element is Paragraph paragraph)
                {
                    blocks.Add(new ParagraphBlock(blockOrder++, ExtractInlineText(paragraph), location));
                    foreach (var unsupported in EnumerateUnsupported(paragraph, skipNestedTables: false))
                    {
                        RecordUnsupported(unsupported, location, unsupportedObjects, findings);
                        blocks.Add(new UnsupportedObjectBlock(blockOrder++, unsupported.LocalName, location));
                    }

                    continue;
                }

                if (element is Table table)
                {
                    var tableId = ParseTable(
                        table,
                        bodyIndex,
                        parentTableId: null,
                        parentCellId: null,
                        ref tableSequence,
                        tablesBySequence,
                        unsupportedObjects,
                        findings);

                    blocks.Add(new TableReferenceBlock(blockOrder++, tableId, location));
                    continue;
                }

                if (!string.IsNullOrEmpty(element.InnerText) || ContainsUnsupported(element))
                {
                    var type = element.LocalName;
                    findings.Add(new ConversionFinding(
                        "UNSUPPORTED_BODY_ELEMENT",
                        $"Unsupported Word body element was preserved only as a finding: {type}.",
                        FindingSeverity.Warning,
                        location));
                    unsupportedObjects.Add(new UnsupportedObjectRecord(type, location, element.InnerText));
                    blocks.Add(new UnsupportedObjectBlock(blockOrder++, type, location));
                }
            }

            var tables = tablesBySequence
                .OrderBy(pair => pair.Key)
                .Select(pair => pair.Value)
                .ToArray();

            return new DocumentModel(
                Path.GetFullPath(docxPath),
                blocks,
                tables,
                unsupportedObjects,
                findings);
        }
        catch (DocumentReadException)
        {
            throw;
        }
        catch (OpenXmlPackageException ex)
        {
            throw new DocumentReadException(DocumentReadError.CorruptDocument, "The Word package is not readable.", ex);
        }
        catch (InvalidDataException ex)
        {
            throw new DocumentReadException(DocumentReadError.CorruptDocument, "The Word package is not readable.", ex);
        }
        catch (IOException ex)
        {
            throw new DocumentReadException(DocumentReadError.ReadFailure, "The Word document could not be read.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new DocumentReadException(DocumentReadError.ReadFailure, "The Word document could not be read.", ex);
        }
    }

    private static string ParseTable(
        Table table,
        int bodyIndex,
        string? parentTableId,
        string? parentCellId,
        ref int tableSequence,
        IDictionary<int, TableModel> tablesBySequence,
        ICollection<UnsupportedObjectRecord> unsupportedObjects,
        ICollection<ConversionFinding> findings)
    {
        var sequence = ++tableSequence;
        var tableId = $"Table-{sequence:000}";
        var rows = new List<ModelTableRow>();
        var tableFindings = new List<ConversionFinding>();
        var sourceGridHints = table.GetFirstChild<TableGrid>()?
            .Elements<GridColumn>()
            .Select(column => ParseGridWidth(column.Width?.Value))
            .ToArray() ?? Array.Empty<int>();

        var rowIndex = 0;
        foreach (var row in table.Elements<WordTableRow>())
        {
            var cells = new List<SourceCell>();
            var sourceCellIndex = 0;

            foreach (var cell in row.Elements<TableCell>())
            {
                var cellId = $"{tableId}-R{rowIndex + 1}C{sourceCellIndex + 1}";
                var cellLocation = new SourceLocation(
                    "word/document.xml",
                    bodyIndex,
                    sequence - 1,
                    rowIndex,
                    sourceCellIndex);
                var nestedTableIds = new List<string>();

                foreach (var nestedTable in cell.Elements<Table>())
                {
                    nestedTableIds.Add(ParseTable(
                        nestedTable,
                        bodyIndex,
                        tableId,
                        cellId,
                        ref tableSequence,
                        tablesBySequence,
                        unsupportedObjects,
                        findings));
                }

                var gridSpan = cell.TableCellProperties?.GridSpan?.Val?.Value ?? 1;
                if (gridSpan < 1)
                {
                    gridSpan = 1;
                    var finding = new ConversionFinding(
                        "INVALID_GRID_SPAN",
                        "A table cell declared a grid span smaller than one.",
                        FindingSeverity.Warning,
                        cellLocation);
                    findings.Add(finding);
                    tableFindings.Add(finding);
                }

                var verticalMerge = GetVerticalMerge(cell.TableCellProperties?.VerticalMerge);

                cells.Add(new SourceCell(
                    cellId,
                    ExtractCellText(cell),
                    rowIndex,
                    sourceCellIndex,
                    gridSpan,
                    verticalMerge,
                    nestedTableIds,
                    cellLocation));

                foreach (var unsupported in EnumerateUnsupported(cell, skipNestedTables: true))
                {
                    RecordUnsupported(unsupported, cellLocation, unsupportedObjects, findings);
                }

                sourceCellIndex++;
            }

            rows.Add(new ModelTableRow(rowIndex, cells));
            rowIndex++;
        }

        tablesBySequence[sequence] = new TableModel(
            tableId,
            sequence - 1,
            parentTableId,
            parentCellId,
            rows,
            sourceGridHints,
            tableFindings);

        return tableId;
    }

    private static string ExtractCellText(TableCell cell)
    {
        var paragraphs = cell.Elements<Paragraph>()
            .Select(ExtractInlineText)
            .ToArray();
        var length = paragraphs.Length;
        while (length > 0 && paragraphs[length - 1].Length == 0)
        {
            length--;
        }

        return string.Join("\n", paragraphs.Take(length));
    }

    private static string ExtractInlineText(OpenXmlElement element)
    {
        var builder = new StringBuilder();
        AppendInlineText(element, builder);
        return builder.ToString();
    }

    private static void AppendInlineText(OpenXmlElement element, StringBuilder builder)
    {
        foreach (var child in element.ChildElements)
        {
            switch (child)
            {
                case Text text:
                    builder.Append(text.Text);
                    break;
                case TabChar:
                    builder.Append('\t');
                    break;
                case Break:
                case CarriageReturn:
                    builder.Append('\n');
                    break;
                case Table:
                    break;
                default:
                    AppendInlineText(child, builder);
                    break;
            }
        }
    }

    private static IEnumerable<OpenXmlElement> EnumerateUnsupported(OpenXmlElement root, bool skipNestedTables)
    {
        foreach (var child in root.ChildElements)
        {
            if (skipNestedTables && child is Table)
            {
                continue;
            }

            if (UnsupportedElementNames.Contains(child.LocalName))
            {
                yield return child;
                continue;
            }

            foreach (var nested in EnumerateUnsupported(child, skipNestedTables))
            {
                yield return nested;
            }
        }
    }

    private static void RecordUnsupported(
        OpenXmlElement unsupported,
        SourceLocation location,
        ICollection<UnsupportedObjectRecord> unsupportedObjects,
        ICollection<ConversionFinding> findings)
    {
        var type = unsupported.LocalName;
        unsupportedObjects.Add(new UnsupportedObjectRecord(type, location));
        findings.Add(new ConversionFinding(
            "UNSUPPORTED_OBJECT",
            $"Unsupported Word object detected: {type}.",
            FindingSeverity.Warning,
            location));
    }

    private static bool ContainsUnsupported(OpenXmlElement root) =>
        EnumerateUnsupported(root, skipNestedTables: false).Any();

    private static int ParseGridWidth(string? rawWidth) =>
        int.TryParse(rawWidth, NumberStyles.Integer, CultureInfo.InvariantCulture, out var width)
            ? width
            : 0;

    private static string? GetVerticalMerge(VerticalMerge? merge)
    {
        if (merge is null)
        {
            return null;
        }

        return merge.Val?.Value == MergedCellValues.Restart ? "restart" : "continue";
    }
}
