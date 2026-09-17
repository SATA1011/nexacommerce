using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Catalog;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Catalog;

public sealed class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Product>(
            StoredProcedure.ProductsGet,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Product>(
            StoredProcedure.ProductsGetBySlug,
            new { p_slug = slug.Trim() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        ProductFilter filter,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            StoredProcedure.ProductsGetAll,
            new
            {
                p_search_term = string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : filter.SearchTerm.Trim(),
                p_category_id = filter.CategoryId?.ToString(),
                p_brand_id = filter.BrandId?.ToString(),
                p_vendor_slug = string.IsNullOrWhiteSpace(filter.VendorSlug) ? null : filter.VendorSlug.Trim(),
                p_min_price = filter.MinPrice,
                p_max_price = filter.MaxPrice,
                p_sort_by = string.IsNullOrWhiteSpace(filter.SortBy) ? null : filter.SortBy.Trim(),
                p_page_number = filter.PageNumber,
                p_page_size = filter.PageSize
            },
            commandType: CommandType.StoredProcedure
        );

        var totalCount = await multi.ReadSingleAsync<int>();
        var products = await multi.ReadAsync<Product>();

        return (products, totalCount);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetByVendorIdAsync(
        Guid vendorId,
        ProductFilter filter,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            StoredProcedure.ProductsGetByVendorId,
            new
            {
                p_vendor_id = vendorId.ToString(),
                p_search_term = string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : filter.SearchTerm.Trim(),
                p_status = string.IsNullOrWhiteSpace(filter.Status) ? null : filter.Status.Trim(),
                p_page_number = filter.PageNumber,
                p_page_size = filter.PageSize
            },
            commandType: CommandType.StoredProcedure
        );

        var totalCount = await multi.ReadSingleAsync<int>();
        var products = await multi.ReadAsync<Product>();

        return (products, totalCount);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetForModerationAsync(
        ProductFilter filter,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            StoredProcedure.ProductsGetForModeration,
            new
            {
                p_search_term = string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : filter.SearchTerm.Trim(),
                p_status = string.IsNullOrWhiteSpace(filter.Status) ? null : filter.Status.Trim(),
                p_page_number = filter.PageNumber,
                p_page_size = filter.PageSize
            },
            commandType: CommandType.StoredProcedure
        );

        var totalCount = await multi.ReadSingleAsync<int>();
        var products = await multi.ReadAsync<Product>();

        return (products, totalCount);
    }

    public async Task<Product> InsertOrUpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = product.Id.ToString(),
            vendor_id = product.VendorId.ToString(),
            category_id = product.CategoryId?.ToString(),
            brand_id = product.BrandId?.ToString(),
            title = product.Title,
            slug = product.Slug,
            short_description = product.ShortDescription,
            description = product.Description,
            sku = product.Sku,
            price = product.Price,
            compare_at_price = product.CompareAtPrice,
            cost_price = product.CostPrice,
            stock_quantity = product.StockQuantity,
            primary_image_url = product.PrimaryImageUrl,
            status = product.Status,
            is_active = product.IsActive ? 1 : 0
        });

        return await connection.QuerySingleAsync<Product>(
            StoredProcedure.ProductsInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Product?> UpdateStatusAsync(Guid id, string status, string? rejectionReason = null, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Product>(
            StoredProcedure.ProductsUpdateStatus,
            new
            {
                p_id = id.ToString(),
                p_status = status,
                p_rejection_reason = rejectionReason
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task SoftDeleteAsync(Guid id, Guid? vendorId = null, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.ProductsSoftDelete,
            new
            {
                p_id = id.ToString(),
                p_vendor_id = vendorId?.ToString()
            },
            commandType: CommandType.StoredProcedure
        );
    }
}
