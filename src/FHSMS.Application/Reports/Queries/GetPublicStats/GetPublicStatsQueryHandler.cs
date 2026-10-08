using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Reports.Queries.GetPublicStats;

public class GetPublicStatsQueryHandler : IRequestHandler<GetPublicStatsQuery, PublicStatsDto>
{
    private const int TrailingDays = 30;

    private readonly IApplicationDbContext _context;

    public GetPublicStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PublicStatsDto> Handle(GetPublicStatsQuery request, CancellationToken cancellationToken)
    {
        var registeredFarmers = await _context.Farmers.CountAsync(f => f.IsActive, cancellationToken);
        var registeredHotels = await _context.Customers.CountAsync(c => c.IsActive, cancellationToken);

        var since = DateTime.UtcNow.AddDays(-TrailingDays);

        var deliveredOrderIds = await _context.Deliveries
            .Where(d => d.Status == DeliveryStatus.Delivered && d.DeliveredAt != null && d.DeliveredAt >= since)
            .Select(d => d.OrderId)
            .ToListAsync(cancellationToken);

        var totalKgTrailing = await _context.OrderItems
            .Where(i => deliveredOrderIds.Contains(i.OrderId))
            .SumAsync(i => (decimal?)i.Quantity, cancellationToken) ?? 0m;

        var avgDailyKg = Math.Round(totalKgTrailing / TrailingDays, 1);

        var completedTrips = await _context.Deliveries
            .CountAsync(d => d.Status == DeliveryStatus.Delivered, cancellationToken);
        var failedTrips = await _context.Deliveries
            .CountAsync(d => d.Status == DeliveryStatus.Failed, cancellationToken);
        var finishedTrips = completedTrips + failedTrips;

        return new PublicStatsDto
        {
            RegisteredFarmers = registeredFarmers,
            RegisteredHotels = registeredHotels,
            AvgDailyKgDelivered = avgDailyKg,
            ServiceSatisfactionPercent = finishedTrips > 0
                ? Math.Round(100.0 * completedTrips / finishedTrips, 1)
                : null
        };
    }
}
