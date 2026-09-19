using System.Security.Cryptography;
using FHSMS.Application.Common.Interfaces;

namespace FHSMS.Infrastructure.Services;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
