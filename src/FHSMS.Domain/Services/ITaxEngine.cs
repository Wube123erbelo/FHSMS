using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;

namespace FHSMS.Domain.Services;

/// <summary>
/// The single place where "how much tax does this line owe" is decided. Nothing
/// outside this interface should ever compute tax - controllers, handlers, and
/// reports all call through here so the rule "never hard-code VAT = 15%" holds
/// everywhere in the codebase, not just in one screen.
/// </summary>
public interface ITaxEngine
{
    /// <summary>
    /// Calculates tax for one order/invoice line.
    /// </summary>
    /// <param name="quantity">Quantity being invoiced.</param>
    /// <param name="unitPrice">Price per unit at the time of the order.</param>
    /// <param name="productTaxProfile">The product's own tax treatment.</param>
    /// <param name="activeConfiguration">
    /// The currently active TaxConfiguration, or null if none exists / tax is
    /// not configured at all for this tax type.
    /// </param>
    /// <param name="activeRate">
    /// The TaxRate in force as of the calculation date (already resolved by the
    /// caller via TaxConfiguration.GetRateAsOf). Null when configuration is
    /// disabled or has no rate scheduled yet.
    /// </param>
    TaxCalculationResult Calculate(
        decimal quantity,
        decimal unitPrice,
        TaxProfileType productTaxProfile,
        TaxConfiguration? activeConfiguration,
        TaxRate? activeRate);
}
