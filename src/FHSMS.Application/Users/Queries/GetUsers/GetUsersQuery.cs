using MediatR;

namespace FHSMS.Application.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<List<UserDto>>;

public class UserDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Seen active in the last 5 minutes - see the presence-tracking middleware in Program.cs.</summary>
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
