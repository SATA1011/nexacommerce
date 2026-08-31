using NexaCommerce.Domain.Entities.Identity;

namespace NexaCommerce.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<RefreshToken> InsertOrUpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(string tokenHash, string revokedByIp, string? replacedByTokenHash = null, string? reasonRevoked = null, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
