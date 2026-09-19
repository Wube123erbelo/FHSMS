using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Commissions.Queries.GetMyCommissions;

public class GetMyCommissionsQueryHandler : IRequestHandler<GetMyCommissionsQuery, List<CommissionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyCommissionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CommissionDto>> Handle(GetMyCommissionsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId) return new List<CommissionDto>();

        return await _context.Commissions
            .Where(c => c.AgentUserId == userId)
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
