using FHSMS.Domain.Enums;

namespace FHSMS.Domain.Services;

/// <summary>Pure calculation output for a single invoice line - no persistence, no side effects.</summary>
public record TaxCalculationResult(
    decimal LineSubtotal,
    TaxProfileType TaxProfileApplied,
    decimal TaxRateApplied,
    decimal TaxableAmount,
    decimal TaxAmount,
    decimal LineTotal);
