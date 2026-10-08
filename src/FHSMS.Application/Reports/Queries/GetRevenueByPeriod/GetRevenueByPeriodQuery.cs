using MediatR;

namespace FHSMS.Application.Reports.Queries.GetRevenueByPeriod;

public enum ReportGrouping { Day, Month }

public record GetRevenueByPeriodQuery(DateTime? From, DateTime? To, ReportGrouping GroupBy = ReportGrouping.Day)
    : IRequest<List<RevenuePeriodDto>>;

public class RevenuePeriodDto
{
    public string Period { get; set; } = default!;
    public decimal Revenue { get; set; }
    public decimal TaxCollected { get; set; }
    public int InvoiceCount { get; set; }
}
