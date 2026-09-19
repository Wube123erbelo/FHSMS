using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Tax.Commands.ConfigureTax;

/// <summary>
/// Creates a brand-new tax configuration (e.g. the first time VAT is set up), or
/// schedules a rate change for an existing one, from Settings -> Tax & VAT.
/// This is the only entry point that is allowed to introduce a tax rate into the
/// system - it is never inlined anywhere else.
/// </summary>
public record ConfigureTaxCommand(
    Guid? ExistingTaxConfigurationId,
    string Name,
    TaxType TaxType,
    bool IsEnabled,
    TaxCalculationMode CalculationMode,
    bool ExemptionAllowed,
    decimal Rate,
    DateTime EffectiveFrom) : IRequest<Guid>;
