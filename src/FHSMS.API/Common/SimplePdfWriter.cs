using System.Globalization;
using System.Text;

namespace FHSMS.API.Common;

/// <summary>
/// A minimal, dependency-free multi-page PDF writer for text/table content
/// (invoices, receipts, list exports). Implements just enough of the PDF 1.4
/// spec by hand - Helvetica, left-aligned text, fixed-position table cells -
/// to avoid pulling in a full PDF library for what is fundamentally "print
/// these lines and tables of text". Content is paginated automatically once
/// AddLine/AddRow calls exceed one page's height, so a 500-row export
/// produces a correct multi-page PDF instead of silently running off the
/// bottom of a single page. Not a general-purpose PDF engine - if richer
/// layout (images, real cell borders, wrapped text) is needed later, swap
/// this for QuestPDF or similar.
/// </summary>
public class SimplePdfWriter
{
    private const int PageWidth = 612;  // US Letter, points
    private const int PageHeight = 792;
    private const int LeftMargin = 50;
    private const int TopMargin = 60;
    private const int BottomMargin = 50;
    private const int LineHeight = 16;

    private record Cell(string Text, int X);
    private record Line(List<Cell> Cells, int FontSize, bool Bold);

    private readonly List<Line> _lines = new();

    public SimplePdfWriter AddLine(string text, int fontSize = 10, bool bold = false)
    {
        _lines.Add(new Line(new List<Cell> { new(text, LeftMargin) }, fontSize, bold));
        return this;
    }

    public SimplePdfWriter AddSpacer() => AddLine(string.Empty);

    /// <summary>One row of a table: each cell positioned at LeftMargin + columnX[i], so columns line up exactly regardless of Helvetica's non-monospace widths.</summary>
    public SimplePdfWriter AddRow(string[] cells, int[] columnX, int fontSize = 9, bool bold = false)
    {
        var line = new List<Cell>();
        for (var i = 0; i < cells.Length && i < columnX.Length; i++)
            line.Add(new Cell(cells[i], LeftMargin + columnX[i]));
        _lines.Add(new Line(line, fontSize, bold));
        return this;
    }

    public byte[] Build()
    {
        var usableHeight = PageHeight - TopMargin - BottomMargin;
        var linesPerPage = Math.Max(1, usableHeight / LineHeight);

        var pages = new List<List<Line>>();
        for (var i = 0; i < _lines.Count; i += linesPerPage)
            pages.Add(_lines.Skip(i).Take(linesPerPage).ToList());
        if (pages.Count == 0)
            pages.Add(new List<Line>());

        // Object numbering: 1=catalog, 2=pages, then one Page object per
        // page, then one content-stream object per page, then two shared
        // font objects (F1 regular, F2 bold) referenced by every page.
        var pageCount = pages.Count;
        var firstPageObj = 3;
        var firstContentObj = firstPageObj + pageCount;
        var fontRegularObj = firstContentObj + pageCount;
        var fontBoldObj = fontRegularObj + 1;

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>"
        };

        var kids = string.Join(" ", Enumerable.Range(0, pageCount).Select(i => $"{firstPageObj + i} 0 R"));
        objects.Add($"<< /Type /Pages /Kids [{kids}] /Count {pageCount} >>");

        for (var p = 0; p < pageCount; p++)
        {
            objects.Add(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth} {PageHeight}] " +
                $"/Resources << /Font << /F1 {fontRegularObj} 0 R /F2 {fontBoldObj} 0 R >> >> " +
                $"/Contents {firstContentObj + p} 0 R >>");
        }

        for (var p = 0; p < pageCount; p++)
        {
            var content = new StringBuilder();
            content.Append("BT\n");
            var y = PageHeight - TopMargin;

            foreach (var line in pages[p])
            {
                var font = line.Bold ? "/F2" : "/F1";
                content.Append($"{font} {line.FontSize} Tf\n");
                foreach (var cell in line.Cells)
                {
                    // Absolute text-matrix positioning (Tm), not relative Td
                    // chains - every cell/line places itself independently,
                    // so there's no cumulative-offset drift across a long page.
                    content.Append($"1 0 0 1 {cell.X} {y} Tm\n");
                    content.Append($"({Escape(cell.Text)}) Tj\n");
                }
                y -= LineHeight;
            }

            content.Append("ET\n");
            var contentBytes = Encoding.ASCII.GetBytes(content.ToString());
            objects.Add($"<< /Length {contentBytes.Length} >>\nstream\n{content}endstream");
        }

        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

        return AssemblePdf(objects);
    }

    private static string Escape(string text) =>
        text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static byte[] AssemblePdf(List<string> objects)
    {
        var sb = new StringBuilder();
        sb.Append("%PDF-1.4\n");
        var offsets = new List<int>();

        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
            sb.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }

        var xrefStart = Encoding.ASCII.GetByteCount(sb.ToString());
        sb.Append($"xref\n0 {objects.Count + 1}\n");
        sb.Append("0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            sb.Append($"{offset:D10} 00000 n \n");
        }

        sb.Append($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefStart}\n%%EOF");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }
}
