using MediatR;

namespace FHSMS.Application.Reports.Queries.GetPublicStats;

/// <summary>
/// Backs the public landing page's "What We've Achieved So Far" band -
/// anonymous, read-only, and deliberately narrow (counts and one rolling
/// average, nothing identifying) since it's served with no authentication.
/// </summary>
public record GetPublicStatsQuery : IRequest<PublicStatsDto>;

public class PublicStatsDto
{
    public int RegisteredFarmers { get; set; }
    public int RegisteredHotels { get; set; }
    /// <summary>Average kg delivered per day, averaged over the trailing 30 days.</summary>
    public decimal AvgDailyKgDelivered { get; set; }
    /// <summary>Share of completed trips (Delivered vs. Delivered+Failed) as a 0-100 percentage. Null until there's at least one completed or failed trip to measure - shown before then rather than a misleading 0%.</summary>
    public double? ServiceSatisfactionPercent { get; set; }
}
