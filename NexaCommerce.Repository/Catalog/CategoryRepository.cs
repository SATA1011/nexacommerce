using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Catalog;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Catalog;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Category>(
            StoredProcedure.CategoriesGet,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Category>(
            StoredProcedure.CategoriesGetBySlug,
            new { p_slug = slug.Trim() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<Category>(
            StoredProcedure.CategoriesGetAll,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Category> InsertOrUpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = category.Id.ToString(),
            parent_id = category.ParentId?.ToString(),
            name = category.Name,
            slug = category.Slug,
            description = category.Description,
            image_url = category.ImageUrl,
            display_order = category.DisplayOrder,
            is_active = category.IsActive ? 1 : 0
        });

        return await connection.QuerySingleAsync<Category>(
            StoredProcedure.CategoriesInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            StoredProcedure.CategoriesSoftDelete,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }
}
