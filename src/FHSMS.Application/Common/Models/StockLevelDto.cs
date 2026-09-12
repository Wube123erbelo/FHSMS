namespace FHSMS.Application.Common.Models;

public class StockLevelDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? UnitAbbreviation { get; set; }
    public decimal QuantityOnHand { get; set; }
}
