namespace FHSMS.Domain.Enums;

/// <summary>
/// Whether the configured rate is already baked into the product price (Inclusive)
/// or should be added on top of it (Exclusive). Configured per TaxConfiguration in
/// Settings -> Tax & VAT.
/// </summary>
public enum TaxCalculationMode
{
    Exclusive = 1,
    Inclusive = 2
}
