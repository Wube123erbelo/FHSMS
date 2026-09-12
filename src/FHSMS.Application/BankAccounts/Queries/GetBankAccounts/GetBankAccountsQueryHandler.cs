using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.BankAccounts.Queries.GetBankAccounts;

public class GetBankAccountsQueryHandler : IRequestHandler<GetBankAccountsQuery, List<BankAccountDto>>
{
    private readonly IApplicationDbContext _context;
    public GetBankAccountsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<BankAccountDto>> Handle(GetBankAccountsQuery request, CancellationToken cancellationToken)
    {
        return await _context.BankAccounts
            .OrderBy(b => b.BankName)
            .Select(b => new BankAccountDto
            {
                Id = b.Id, BankName = b.BankName, AccountName = b.AccountName, AccountNumber = b.AccountNumber, IsActive = b.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
