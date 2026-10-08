namespace FHSMS.Domain.Enums;

/// <summary>Every kind of stock movement the warehouse can record.</summary>
public enum InventoryTransactionType
{
    Receiving = 1,
    Issuing = 2,
    Adjustment = 3,
    Damage = 4,
    Wastage = 5
}
