using WordToExcel.App.Model;

namespace WordToExcel.App.Conversion;

internal sealed class TableNormalizer : ITableNormalizer
{
    public NormalizedTable Normalize(TableModel table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var findings = new List<ConversionFinding>(table.Findings);
        var placed = new Dictionary<(int Row, int Column), NormalizedCell>();
        var activeMerges = new Dictionary<int, ActiveMerge>();
        var maxColumnExclusive = 0;
        var height = table.Rows.Count == 0 ? 0 : table.Rows.Max(row => row.RowIndex) + 1;

        foreach (var row in table.Rows.OrderBy(row => row.RowIndex))
        {
            var nextActiveMerges = new Dictionary<int, ActiveMerge>();
            var column = 0;

            foreach (var cell in row.Cells.OrderBy(cell => cell.SourceCellIndex))
            {
                var span = Math.Max(1, cell.GridSpan);
                var mergeState = NormalizeMergeState(cell.VerticalMerge);

                if (mergeState == MergeState.Continue)
                {
                    var continuation = activeMerges.Values
                        .Where(merge => merge.StartColumn >= column && merge.Span == span)
                        .OrderBy(merge => merge.StartColumn)
                        .FirstOrDefault();

                    if (continuation is not null && cell.SourceTextExact.Length == 0)
                    {
                        PlaceCoveredRange(placed, row.RowIndex, continuation.StartColumn, continuation.Span);
                        nextActiveMerges[continuation.StartColumn] = continuation;
                        column = continuation.StartColumn + continuation.Span;
                        maxColumnExclusive = Math.Max(maxColumnExclusive, column);
                        continue;
                    }

                    findings.Add(new ConversionFinding(
                        "TABLE_VERTICAL_MERGE_AMBIGUOUS",
                        "A vertical-merge continuation could not be matched safely to a preceding merge anchor.",
                        FindingSeverity.Warning,
                        cell.SourceLocation));
                }

                PlaceAnchor(placed, row.RowIndex, column, cell, span);

                if (mergeState == MergeState.Restart)
                {
                    nextActiveMerges[column] = new ActiveMerge(column, span);
                }

                column += span;
                maxColumnExclusive = Math.Max(maxColumnExclusive, column);
            }

            activeMerges = nextActiveMerges;
        }

        var width = Math.Max(maxColumnExclusive, table.SourceGridHints.Count);
        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                placed.TryAdd(
                    (row, column),
                    new NormalizedCell(null, row, column, string.Empty));
            }
        }

        var cells = placed.Values
            .OrderBy(cell => cell.Row)
            .ThenBy(cell => cell.Column)
            .ToArray();

        return new NormalizedTable(table.TableId, width, height, cells, findings);
    }

    private static void PlaceAnchor(
        IDictionary<(int Row, int Column), NormalizedCell> placed,
        int row,
        int startColumn,
        SourceCell sourceCell,
        int span)
    {
        placed[(row, startColumn)] = new NormalizedCell(
            sourceCell.CellId,
            row,
            startColumn,
            sourceCell.SourceTextExact);

        PlaceCoveredRange(placed, row, startColumn + 1, span - 1);
    }

    private static void PlaceCoveredRange(
        IDictionary<(int Row, int Column), NormalizedCell> placed,
        int row,
        int startColumn,
        int count)
    {
        for (var offset = 0; offset < count; offset++)
        {
            var column = startColumn + offset;
            placed[(row, column)] = new NormalizedCell(null, row, column, string.Empty);
        }
    }

    private static MergeState NormalizeMergeState(string? value)
    {
        if (string.Equals(value, "restart", StringComparison.OrdinalIgnoreCase))
        {
            return MergeState.Restart;
        }

        if (string.Equals(value, "continue", StringComparison.OrdinalIgnoreCase))
        {
            return MergeState.Continue;
        }

        return MergeState.None;
    }

    private enum MergeState
    {
        None,
        Restart,
        Continue,
    }

    private sealed record ActiveMerge(int StartColumn, int Span);
}
