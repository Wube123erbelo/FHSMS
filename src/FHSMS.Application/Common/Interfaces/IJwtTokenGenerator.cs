using FHSMS.Domain.Entities;

namespace FHSMS.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
