using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Identity;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Identity;

public sealed class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Role>(
            StoredProcedure.RolesGet,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Role>(
            StoredProcedure.RolesGetByName,
            new { p_name = name },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<(IEnumerable<Role> Roles, int TotalCount)> GetAllAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            StoredProcedure.RolesGetAll,
            new { p_search_term = searchTerm, p_page_number = pageNumber, p_page_size = pageSize },
            commandType: CommandType.StoredProcedure
        );

        var totalCount = await multi.ReadSingleAsync<int>();
        var roles = await multi.ReadAsync<Role>();

        return (roles, totalCount);
    }

    public async Task<Role> InsertOrUpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = role.Id.ToString(),
            name = role.Name,
            description = role.Description
        });

        return await connection.QuerySingleAsync<Role>(
            StoredProcedure.RolesInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.RolesSoftDelete,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.UserRolesAssign,
            new { p_user_id = userId.ToString(), p_role_id = roleId.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.UserRolesRemove,
            new { p_user_id = userId.ToString(), p_role_id = roleId.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var roles = await connection.QueryAsync<Role>(
            StoredProcedure.UserRolesGetByUserId,
            new { p_user_id = userId.ToString() },
            commandType: CommandType.StoredProcedure
        );
        return roles.Select(r => r.Name);
    }
}
