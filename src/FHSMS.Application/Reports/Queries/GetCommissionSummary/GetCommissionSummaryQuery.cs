using MediatR;

namespace FHSMS.Application.Reports.Queries.GetCommissionSummary;

public record GetCommissionSummaryQuery(DateTime? From, DateTime? To) : IRequest<List<CommissionSummaryDto>>;

public class CommissionSummaryDto
{
    public string AgentType { get; set; } = default!;
    public int CommissionCount { get; set; }
    public decimal TotalCommission { get; set; }
}
