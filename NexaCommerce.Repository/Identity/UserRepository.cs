using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Identity;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Identity;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            StoredProcedure.UsersGet,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            StoredProcedure.UsersGetByEmail,
            new { p_email = email },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            StoredProcedure.UsersGetAll,
            new { p_search_term = searchTerm, p_page_number = pageNumber, p_page_size = pageSize },
            commandType: CommandType.StoredProcedure
        );

        var totalCount = await multi.ReadSingleAsync<int>();
        var users = await multi.ReadAsync<User>();

        return (users, totalCount);
    }

    public async Task<User> InsertOrUpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = user.Id.ToString(),
            email = user.Email,
            first_name = user.FirstName,
            last_name = user.LastName,
            password_hash = user.PasswordHash,
            phone_number = user.PhoneNumber,
            security_stamp = user.SecurityStamp,
            is_active = user.IsActive ? 1 : 0,
            is_email_confirmed = user.IsEmailConfirmed ? 1 : 0
        });

        return await connection.QuerySingleAsync<User>(
            StoredProcedure.UsersInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.UsersSoftDelete,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }
}
