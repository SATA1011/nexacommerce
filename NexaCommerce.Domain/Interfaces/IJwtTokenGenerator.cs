using NexaCommerce.Domain.Entities.Identity;

namespace NexaCommerce.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string>? roles = null, Guid? customerId = null);
    string GenerateRefreshToken();
}
