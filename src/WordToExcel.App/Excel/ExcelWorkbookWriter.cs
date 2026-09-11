using ClosedXML.Excel;
using WordToExcel.App.Model;

namespace WordToExcel.App.Excel;

internal sealed class ExcelWorkbookWriter : IExcelWorkbookWriter
{
    public void Write(
        DocumentModel document,
        IReadOnlyList<NormalizedTable> normalizedTables,
        string outputPath)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(normalizedTables);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        using var workbook = new XLWorkbook();
        var sheetNamesByTableId = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var index = 0; index < normalizedTables.Count; index++)
        {
            var table = normalizedTables[index];
            var sheetName = TableSheetName(index);
            sheetNamesByTableId[table.TableId] = sheetName;
            var worksheet = workbook.Worksheets.Add(sheetName);

            foreach (var normalizedCell in table.Cells)
            {
                var plan = normalizedCell.OutputValuePlan;
                if (plan is null && normalizedCell.SourceCellId is null && normalizedCell.SourceTextExact.Length == 0)
                {
                    continue;
                }

                plan ??= new OutputValuePlan(
                    ValueMode.Text,
                    normalizedCell.SourceTextExact,
                    normalizedCell.SourceTextExact);

                var cell = worksheet.Cell(normalizedCell.Row + 1, normalizedCell.Column + 1);
                WriteValue(cell, plan);
            }
        }

        WriteContextSheet(workbook, document, sheetNamesByTableId);
        workbook.SaveAs(outputPath);
    }

    private static void WriteValue(IXLCell cell, OutputValuePlan plan)
    {
        switch (plan.Mode)
        {
            case ValueMode.Text:
                cell.Value = plan.ExpectedSourceText;
                break;

            case ValueMode.SafeNumber:
                cell.Value = Convert.ToDouble(plan.ExcelValue, System.Globalization.CultureInfo.InvariantCulture);
                break;

            case ValueMode.SafeDate:
                if (plan.ExcelValue is not DateTime date)
                {
                    throw new InvalidDataException("SafeDate output plan must contain a DateTime value.");
                }

                cell.Value = date;
                if (!string.IsNullOrWhiteSpace(plan.NumberFormat))
                {
                    cell.Style.NumberFormat.Format = plan.NumberFormat;
                }

                break;

            default:
                throw new InvalidDataException($"Unsupported output value mode: {plan.Mode}.");
        }
    }

    private static void WriteContextSheet(
        XLWorkbook workbook,
        DocumentModel document,
        IReadOnlyDictionary<string, string> sheetNamesByTableId)
    {
        var worksheet = workbook.Worksheets.Add("Контекст");
        worksheet.Cell(1, 1).Value = "Порядок";
        worksheet.Cell(1, 2).Value = "Тип";
        worksheet.Cell(1, 3).Value = "Содержимое";

        var row = 2;
        foreach (var block in document.Blocks.OrderBy(block => block.SourceOrder))
        {
            worksheet.Cell(row, 1).Value = block.SourceOrder;

            switch (block)
            {
                case ParagraphBlock paragraph:
                    worksheet.Cell(row, 2).Value = "Текст";
                    worksheet.Cell(row, 3).Value = paragraph.SourceTextExact;
                    break;

                case TableReferenceBlock tableReference:
                    worksheet.Cell(row, 2).Value = "Таблица";
                    worksheet.Cell(row, 3).Value = sheetNamesByTableId.TryGetValue(tableReference.TableId, out var sheetName)
                        ? sheetName
                        : tableReference.TableId;
                    break;

                case UnsupportedObjectBlock unsupported:
                    worksheet.Cell(row, 2).Value = "Неподдерживаемый объект";
                    worksheet.Cell(row, 3).Value = unsupported.ObjectType;
                    break;

                default:
                    throw new InvalidDataException($"Unsupported document block type: {block.GetType().Name}.");
            }

            row++;
        }
    }

    internal static string TableSheetName(int zeroBasedIndex) => $"Таблица {zeroBasedIndex + 1}";
}
