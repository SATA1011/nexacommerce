using NexaCommerce.Domain.Entities.Identity;

namespace NexaCommerce.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<User> InsertOrUpdateAsync(User user, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
