using MediatR;

namespace FHSMS.Application.Farmers.Commands.SetFarmerActive;

public record SetFarmerActiveCommand(Guid FarmerId, bool IsActive) : IRequest;
