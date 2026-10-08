using System.Globalization;

namespace FHSMS.API.Common;

/// <summary>
/// The PDF-format sibling of CsvExporter/ExcelExporter - same column-based
/// shape, so a controller wires all three exports identically:
///   CsvExporter.Write(rows, columns)
///   ExcelExporter.Write(title, rows, columns)
///   PdfTableExporter.Write(title, rows, columns)
/// Columns are given equal width across the page and long values are
/// truncated with an ellipsis rather than wrapped (SimplePdfWriter has no
/// text-wrapping), so this suits reference/report exports with a modest
/// number of columns - for anything wider, CSV/Excel remain the better fit
/// and are always offered alongside this.
/// </summary>
public static class PdfTableExporter
{
    private const int UsableWidth = 512; // 612pt page width - 50pt margins on each side

    public static byte[] Write<T>(string title, IEnumerable<T> rows, (string Header, Func<T, object?> Value)[] columns)
    {
        var pdf = new SimplePdfWriter();
        pdf.AddLine("FHSMS - Farm-Hotel Supply Management", 12, bold: true);
        pdf.AddLine(title, 11, bold: true);
        pdf.AddSpacer();

        var columnWidth = Math.Max(40, UsableWidth / columns.Length);
        var columnX = Enumerable.Range(0, columns.Length).Select(i => i * columnWidth).ToArray();
        var maxChars = Math.Max(4, columnWidth / 6); // rough character budget at 8-9pt Helvetica

        pdf.AddRow(columns.Select(c => Truncate(c.Header, maxChars)).ToArray(), columnX, fontSize: 9, bold: true);

        var rowCount = 0;
        foreach (var row in rows)
        {
            var cells = columns.Select(c => Truncate(FormatValue(c.Value(row)), maxChars)).ToArray();
            pdf.AddRow(cells, columnX, fontSize: 8);
            rowCount++;
        }

        if (rowCount == 0)
            pdf.AddLine("(no rows)", 9);

        return pdf.Build();
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        DateTime dt => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        decimal d => d.ToString("0.##", CultureInfo.InvariantCulture),
        bool b => b ? "Yes" : "No",
        _ => value.ToString() ?? string.Empty
    };

    private static string Truncate(string text, int maxChars)
        => text.Length > maxChars ? text[..Math.Max(1, maxChars - 1)] + "\u2026" : text;
}
