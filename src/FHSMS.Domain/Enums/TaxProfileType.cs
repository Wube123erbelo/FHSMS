namespace FHSMS.Domain.Enums;

/// <summary>
/// Per-product tax treatment. A single active TaxConfiguration (e.g. VAT 15%) can
/// still apply differently line-by-line depending on the product's profile:
///   StandardVat - taxed at the configured rate
///   ZeroRated   - taxable in principle but the rate is 0% (still reported as taxable)
///   TaxExempt   - never taxed, regardless of configuration
///   NoTax       - tax simply does not apply to this product
/// </summary>
public enum TaxProfileType
{
    StandardVat = 1,
    ZeroRated = 2,
    TaxExempt = 3,
    NoTax = 4
}
