using MediatR;

namespace FHSMS.Application.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid CustomerId) : IRequest;
