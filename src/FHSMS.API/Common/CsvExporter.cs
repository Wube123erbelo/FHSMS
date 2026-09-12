using System.Globalization;
using System.Text;

namespace FHSMS.API.Common;

/// <summary>
/// Minimal, dependency-free CSV writer used by every "/export/csv" endpoint.
/// Handles quoting/escaping per RFC 4180 (commas, quotes, newlines) without
/// pulling in a full CSV library for what is a genuinely simple format.
/// </summary>
public static class CsvExporter
{
    public static byte[] Write<T>(IEnumerable<T> rows, (string Header, Func<T, object?> Value)[] columns)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", columns.Select(c => Escape(c.Header))));

        foreach (var row in rows)
        {
            var values = columns.Select(c => Escape(FormatValue(c.Value(row))));
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        decimal d => d.ToString("0.####", CultureInfo.InvariantCulture),
        bool b => b ? "Yes" : "No",
        _ => value.ToString() ?? string.Empty
    };

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }
}
