using InternalPortal.Application.Common.Interfaces;

namespace InternalPortal.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(11));
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
