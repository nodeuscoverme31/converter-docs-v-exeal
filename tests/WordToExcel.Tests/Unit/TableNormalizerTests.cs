using WordToExcel.App.Conversion;
using WordToExcel.App.Model;
using Xunit;

namespace WordToExcel.Tests.Unit;

public sealed class TableNormalizerTests
{
    [Fact]
    public void NormalizesRegularRowsIntoRectangularGrid()
    {
        var table = Table(
            Row(0, Cell("a", "A", 0, 0), Cell("b", "B", 0, 1)),
            Row(1, Cell("c", "C", 1, 0), Cell("d", "D", 1, 1)));

        var result = new TableNormalizer().Normalize(table);

        Assert.Equal(2, result.Width);
        Assert.Equal(2, result.Height);
        Assert.Equal("A", At(result, 0, 0).SourceTextExact);
        Assert.Equal("B", At(result, 0, 1).SourceTextExact);
        Assert.Equal("C", At(result, 1, 0).SourceTextExact);
        Assert.Equal("D", At(result, 1, 1).SourceTextExact);
        Assert.Empty(result.Findings);
    }

    [Fact]
    public void HorizontalSpanKeepsContentOnlyInAnchorAndReservesCoveredCells()
    {
        var table = Table(
            Row(0, Cell("span", "Merged", 0, 0, gridSpan: 2), Cell("tail", "Tail", 0, 1)));

        var result = new TableNormalizer().Normalize(table);

        Assert.Equal(3, result.Width);
        Assert.Equal("Merged", At(result, 0, 0).SourceTextExact);
        Assert.Equal("span", At(result, 0, 0).SourceCellId);
        Assert.Equal(string.Empty, At(result, 0, 1).SourceTextExact);
        Assert.Null(At(result, 0, 1).SourceCellId);
        Assert.Equal("Tail", At(result, 0, 2).SourceTextExact);
    }

    [Fact]
    public void VerticalMergeKeepsRestartContentAndBlanksContinuationCoordinate()
    {
        var table = Table(
            Row(0, Cell("vm0", "Top", 0, 0, verticalMerge: "restart"), Cell("r0c1", "X", 0, 1)),
            Row(1, Cell("vm1", string.Empty, 1, 0, verticalMerge: "continue"), Cell("r1c1", "Y", 1, 1)));

        var result = new TableNormalizer().Normalize(table);

        Assert.Equal("Top", At(result, 0, 0).SourceTextExact);
        Assert.Equal("vm0", At(result, 0, 0).SourceCellId);
        Assert.Equal(string.Empty, At(result, 1, 0).SourceTextExact);
        Assert.Null(At(result, 1, 0).SourceCellId);
        Assert.Equal("Y", At(result, 1, 1).SourceTextExact);
        Assert.Empty(result.Findings);
    }

    [Fact]
    public void IrregularRowsArePaddedWithoutShiftingExistingCells()
    {
        var table = Table(
            Row(0, Cell("a", "A", 0, 0), Cell("b", "B", 0, 1), Cell("c", "C", 0, 2)),
            Row(1, Cell("d", "D", 1, 0)));

        var result = new TableNormalizer().Normalize(table);

        Assert.Equal(3, result.Width);
        Assert.Equal(2, result.Height);
        Assert.Equal("D", At(result, 1, 0).SourceTextExact);
        Assert.Equal(string.Empty, At(result, 1, 1).SourceTextExact);
        Assert.Equal(string.Empty, At(result, 1, 2).SourceTextExact);
    }

    [Fact]
    public void EmptySourceCellRemainsARealAnchorCell()
    {
        var table = Table(Row(0, Cell("empty", string.Empty, 0, 0), Cell("value", "V", 0, 1)));

        var result = new TableNormalizer().Normalize(table);

        Assert.Equal("empty", At(result, 0, 0).SourceCellId);
        Assert.Equal(string.Empty, At(result, 0, 0).SourceTextExact);
        Assert.Equal("V", At(result, 0, 1).SourceTextExact);
    }

    [Fact]
    public void OrphanVerticalContinuationIsPreservedAndReportedAsAmbiguous()
    {
        var table = Table(Row(0, Cell("orphan", "Unexpected", 0, 0, verticalMerge: "continue")));

        var result = new TableNormalizer().Normalize(table);

        Assert.Equal("Unexpected", At(result, 0, 0).SourceTextExact);
        Assert.Equal("orphan", At(result, 0, 0).SourceCellId);
        var finding = Assert.Single(result.Findings);
        Assert.Equal("TABLE_VERTICAL_MERGE_AMBIGUOUS", finding.Code);
        Assert.Equal(FindingSeverity.Warning, finding.Severity);
    }

    private static NormalizedCell At(NormalizedTable table, int row, int column) =>
        Assert.Single(table.Cells, cell => cell.Row == row && cell.Column == column);

    private static TableModel Table(params TableRow[] rows) =>
        new(
            "Table-001",
            0,
            null,
            null,
            rows,
            Array.Empty<int>(),
            Array.Empty<ConversionFinding>());

    private static TableRow Row(int rowIndex, params SourceCell[] cells) => new(rowIndex, cells);

    private static SourceCell Cell(
        string id,
        string text,
        int row,
        int sourceCell,
        int gridSpan = 1,
        string? verticalMerge = null) =>
        new(
            id,
            text,
            row,
            sourceCell,
            gridSpan,
            verticalMerge,
            Array.Empty<string>(),
            new SourceLocation("word/document.xml", row, 0, row, sourceCell));
}
