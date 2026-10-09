using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Commissions.Queries.GetCommissionsByAgent;

public class GetCommissionsByAgentQueryHandler : IRequestHandler<GetCommissionsByAgentQuery, List<CommissionDto>>
{
    private readonly IApplicationDbContext _context;
    public GetCommissionsByAgentQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<CommissionDto>> Handle(GetCommissionsByAgentQuery request, CancellationToken cancellationToken)
    {
        return await _context.Commissions
            .Where(c => c.AgentUserId == request.AgentUserId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommissionDto
            {
                Id = c.Id,
                AgentUserId = c.AgentUserId,
                SourceType = c.SourceType,
                InvoiceId = c.InvoiceId,
                InventoryTransactionId = c.InventoryTransactionId,
                Basis = c.Basis,
                BaseAmount = c.BaseAmount,
                Percentage = c.Percentage,
                FlatRateAmount = c.FlatRateAmount,
                CommissionAmount = c.CommissionAmount,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
