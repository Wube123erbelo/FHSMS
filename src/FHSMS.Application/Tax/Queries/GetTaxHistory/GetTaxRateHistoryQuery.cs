using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Tax.Queries.GetTaxHistory;

/// <summary>Full effective-dated rate history for one tax configuration - useful for audits and reports.</summary>
public record GetTaxRateHistoryQuery(Guid TaxConfigurationId) : IRequest<List<TaxRateHistoryDto>>;
