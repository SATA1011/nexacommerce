using NexaCommerce.Domain.Entities.Catalog;

namespace NexaCommerce.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(ProductFilter filter, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetByVendorIdAsync(Guid vendorId, ProductFilter filter, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetForModerationAsync(ProductFilter filter, CancellationToken cancellationToken = default);
    Task<Product> InsertOrUpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> UpdateStatusAsync(Guid id, string status, string? rejectionReason = null, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, Guid? vendorId = null, CancellationToken cancellationToken = default);
}
