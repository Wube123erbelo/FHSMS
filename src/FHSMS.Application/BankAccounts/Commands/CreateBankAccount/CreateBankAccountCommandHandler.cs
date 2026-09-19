using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;

namespace FHSMS.Application.BankAccounts.Commands.CreateBankAccount;

public class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateBankAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var account = new BankAccount
        {
            BankName = request.BankName,
            AccountName = request.AccountName,
            AccountNumber = request.AccountNumber
        };
        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account.Id;
    }
}
