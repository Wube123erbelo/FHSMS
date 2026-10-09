using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.BankAccounts.Queries.GetBankAccounts;

public record GetBankAccountsQuery : IRequest<List<BankAccountDto>>;
