using NexaCommerce.Domain.Entities.Catalog;

namespace NexaCommerce.Domain.Interfaces;

public interface IProductVariantRepository
{
    Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductVariant> InsertOrUpdateAsync(ProductVariant variant, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid productId, CancellationToken cancellationToken = default);
}
