using NexaCommerce.Domain.Entities.Identity;

namespace NexaCommerce.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string>? roles = null);
    string GenerateRefreshToken();
}
