using MediatR;

namespace FHSMS.Application.Reports.Queries.GetTopCustomers;

/// <summary>The dashboard's "Top Hotels" leaderboard.</summary>
public record GetTopCustomersQuery(DateTime? From, DateTime? To, int Top = 5) : IRequest<List<TopCustomerDto>>;

public class TopCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public decimal QuantityKg { get; set; }
    public decimal Revenue { get; set; }
}
