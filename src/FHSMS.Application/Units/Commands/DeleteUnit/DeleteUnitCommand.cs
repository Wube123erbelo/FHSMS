using MediatR;

namespace FHSMS.Application.Units.Commands.DeleteUnit;

public record DeleteUnitCommand(Guid UnitId) : IRequest;
