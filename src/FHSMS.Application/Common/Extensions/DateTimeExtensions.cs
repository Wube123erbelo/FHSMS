namespace FHSMS.Application.Common.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Normalizes a DateTime to Kind=Utc without shifting the clock time.
    /// Necessary before using any DateTime in an EF Core query against
    /// Npgsql/PostgreSQL: query-string-bound dates (e.g. ?from=2026-07-01)
    /// arrive as Kind=Unspecified, and Npgsql throws
    /// "Cannot write DateTime with Kind=Unspecified to PostgreSQL type
    /// 'timestamp with time zone', only UTC is supported" the moment one is
    /// used in a comparison - this is what was behind the Reports page's
    /// "an unexpected error occurred" on every date-range Apply.
    /// </summary>
    public static DateTime AsUtc(this DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public static DateTime? AsUtc(this DateTime? value) => value?.AsUtc();
}
