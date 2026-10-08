using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Units.Queries.GetUnits;

public record GetUnitsQuery : IRequest<List<UnitDto>>;
