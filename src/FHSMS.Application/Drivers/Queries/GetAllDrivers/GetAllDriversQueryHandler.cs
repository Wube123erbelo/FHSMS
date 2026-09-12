using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Drivers.Queries.GetAllDrivers;

public class GetAllDriversQueryHandler : IRequestHandler<GetAllDriversQuery, List<AdminDriverDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAllDriversQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AdminDriverDto>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
    {
        var drivers = await _context.Drivers.OrderBy(d => d.FullName).ToListAsync(cancellationToken);
        var deliveries = await _context.Deliveries
            .Where(d => d.DriverId != null)
            .ToListAsync(cancellationToken);

        return drivers.Select(driver =>
        {
            var trips = deliveries.Where(d => d.DriverId == driver.Id).ToList();
            var completed = trips.Where(d => d.Status == DeliveryStatus.Delivered).ToList();
            return new AdminDriverDto
            {
                Id = driver.Id,
                Code = driver.Code,
                FullName = driver.FullName,
                Phone = driver.Phone,
                PlateNumber = driver.PlateNumber,
                TruckType = driver.TruckType.ToString(),
                IsActive = driver.IsActive,
                PendingTripCount = trips.Count(d => d.Status is DeliveryStatus.Pending or DeliveryStatus.InTransit),
                CompletedTripCount = completed.Count,
                TotalEarned = completed.Sum(d => d.TripPrice ?? 0)
            };
        }).ToList();
    }
}
