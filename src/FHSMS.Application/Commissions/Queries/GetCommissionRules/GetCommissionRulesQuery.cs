using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Commissions.Queries.GetCommissionRules;

public record GetCommissionRulesQuery : IRequest<List<CommissionRuleDto>>;
