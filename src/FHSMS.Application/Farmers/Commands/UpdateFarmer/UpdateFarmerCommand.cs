using MediatR;

namespace FHSMS.Application.Farmers.Commands.UpdateFarmer;

public record UpdateFarmerCommand(
    Guid FarmerId, string Name, string? ContactPerson, string? Phone, string? Location, string? BankAccountNumber, bool IsActive)
    : IRequest;
