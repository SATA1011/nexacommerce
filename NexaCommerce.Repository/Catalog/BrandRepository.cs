using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Catalog;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Catalog;

public sealed class BrandRepository : IBrandRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BrandRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Brand>(
            StoredProcedure.BrandsGet,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Brand>(
            StoredProcedure.BrandsGetBySlug,
            new { p_slug = slug.Trim() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Brand>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<Brand>(
            StoredProcedure.BrandsGetAll,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Brand> InsertOrUpdateAsync(Brand brand, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = brand.Id.ToString(),
            name = brand.Name,
            slug = brand.Slug,
            description = brand.Description,
            logo_url = brand.LogoUrl,
            is_active = brand.IsActive ? 1 : 0
        });

        return await connection.QuerySingleAsync<Brand>(
            StoredProcedure.BrandsInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.BrandsSoftDelete,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }
}
