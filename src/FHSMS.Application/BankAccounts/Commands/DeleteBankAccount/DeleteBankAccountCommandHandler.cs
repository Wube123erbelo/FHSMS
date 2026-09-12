using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.BankAccounts.Commands.DeleteBankAccount;

public class DeleteBankAccountCommandHandler : IRequestHandler<DeleteBankAccountCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteBankAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteBankAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Id == request.BankAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(BankAccount), request.BankAccountId);

        account.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
