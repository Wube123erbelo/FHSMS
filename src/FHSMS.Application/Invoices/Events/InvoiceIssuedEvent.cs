using FHSMS.Application.Common.Services;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Invoices.Events;

/// <summary>
/// Published after an invoice is successfully generated and saved. Anything that
/// should happen "because an invoice was issued" (commission accrual, customer
/// notification, future Telegram receipt, etc.) subscribes here instead of being
/// bolted onto GenerateInvoiceCommandHandler directly - keeps that handler focused
/// purely on tax/commission calculation and invoice persistence.
///
/// Bonus carries the ALREADY-COMPUTED hotel-agent bonus (see
/// HotelAgentBonusCalculator) so AccrueCommissionOnInvoiceIssued never
/// recomputes it independently - the number on the Invoice and the number on
/// the resulting Commission record can never drift apart.
/// </summary>
public record InvoiceIssuedEvent(
    Guid InvoiceId,
    Guid OrderId,
    Guid CustomerId,
    decimal GrandTotal,
    decimal Subtotal,
    Guid? AgentUserId,
    AgentType? AgentType,
    HotelAgentBonusResult Bonus) : INotification;
