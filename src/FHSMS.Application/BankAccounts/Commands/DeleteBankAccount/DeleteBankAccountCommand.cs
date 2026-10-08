using MediatR;

namespace FHSMS.Application.BankAccounts.Commands.DeleteBankAccount;

public record DeleteBankAccountCommand(Guid BankAccountId) : IRequest;
