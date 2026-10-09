using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Customers.Queries.GetCustomers;

public record GetCustomersQuery : IRequest<List<CustomerDto>>;
