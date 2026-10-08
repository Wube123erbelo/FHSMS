using FHSMS.Application.Common.Extensions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Reports.Queries.GetRevenueByPeriod;

/// <summary>
/// Time-bucketed revenue for charting. Grouping happens in memory rather than
/// via a SQL GROUP BY on a truncated date, since that truncation is
/// provider-specific (Npgsql's date_trunc vs SQL Server's DATEPART) and this
/// keeps the query portable at the cost of pulling the period's invoices into
/// memory first - fine at reporting-dashboard scale.
/// </summary>
public class GetRevenueByPeriodQueryHandler : IRequestHandler<GetRevenueByPeriodQuery, List<RevenuePeriodDto>>
{
    private readonly IApplicationDbContext _context;
    public GetRevenueByPeriodQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<RevenuePeriodDto>> Handle(GetRevenueByPeriodQuery request, CancellationToken cancellationToken)
    {
        var from = request.From.AsUtc() ?? DateTime.UtcNow.AddMonths(-6);
        var to = request.To.AsUtc() ?? DateTime.UtcNow;

        var invoices = await _context.Invoices
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var grouped = invoices
            .GroupBy(i => request.GroupBy == ReportGrouping.Month
                ? new DateTime(i.InvoiceDate.Year, i.InvoiceDate.Month, 1)
                : i.InvoiceDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new RevenuePeriodDto
            {
                Period = request.GroupBy == ReportGrouping.Month ? g.Key.ToString("yyyy-MM") : g.Key.ToString("yyyy-MM-dd"),
                Revenue = g.Sum(i => i.GrandTotal),
                TaxCollected = g.Sum(i => i.TaxAmount),
                InvoiceCount = g.Count()
            })
            .ToList();

        return grouped;
    }
}
