using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WordToExcel.Tests.Fixtures;

internal sealed class DocxFixture : IDisposable
{
    public DocxFixture(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public void Dispose()
    {
        var directory = System.IO.Path.GetDirectoryName(Path);
        if (directory is not null && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}

internal static class DocxFixtureFactory
{
    public static DocxFixture Create(Action<Body> configure)
    {
        var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"word-to-excel-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = System.IO.Path.Combine(directory, "fixture.docx");

        using (var package = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document))
        {
            var mainPart = package.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());
            configure(body);
            mainPart.Document.Save();
        }

        return new DocxFixture(path);
    }

    public static Paragraph Paragraph(string text) =>
        new(new Run(new Text(text)));

    public static Paragraph ParagraphWithBreak(string before, string after) =>
        new(new Run(new Text(before), new Break(), new Text(after)));

    public static Table Table(params TableRow[] rows)
    {
        var maxColumns = rows.Length == 0
            ? 1
            : rows.Max(row => row.Elements<TableCell>().Sum(cell => cell.TableCellProperties?.GridSpan?.Val?.Value ?? 1));
        var grid = new TableGrid();
        for (var index = 0; index < Math.Max(1, maxColumns); index++)
        {
            grid.Append(new GridColumn { Width = "2400" });
        }

        var table = new Table();
        table.Append(grid);
        table.Append(rows);
        return table;
    }

    public static TableRow Row(params TableCell[] cells) =>
        new(cells);

    public static TableCell Cell(
        string text,
        int span = 1,
        MergedCellValues? verticalMerge = null,
        Table? nestedTable = null)
    {
        var cell = new TableCell();
        var properties = new TableCellProperties();
        if (span > 1)
        {
            properties.Append(new GridSpan { Val = span });
        }

        if (verticalMerge is not null)
        {
            properties.Append(new VerticalMerge { Val = verticalMerge.Value });
        }

        if (properties.ChildElements.Count > 0)
        {
            cell.Append(properties);
        }

        cell.Append(Paragraph(text));
        if (nestedTable is not null)
        {
            cell.Append(nestedTable);
            cell.Append(new Paragraph());
        }

        return cell;
    }
}
