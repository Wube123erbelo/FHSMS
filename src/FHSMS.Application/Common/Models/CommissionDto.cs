using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class CommissionDto
{
    public Guid Id { get; set; }
    public Guid AgentUserId { get; set; }
    public CommissionSourceType SourceType { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? InventoryTransactionId { get; set; }
    public CommissionBasis Basis { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal? Percentage { get; set; }
    public decimal? FlatRateAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public CommissionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CommissionUnitRateDto
{
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = default!;
    public string UnitAbbreviation { get; set; } = default!;
    public decimal RateAmount { get; set; }
}

public class CommissionRuleDto
{
    public Guid Id { get; set; }
    public AgentType AgentType { get; set; }
    public CommissionBasis Basis { get; set; }
    public decimal? Percentage { get; set; }
    public decimal? FlatRateAmount { get; set; }
    public bool IsActive { get; set; }
    public List<CommissionUnitRateDto> UnitRates { get; set; } = new();
}
