using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Commissions.Commands.CalculateCommission;

/// <summary>
/// Accrues a commission for the agent who placed an order, based on the
/// resulting invoice's grand total and the currently configured rate for
/// that agent type. Called after GenerateInvoiceCommand when the order has
/// an AgentUserId.
/// </summary>
public record CalculateCommissionCommand(Guid InvoiceId, Guid AgentUserId, AgentType AgentType) : IRequest<Guid?>;
