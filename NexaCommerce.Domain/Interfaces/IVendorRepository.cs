using NexaCommerce.Domain.Entities.Identity;

namespace NexaCommerce.Domain.Interfaces;

public interface IVendorRepository
{
    Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vendor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Vendor> Vendors, int TotalCount)> GetAllAsync(string? searchTerm, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Vendor> InsertOrUpdateAsync(Vendor vendor, CancellationToken cancellationToken = default);
    Task<Vendor?> UpdateStatusAsync(Guid id, string status, bool isVerified, CancellationToken cancellationToken = default);
}
