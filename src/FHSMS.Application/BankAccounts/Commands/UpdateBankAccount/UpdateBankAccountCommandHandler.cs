using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.BankAccounts.Commands.UpdateBankAccount;

public class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateBankAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Id == request.BankAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(BankAccount), request.BankAccountId);

        account.BankName = request.BankName;
        account.AccountName = request.AccountName;
        account.AccountNumber = request.AccountNumber;
        account.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
