using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Catalog;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Catalog;

public sealed class ProductVariantRepository : IProductVariantRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductVariantRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<ProductVariant>(
            StoredProcedure.ProductVariantsGetByProductId,
            new { p_product_id = productId.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<ProductVariant> InsertOrUpdateAsync(ProductVariant variant, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = variant.Id.ToString(),
            product_id = variant.ProductId.ToString(),
            sku = variant.Sku,
            title = variant.Title,
            price = variant.Price,
            compare_at_price = variant.CompareAtPrice,
            stock_quantity = variant.StockQuantity,
            attributes_json = variant.AttributesJson,
            is_active = variant.IsActive ? 1 : 0
        });

        return await connection.QuerySingleAsync<ProductVariant>(
            StoredProcedure.ProductVariantsInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteAsync(Guid id, Guid productId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.ProductVariantsDelete,
            new
            {
                p_id = id.ToString(),
                p_product_id = productId.ToString()
            },
            commandType: CommandType.StoredProcedure
        );
    }
}
