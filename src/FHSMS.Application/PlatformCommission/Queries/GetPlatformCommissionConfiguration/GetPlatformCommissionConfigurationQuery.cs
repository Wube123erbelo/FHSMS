using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.PlatformCommission.Queries.GetPlatformCommissionConfiguration;

/// <summary>Returns the platform commission configuration (there is exactly one) with its currently-in-force rate.</summary>
public record GetPlatformCommissionConfigurationQuery : IRequest<PlatformCommissionConfigurationDto?>;
