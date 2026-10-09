using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;

namespace FHSMS.Domain.Services;

/// <summary>
/// Default implementation of the configurable tax engine described in
/// Settings -> Tax & VAT. This class deliberately has ZERO dependency on EF Core,
/// HTTP, or any infrastructure concern - it is pure domain logic, consistent with
/// Clean Architecture keeping business rules independent of infrastructure.
///
/// Decision tree:
///   1. No active configuration, or configuration disabled  -> no tax at all.
///   2. Product profile is TaxExempt or NoTax                -> no tax on this line,
///      even though tax is enabled globally.
///   3. Product profile is ZeroRated                          -> taxable in principle,
///      but the effective rate is forced to 0%.
///   4. Otherwise (StandardVat)                                -> apply the active rate,
///      either inclusive or exclusive per the configuration's CalculationMode.
/// </summary>
public class TaxEngine : ITaxEngine
{
    public TaxCalculationResult Calculate(
        decimal quantity,
        decimal unitPrice,
        TaxProfileType productTaxProfile,
        TaxConfiguration? activeConfiguration,
        TaxRate? activeRate)
    {
        var lineSubtotal = Math.Round(quantity * unitPrice, 2);

        // 1. Tax not configured or explicitly disabled by the Admin.
        if (activeConfiguration is null || !activeConfiguration.IsEnabled || activeRate is null)
        {
            return new TaxCalculationResult(
                LineSubtotal: lineSubtotal,
                TaxProfileApplied: productTaxProfile,
                TaxRateApplied: 0m,
                TaxableAmount: lineSubtotal,
                TaxAmount: 0m,
                LineTotal: lineSubtotal);
        }

        // 2. Product itself is outside the tax net regardless of global config.
        if (productTaxProfile is TaxProfileType.TaxExempt or TaxProfileType.NoTax)
        {
            return new TaxCalculationResult(
                LineSubtotal: lineSubtotal,
                TaxProfileApplied: productTaxProfile,
                TaxRateApplied: 0m,
                TaxableAmount: lineSubtotal,
                TaxAmount: 0m,
                LineTotal: lineSubtotal);
        }

        // 3. Zero-rated: taxable category, 0% rate.
        var effectiveRate = productTaxProfile == TaxProfileType.ZeroRated
            ? 0m
            : activeRate.Rate;

        decimal taxableAmount;
        decimal taxAmount;
        decimal lineTotal;

        if (activeConfiguration.CalculationMode == TaxCalculationMode.Inclusive)
        {
            // Price already contains tax: back it out.
            var divisor = 1 + (effectiveRate / 100m);
            taxableAmount = Math.Round(lineSubtotal / divisor, 2);
            taxAmount = Math.Round(lineSubtotal - taxableAmount, 2);
            lineTotal = lineSubtotal;
        }
        else
        {
            // Exclusive: tax is added on top of the subtotal.
            taxableAmount = lineSubtotal;
            taxAmount = Math.Round(lineSubtotal * (effectiveRate / 100m), 2);
            lineTotal = Math.Round(lineSubtotal + taxAmount, 2);
        }

        return new TaxCalculationResult(
            LineSubtotal: lineSubtotal,
            TaxProfileApplied: productTaxProfile,
            TaxRateApplied: effectiveRate,
            TaxableAmount: taxableAmount,
            TaxAmount: taxAmount,
            LineTotal: lineTotal);
    }
}
