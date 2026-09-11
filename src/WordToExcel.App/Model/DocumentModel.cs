namespace WordToExcel.App.Model;

internal enum DocumentBlockKind
{
    Paragraph,
    TableReference,
    UnsupportedObject,
}

internal enum FindingSeverity
{
    Warning,
    Error,
}

internal enum ValueMode
{
    Text,
    SafeNumber,
    SafeDate,
}

internal sealed record SourceLocation(
    string Part,
    int BlockIndex,
    int? TableIndex = null,
    int? RowIndex = null,
    int? CellIndex = null);

internal sealed record ConversionFinding(
    string Code,
    string Message,
    FindingSeverity Severity,
    SourceLocation? SourceLocation = null);

internal abstract record DocumentBlock(
    int SourceOrder,
    DocumentBlockKind Kind,
    SourceLocation SourceLocation);

internal sealed record ParagraphBlock(
    int SourceOrder,
    string SourceTextExact,
    SourceLocation SourceLocation)
    : DocumentBlock(SourceOrder, DocumentBlockKind.Paragraph, SourceLocation);

internal sealed record TableReferenceBlock(
    int SourceOrder,
    string TableId,
    SourceLocation SourceLocation)
    : DocumentBlock(SourceOrder, DocumentBlockKind.TableReference, SourceLocation);

internal sealed record UnsupportedObjectBlock(
    int SourceOrder,
    string ObjectType,
    SourceLocation SourceLocation)
    : DocumentBlock(SourceOrder, DocumentBlockKind.UnsupportedObject, SourceLocation);

internal sealed record UnsupportedObjectRecord(
    string ObjectType,
    SourceLocation SourceLocation,
    string? Description = null);

internal sealed record SourceCell(
    string CellId,
    string SourceTextExact,
    int RowIndex,
    int SourceCellIndex,
    int GridSpan,
    string? VerticalMerge,
    IReadOnlyList<string> NestedTableIds,
    SourceLocation SourceLocation);

internal sealed record TableRow(
    int RowIndex,
    IReadOnlyList<SourceCell> Cells);

internal sealed record TableModel(
    string TableId,
    int SourceOrder,
    string? ParentTableId,
    string? ParentCellId,
    IReadOnlyList<TableRow> Rows,
    IReadOnlyList<int> SourceGridHints,
    IReadOnlyList<ConversionFinding> Findings);

internal sealed record OutputValuePlan(
    ValueMode Mode,
    string ExpectedSourceText,
    object? ExcelValue,
    string? NumberFormat = null);

internal sealed record NormalizedCell(
    string? SourceCellId,
    int Row,
    int Column,
    string SourceTextExact,
    OutputValuePlan? OutputValuePlan = null);

internal sealed record NormalizedTable(
    string TableId,
    int Width,
    int Height,
    IReadOnlyList<NormalizedCell> Cells,
    IReadOnlyList<ConversionFinding> Findings);

internal sealed class DocumentModel
{
    public DocumentModel(
        string sourceDocument,
        IReadOnlyList<DocumentBlock> blocks,
        IReadOnlyList<TableModel> tables,
        IReadOnlyList<UnsupportedObjectRecord> unsupportedObjects,
        IReadOnlyList<ConversionFinding> findings)
    {
        SourceDocument = sourceDocument;
        Blocks = blocks;
        Tables = tables;
        UnsupportedObjects = unsupportedObjects;
        Findings = findings;
    }

    public string SourceDocument { get; }

    public IReadOnlyList<DocumentBlock> Blocks { get; }

    public IReadOnlyList<TableModel> Tables { get; }

    public IReadOnlyList<UnsupportedObjectRecord> UnsupportedObjects { get; }

    public IReadOnlyList<ConversionFinding> Findings { get; }
}

internal enum DocumentReadError
{
    CorruptDocument,
    ProtectedDocument,
    UnsupportedStructure,
    ReadFailure,
}

internal sealed class DocumentReadException : Exception
{
    public DocumentReadException(DocumentReadError error, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Error = error;
    }

    public DocumentReadError Error { get; }
}

internal enum LegacyDocError
{
    UnsupportedLegacyDoc,
    ProtectedDocument,
    ConversionFailure,
}

internal sealed class LegacyDocConversionException : Exception
{
    public LegacyDocConversionException(LegacyDocError error, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Error = error;
    }

    public LegacyDocError Error { get; }
}
