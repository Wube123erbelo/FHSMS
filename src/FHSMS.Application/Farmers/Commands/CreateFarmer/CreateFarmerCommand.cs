using MediatR;

namespace FHSMS.Application.Farmers.Commands.CreateFarmer;

public record CreateFarmerCommand(
    string Name, string? ContactPerson, string? Phone, string? Location, string? BankAccountNumber) : IRequest<Guid>;
