using System.Text;

namespace FHSMS.API.Common;

/// <summary>
/// "Excel export" without a real .xlsx (OOXML zip) writer: emits an HTML
/// table with the legacy "application/vnd.ms-excel" content type, which
/// Excel, LibreOffice Calc, and Google Sheets all open directly and treat as
/// a spreadsheet - a well-known, broadly compatible lightweight technique.
/// For a true .xlsx (e.g. if formulas/formatting are needed), swap this for
/// ClosedXML or EPPlus.
/// </summary>
public static class ExcelExporter
{
    public static byte[] Write<T>(string sheetTitle, IEnumerable<T> rows, (string Header, Func<T, object?> Value)[] columns)
    {
        var sb = new StringBuilder();
        sb.Append("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
        sb.Append($"<head><meta charset=\"UTF-8\"/><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
        sb.Append($"<x:Name>{Escape(sheetTitle)}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions>");
        sb.Append("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body>");
        sb.Append("<table border=\"1\"><thead><tr>");

        foreach (var col in columns)
            sb.Append($"<th>{Escape(col.Header)}</th>");
        sb.Append("</tr></thead><tbody>");

        foreach (var row in rows)
        {
            sb.Append("<tr>");
            foreach (var col in columns)
                sb.Append($"<td>{Escape(FormatValue(col.Value(row)))}</td>");
            sb.Append("</tr>");
        }

        sb.Append("</tbody></table></body></html>");

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
        decimal d => d.ToString("0.####"),
        bool b => b ? "Yes" : "No",
        _ => value.ToString() ?? string.Empty
    };

    private static string Escape(string value) =>
        value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
