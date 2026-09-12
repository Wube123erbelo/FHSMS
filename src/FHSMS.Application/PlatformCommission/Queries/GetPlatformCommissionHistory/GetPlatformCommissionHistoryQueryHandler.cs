using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.PlatformCommission.Queries.GetPlatformCommissionHistory;

public class GetPlatformCommissionHistoryQueryHandler
    : IRequestHandler<GetPlatformCommissionHistoryQuery, List<PlatformCommissionRateHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPlatformCommissionHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlatformCommissionRateHistoryDto>> Handle(
        GetPlatformCommissionHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _context.PlatformCommissionRates
            .Where(r => r.PlatformCommissionConfigurationId == request.ConfigurationId)
            .OrderByDescending(r => r.EffectiveFrom)
            .Select(r => new PlatformCommissionRateHistoryDto
            {
                Id = r.Id,
                Rate = r.Rate,
                EffectiveFrom = r.EffectiveFrom,
                EffectiveTo = r.EffectiveTo
            })
            .ToListAsync(cancellationToken);
    }
}
