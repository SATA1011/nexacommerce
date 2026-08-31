using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Identity;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Identity;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<RefreshToken>(
            StoredProcedure.RefreshTokensGet,
            new { p_token_hash = tokenHash },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<RefreshToken> InsertOrUpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = refreshToken.Id.ToString(),
            user_id = refreshToken.UserId.ToString(),
            token_hash = refreshToken.TokenHash,
            expires_at_utc = refreshToken.ExpiresAtUtc.ToString("yyyy-MM-dd HH:mm:ss.ffffff"),
            created_by_ip = refreshToken.CreatedByIp
        });

        return await connection.QuerySingleAsync<RefreshToken>(
            StoredProcedure.RefreshTokensInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task RevokeAsync(string tokenHash, string revokedByIp, string? replacedByTokenHash = null, string? reasonRevoked = null, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.RefreshTokensRevoke,
            new
            {
                p_token_hash = tokenHash,
                p_revoked_by_ip = revokedByIp,
                p_replaced_by_token_hash = replacedByTokenHash,
                p_reason_revoked = reasonRevoked
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.RefreshTokensDeleteExpired,
            commandType: CommandType.StoredProcedure
        );
    }
}
