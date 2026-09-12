using MediatR;

namespace FHSMS.Application.BankReconciliation.Commands.ReconcileTransaction;

/// <summary>Matches a bank statement line to a recorded Payment - the core reconciliation action.</summary>
public record ReconcileTransactionCommand(Guid BankTransactionId, Guid PaymentId) : IRequest;

public record MarkTransactionDisputedCommand(Guid BankTransactionId) : IRequest;
