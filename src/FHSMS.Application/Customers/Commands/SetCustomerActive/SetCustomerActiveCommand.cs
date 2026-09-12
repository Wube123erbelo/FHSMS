using MediatR;

namespace FHSMS.Application.Customers.Commands.SetCustomerActive;

public record SetCustomerActiveCommand(Guid CustomerId, bool IsActive) : IRequest;
