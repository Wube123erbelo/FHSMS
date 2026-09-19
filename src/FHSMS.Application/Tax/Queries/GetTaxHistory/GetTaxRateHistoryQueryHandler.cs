using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Tax.Queries.GetTaxHistory;

public class GetTaxRateHistoryQueryHandler : IRequestHandler<GetTaxRateHistoryQuery, List<TaxRateHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTaxRateHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaxRateHistoryDto>> Handle(GetTaxRateHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _context.TaxRates
            .Where(r => r.TaxConfigurationId == request.TaxConfigurationId)
            .OrderByDescending(r => r.EffectiveFrom)
            .Select(r => new TaxRateHistoryDto
            {
                Id = r.Id,
                Rate = r.Rate,
                EffectiveFrom = r.EffectiveFrom,
                EffectiveTo = r.EffectiveTo
            })
            .ToListAsync(cancellationToken);
    }
}
