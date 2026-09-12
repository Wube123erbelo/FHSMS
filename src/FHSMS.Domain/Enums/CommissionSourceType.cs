namespace FHSMS.Domain.Enums;

/// <summary>What business event generated a Commission record.</summary>
public enum CommissionSourceType
{
    /// <summary>A hotel agent's order resulted in an issued invoice (PercentageOfInvoice basis).</summary>
    Invoice = 1,

    /// <summary>A farmer agent recorded stock received from a farmer (FlatRatePerQuantity basis).</summary>
    StockReceipt = 2
}
