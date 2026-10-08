using MediatR;

namespace FHSMS.Application.Tax.Commands.UpdateVatToggle;

/// <summary>
/// The literal "VAT Enabled [ ON/OFF ]" switch from the Settings screen. Kept as
/// its own tiny command (rather than folding into ConfigureTaxCommand) because
/// toggling is a one-click, no-form action in the UI.
/// </summary>
public record SetTaxEnabledCommand(Guid TaxConfigurationId, bool IsEnabled) : IRequest;
