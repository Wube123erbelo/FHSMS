using MediatR;

namespace FHSMS.Application.BankAccounts.Commands.CreateBankAccount;

public record CreateBankAccountCommand(string BankName, string AccountName, string AccountNumber) : IRequest<Guid>;
