using NexaCommerce.Domain.Entities.Catalog;

namespace NexaCommerce.Domain.Interfaces;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Brand>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Brand> InsertOrUpdateAsync(Brand brand, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
