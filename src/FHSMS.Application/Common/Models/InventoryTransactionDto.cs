using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class InventoryTransactionDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public InventoryTransactionType Type { get; set; }
    public decimal QuantityChange { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? FarmerId { get; set; }
    public string? FarmerName { get; set; }
    public Guid? AgentUserId { get; set; }
    public string? AgentName { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public bool FarmerPaymentConfirmed { get; set; }
    public decimal? AmountPaidToFarmer { get; set; }
    // The auto-generated farmer-side invoice for this Receiving transaction (see FarmerInvoice) - null for non-Receiving transaction types.
    public Guid? FarmerInvoiceId { get; set; }
    public string? FarmerInvoiceNumber { get; set; }
    public FarmerInvoiceStatus? FarmerInvoiceStatus { get; set; }
    public decimal? FarmerInvoiceTotalAmount { get; set; }
}
