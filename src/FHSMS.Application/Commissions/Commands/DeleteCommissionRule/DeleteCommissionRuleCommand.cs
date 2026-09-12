using MediatR;

namespace FHSMS.Application.Commissions.Commands.DeleteCommissionRule;

public record DeleteCommissionRuleCommand(Guid CommissionRuleId) : IRequest;
