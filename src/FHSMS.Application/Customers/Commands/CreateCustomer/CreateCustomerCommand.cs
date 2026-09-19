using MediatR;

namespace FHSMS.Application.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    decimal? CreditLimit) : IRequest<Guid>;
