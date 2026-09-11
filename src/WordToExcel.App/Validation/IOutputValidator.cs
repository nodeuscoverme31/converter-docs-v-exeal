using WordToExcel.App.Model;

namespace WordToExcel.App.Validation;

internal sealed record OutputValidationFinding(string Code, string Message);

internal sealed record OutputValidationResult(
    bool IsValid,
    IReadOnlyList<OutputValidationFinding> Findings)
{
    public static OutputValidationResult Success { get; } =
        new(true, Array.Empty<OutputValidationFinding>());
}

internal interface IOutputValidator
{
    OutputValidationResult Validate(
        string xlsxPath,
        DocumentModel expectedDocument,
        IReadOnlyList<NormalizedTable> expectedTables);
}
