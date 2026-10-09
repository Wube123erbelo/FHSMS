using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUsersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var onlineThreshold = DateTime.UtcNow.AddMinutes(-5);

        return await _context.Users
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Code = u.Code,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                Phone = u.Phone,
                Location = u.Location,
                IsActive = u.IsActive,
                IsOnline = u.LastSeenAt != null && u.LastSeenAt >= onlineThreshold,
                LastSeenAt = u.LastSeenAt,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
