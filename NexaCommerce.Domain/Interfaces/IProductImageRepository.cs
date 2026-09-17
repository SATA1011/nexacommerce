using NexaCommerce.Domain.Entities.Catalog;

namespace NexaCommerce.Domain.Interfaces;

public interface IProductImageRepository
{
    Task<IEnumerable<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductImage> InsertOrUpdateAsync(ProductImage image, CancellationToken cancellationToken = default);
    Task<ProductImage?> SetPrimaryAsync(Guid id, Guid productId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid productId, CancellationToken cancellationToken = default);
}
