using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.PlatformCommission.Queries.GetPlatformCommissionHistory;

/// <summary>Full effective-dated rate history for the platform commission configuration - useful for audits and reports.</summary>
public record GetPlatformCommissionHistoryQuery(Guid ConfigurationId) : IRequest<List<PlatformCommissionRateHistoryDto>>;
