using DocumentFormat.OpenXml.Wordprocessing;
using WordToExcel.App.Model;
using WordToExcel.App.Word;
using WordToExcel.Tests.Fixtures;
using Xunit;

namespace WordToExcel.Tests.Integration;

public sealed class DocxDocumentReaderTests
{
    [Fact]
    public void ReadsOrderedTextAndMultipleTablesWithoutChangingSource()
    {
        using var fixture = DocxFixtureFactory.Create(body =>
        {
            body.Append(DocxFixtureFactory.Paragraph("Перед таблицей"));
            body.Append(DocxFixtureFactory.Table(
                DocxFixtureFactory.Row(DocxFixtureFactory.Cell("A"), DocxFixtureFactory.Cell("B"))));
            body.Append(DocxFixtureFactory.Paragraph("Между таблицами"));
            body.Append(DocxFixtureFactory.Table(
                DocxFixtureFactory.Row(DocxFixtureFactory.Cell("C"))));
            body.Append(DocxFixtureFactory.Paragraph("После таблиц"));
        });
        var before = File.ReadAllBytes(fixture.Path);

        var model = new DocxDocumentReader().Read(fixture.Path);

        Assert.Equal(2, model.Tables.Count);
        Assert.Collection(
            model.Blocks,
            block => Assert.Equal("Перед таблицей", Assert.IsType<ParagraphBlock>(block).SourceTextExact),
            block => Assert.Equal("Table-001", Assert.IsType<TableReferenceBlock>(block).TableId),
            block => Assert.Equal("Между таблицами", Assert.IsType<ParagraphBlock>(block).SourceTextExact),
            block => Assert.Equal("Table-002", Assert.IsType<TableReferenceBlock>(block).TableId),
            block => Assert.Equal("После таблиц", Assert.IsType<ParagraphBlock>(block).SourceTextExact));
        Assert.Equal("A", model.Tables[0].Rows[0].Cells[0].SourceTextExact);
        Assert.Equal("B", model.Tables[0].Rows[0].Cells[1].SourceTextExact);
        Assert.Equal(before, File.ReadAllBytes(fixture.Path));
    }

    [Fact]
    public void CapturesGridSpanVerticalMergeIrregularRowsAndEmptyCells()
    {
        using var fixture = DocxFixtureFactory.Create(body =>
        {
            body.Append(DocxFixtureFactory.Table(
                DocxFixtureFactory.Row(
                    DocxFixtureFactory.Cell("wide", span: 2),
                    DocxFixtureFactory.Cell("top", verticalMerge: MergedCellValues.Restart)),
                DocxFixtureFactory.Row(
                    DocxFixtureFactory.Cell(string.Empty),
                    DocxFixtureFactory.Cell(string.Empty, verticalMerge: MergedCellValues.Continue))));
        });

        var table = Assert.Single(new DocxDocumentReader().Read(fixture.Path).Tables);

        Assert.Equal(2, table.Rows.Count);
        Assert.Equal(2, table.Rows[0].Cells.Count);
        Assert.Equal(2, table.Rows[0].Cells[0].GridSpan);
        Assert.Equal("restart", table.Rows[0].Cells[1].VerticalMerge);
        Assert.Equal("continue", table.Rows[1].Cells[1].VerticalMerge);
        Assert.Equal(string.Empty, table.Rows[1].Cells[0].SourceTextExact);
        Assert.NotEmpty(table.SourceGridHints);
    }

    [Fact]
    public void ReadsNestedTableSeparatelyAndDoesNotDuplicateItsTextInParentCell()
    {
        var nested = DocxFixtureFactory.Table(
            DocxFixtureFactory.Row(DocxFixtureFactory.Cell("Вложенные данные")));
        using var fixture = DocxFixtureFactory.Create(body =>
        {
            body.Append(DocxFixtureFactory.Table(
                DocxFixtureFactory.Row(DocxFixtureFactory.Cell("Родитель", nestedTable: nested))));
        });

        var model = new DocxDocumentReader().Read(fixture.Path);

        Assert.Equal(2, model.Tables.Count);
        var parent = model.Tables[0];
        var nestedModel = model.Tables[1];
        var parentCell = Assert.Single(Assert.Single(parent.Rows).Cells);
        Assert.Equal("Родитель", parentCell.SourceTextExact);
        Assert.Equal(new[] { "Table-002" }, parentCell.NestedTableIds);
        Assert.Equal("Table-001", nestedModel.ParentTableId);
        Assert.Equal(parentCell.CellId, nestedModel.ParentCellId);
        Assert.Equal("Вложенные данные", nestedModel.Rows[0].Cells[0].SourceTextExact);
    }

    [Fact]
    public void PreservesUnicodeAndLineBreakAndReportsDrawing()
    {
        using var fixture = DocxFixtureFactory.Create(body =>
        {
            body.Append(new Paragraph(
                new Run(new Text("Привет"), new Break(), new Text("мир — ёж"), new Drawing())));
        });

        var model = new DocxDocumentReader().Read(fixture.Path);

        var paragraph = Assert.IsType<ParagraphBlock>(model.Blocks[0]);
        Assert.Equal("Привет\nмир — ёж", paragraph.SourceTextExact);
        Assert.Single(model.UnsupportedObjects);
        Assert.Contains(model.Findings, finding => finding.Code == "UNSUPPORTED_OBJECT");
        Assert.Contains(model.Blocks, block => block is UnsupportedObjectBlock);
    }
}
