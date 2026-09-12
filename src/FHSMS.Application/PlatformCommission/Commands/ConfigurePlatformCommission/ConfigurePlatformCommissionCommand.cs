using MediatR;

namespace FHSMS.Application.PlatformCommission.Commands.ConfigurePlatformCommission;

/// <summary>
/// Creates the platform commission configuration the first time, or
/// schedules a new rate for the existing one, from Settings -> Total
/// Commission. This is the single admin-facing entry point for "the 2% must
/// be adjustable at any time" - never inlined or hard-coded elsewhere.
/// </summary>
public record ConfigurePlatformCommissionCommand(
    Guid? ExistingConfigurationId,
    string Name,
    bool IsEnabled,
    decimal Rate,
    DateTime EffectiveFrom) : IRequest<Guid>;
