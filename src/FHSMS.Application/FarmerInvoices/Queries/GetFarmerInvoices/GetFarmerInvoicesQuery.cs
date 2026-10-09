using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.FarmerInvoices.Queries.GetFarmerInvoices;

/// <summary>
/// Role-scoped in the handler, same principle as GetInvoicesQuery will be for
/// the selling side (Phase 4): a FarmerAgent only ever gets back invoices
/// they themselves logged, regardless of what FarmerId/AgentUserId is passed
/// here - SuperAdmin and Driver see everything.
/// </summary>
public record GetFarmerInvoicesQuery(Guid? FarmerId = null, FarmerInvoiceStatus? Status = null) : IRequest<List<FarmerInvoiceDto>>;
