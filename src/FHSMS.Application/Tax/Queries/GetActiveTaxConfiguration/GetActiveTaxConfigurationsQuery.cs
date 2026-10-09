using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Tax.Queries.GetActiveTaxConfiguration;

/// <summary>Returns every active tax configuration (VAT, Withholding, etc.) with its currently-in-force rate.</summary>
public record GetActiveTaxConfigurationsQuery : IRequest<List<TaxConfigurationDto>>;
