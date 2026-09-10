using WordToExcel.App.Model;

namespace WordToExcel.App.Conversion;

internal interface ITableNormalizer
{
    NormalizedTable Normalize(TableModel table);
}
