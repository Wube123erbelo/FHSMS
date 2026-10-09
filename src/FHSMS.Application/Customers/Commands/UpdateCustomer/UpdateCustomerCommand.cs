using MediatR;

namespace FHSMS.Application.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid CustomerId, string Name, string? ContactPerson, string? Phone, string? Email, string? Address,
    decimal? CreditLimit, bool IsActive) : IRequest;
