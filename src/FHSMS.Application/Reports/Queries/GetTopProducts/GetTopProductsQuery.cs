using MediatR;

namespace FHSMS.Application.Reports.Queries.GetTopProducts;

public record GetTopProductsQuery(DateTime? From, DateTime? To, int Top = 10) : IRequest<List<TopProductDto>>;

public class TopProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
