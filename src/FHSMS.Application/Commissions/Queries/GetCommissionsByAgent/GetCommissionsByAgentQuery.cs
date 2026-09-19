using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Commissions.Queries.GetCommissionsByAgent;

public record GetCommissionsByAgentQuery(Guid AgentUserId) : IRequest<List<CommissionDto>>;
