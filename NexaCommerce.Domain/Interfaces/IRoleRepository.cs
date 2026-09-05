using NexaCommerce.Domain.Entities.Identity;

namespace NexaCommerce.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Role> Roles, int TotalCount)> GetAllAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Role> InsertOrUpdateAsync(Role role, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
