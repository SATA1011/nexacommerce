using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Identity;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Identity;

public sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserSessionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<UserSession>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<UserSession>(
            StoredProcedure.UserSessionsGet,
            new { p_user_id = userId.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<UserSession> InsertOrUpdateAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = session.Id.ToString(),
            user_id = session.UserId.ToString(),
            device_name = session.DeviceName,
            ip_address = session.IpAddress,
            user_agent = session.UserAgent
        });

        return await connection.QuerySingleAsync<UserSession>(
            StoredProcedure.UserSessionsInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.UserSessionsRevokeAll,
            new { p_user_id = userId.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }
}
