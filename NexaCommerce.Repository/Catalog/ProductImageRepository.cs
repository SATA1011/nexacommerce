using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Catalog;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Catalog;

public sealed class ProductImageRepository : IProductImageRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductImageRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<ProductImage>(
            StoredProcedure.ProductImagesGetByProductId,
            new { p_product_id = productId.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<ProductImage> InsertOrUpdateAsync(ProductImage image, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = image.Id.ToString(),
            product_id = image.ProductId.ToString(),
            variant_id = image.VariantId?.ToString(),
            image_url = image.ImageUrl,
            thumbnail_url = image.ThumbnailUrl,
            alt_text = image.AltText,
            sort_order = image.SortOrder,
            is_primary = image.IsPrimary ? 1 : 0
        });

        return await connection.QuerySingleAsync<ProductImage>(
            StoredProcedure.ProductImagesInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<ProductImage?> SetPrimaryAsync(Guid id, Guid productId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ProductImage>(
            StoredProcedure.ProductImagesSetPrimary,
            new
            {
                p_id = id.ToString(),
                p_product_id = productId.ToString()
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteAsync(Guid id, Guid productId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.ProductImagesDelete,
            new
            {
                p_id = id.ToString(),
                p_product_id = productId.ToString()
            },
            commandType: CommandType.StoredProcedure
        );
    }
}
