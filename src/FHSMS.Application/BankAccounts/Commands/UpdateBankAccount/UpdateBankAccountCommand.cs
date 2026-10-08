using MediatR;

namespace FHSMS.Application.BankAccounts.Commands.UpdateBankAccount;

public record UpdateBankAccountCommand(Guid BankAccountId, string BankName, string AccountName, string AccountNumber, bool IsActive) : IRequest;
