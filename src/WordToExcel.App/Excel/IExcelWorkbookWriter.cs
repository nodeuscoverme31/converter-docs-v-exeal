using WordToExcel.App.Model;

namespace WordToExcel.App.Excel;

internal interface IExcelWorkbookWriter
{
    void Write(
        DocumentModel document,
        IReadOnlyList<NormalizedTable> normalizedTables,
        string outputPath);
}
