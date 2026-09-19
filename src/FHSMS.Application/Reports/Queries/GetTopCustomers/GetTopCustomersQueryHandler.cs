using FHSMS.Application.Common.Extensions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Reports.Queries.GetTopCustomers;

public class GetTopCustomersQueryHandler : IRequestHandler<GetTopCustomersQuery, List<TopCustomerDto>>
{
    private readonly IApplicationDbContext _context;
    public GetTopCustomersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<TopCustomerDto>> Handle(GetTopCustomersQuery request, CancellationToken cancellationToken)
    {
        var from = request.From.AsUtc() ?? DateTime.UtcNow.AddMonths(-1);
        var to = request.To.AsUtc() ?? DateTime.UtcNow;

        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var customerIds = invoices.Select(i => i.CustomerId).Distinct().ToList();
        var customers = await _context.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        return invoices
            .GroupBy(i => i.CustomerId)
            .Select(g => new TopCustomerDto
            {
                CustomerId = g.Key,
                CustomerName = customers.TryGetValue(g.Key, out var c) ? c.Name : "-",
                QuantityKg = g.SelectMany(i => i.Items).Sum(it => it.Quantity),
                Revenue = g.Sum(i => i.GrandTotal)
            })
            .OrderByDescending(c => c.Revenue)
            .Take(request.Top)
            .ToList();
    }
}
