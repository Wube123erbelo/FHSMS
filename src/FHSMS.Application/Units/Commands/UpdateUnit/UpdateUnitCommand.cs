using MediatR;

namespace FHSMS.Application.Units.Commands.UpdateUnit;

public record UpdateUnitCommand(Guid UnitId, string Name, string Abbreviation, bool IsActive) : IRequest;
