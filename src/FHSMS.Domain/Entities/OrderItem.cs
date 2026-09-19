using FHSMS.Domain.Common;

namespace FHSMS.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Math.Round(Quantity * UnitPrice, 2);

    private OrderItem() { } // EF Core

    public OrderItem(Guid orderId, Guid productId, string productName, decimal quantity, decimal unitPrice)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
