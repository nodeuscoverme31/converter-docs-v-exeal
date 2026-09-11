namespace WordToExcel.App.Model;

internal enum ConversionStatus
{
    Success,
    Warning,
    Error,
}

internal enum ConversionErrorCategory
{
    UnsupportedFormat,
    ProtectedDocument,
    CorruptDocument,
    LegacyConversion,
    OutputWrite,
    Validation,
    ReadFailure,
}

internal sealed record ConversionResult(
    ConversionStatus Status,
    string UserMessage,
    string? OutputPath = null,
    IReadOnlyList<ConversionFinding>? Warnings = null,
    ConversionErrorCategory? ErrorCategory = null)
{
    public IReadOnlyList<ConversionFinding> EffectiveWarnings =>
        Warnings ?? Array.Empty<ConversionFinding>();
}
