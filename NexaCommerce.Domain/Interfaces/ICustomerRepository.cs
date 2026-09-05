using NexaCommerce.Domain.Entities.Identity;

namespace NexaCommerce.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Customer> Customers, int TotalCount)> GetAllAsync(string? searchTerm, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Customer> InsertOrUpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<Customer?> UpdateStatusAsync(Guid id, string status, bool isVerified, CancellationToken cancellationToken = default);
}
