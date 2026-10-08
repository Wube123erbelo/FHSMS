using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.DriverPayments.Queries.GetDriverPayments;

public class GetDriverPaymentsQueryHandler : IRequestHandler<GetDriverPaymentsQuery, List<DriverPaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetDriverPaymentsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<DriverPaymentDto>> Handle(GetDriverPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DriverPayments.AsQueryable();

        // A driver only ever sees payment records for their own trips -
        // same principle as GetFarmerInvoicesQueryHandler's FarmerAgent
        // scoping. SuperAdmin sees across all drivers.
        if (_currentUser.Role == "Driver")
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);
            query = query.Where(p => p.DriverId == (driver != null ? driver.Id : Guid.Empty));
        }

        if (request.Status is { } status)
            query = query.Where(p => p.Status == status);

        var payments = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);

        var deliveryIds = payments.Select(p => p.DeliveryId).ToList();
        var deliveries = await _context.Deliveries
            .Where(d => deliveryIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        return payments.Select(p =>
        {
            deliveries.TryGetValue(p.DeliveryId, out var delivery);
            return new DriverPaymentDto
            {
                Id = p.Id,
                DeliveryId = p.DeliveryId,
                OrderId = delivery?.OrderId,
                DestinationAddress = delivery?.DestinationAddress,
                DriverId = p.DriverId,
                DriverName = p.DriverName,
                Amount = p.Amount,
                Status = p.Status,
                DriverWasPaid = p.DriverWasPaid,
                CreatedAt = p.CreatedAt,
                ApprovedAt = p.ApprovedAt
            };
        }).ToList();
    }
}
