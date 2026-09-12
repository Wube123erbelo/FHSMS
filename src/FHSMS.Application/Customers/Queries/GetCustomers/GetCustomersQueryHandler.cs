using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, List<CustomerDto>>
{
    private readonly IApplicationDbContext _context;
    public GetCustomersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .OrderBy(c => c.Code)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ContactPerson = c.ContactPerson,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                CreditLimit = c.CreditLimit,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
