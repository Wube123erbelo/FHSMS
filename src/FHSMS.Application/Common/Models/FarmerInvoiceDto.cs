using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

/// <summary>The buying-side counterpart to InvoiceDto - what the company owes a farmer for one stock receipt.</summary>
public class FarmerInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public Guid InventoryTransactionId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? UnitAbbreviation { get; set; }
    public Guid? FarmerId { get; set; }
    public string? FarmerName { get; set; }
    public Guid? AgentUserId { get; set; }
    public string? AgentName { get; set; }
    public decimal Quantity { get; set; }
    public decimal BuyingPriceApplied { get; set; }
    public decimal TotalAmount { get; set; }
    public FarmerInvoiceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
