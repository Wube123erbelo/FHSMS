using MediatR;

namespace FHSMS.Application.Units.Commands.CreateUnit;

public record CreateUnitCommand(string Name, string Abbreviation) : IRequest<Guid>;
