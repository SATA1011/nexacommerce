using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Identity;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Identity;

public sealed class VendorRepository : IVendorRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public VendorRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Vendor>(
            StoredProcedure.VendorsGet,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Vendor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Vendor>(
            StoredProcedure.VendorsGetByUserId,
            new { p_user_id = userId.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<(IEnumerable<Vendor> Vendors, int TotalCount)> GetAllAsync(
        string? searchTerm,
        string? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            StoredProcedure.VendorsGetAll,
            new
            {
                p_search_term = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim(),
                p_status = string.IsNullOrWhiteSpace(status) ? null : status.Trim(),
                p_page_number = pageNumber,
                p_page_size = pageSize
            },
            commandType: CommandType.StoredProcedure
        );

        var totalCount = await multi.ReadSingleAsync<int>();
        var vendors = await multi.ReadAsync<Vendor>();

        return (vendors, totalCount);
    }

    public async Task<Vendor> InsertOrUpdateAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = vendor.Id.ToString(),
            user_id = vendor.UserId.ToString(),
            store_name = vendor.StoreName,
            slug = vendor.Slug,
            description = vendor.Description,
            tax_number = vendor.TaxNumber,
            commission_rate = vendor.CommissionRate,
            status = vendor.Status,
            is_verified = vendor.IsVerified ? 1 : 0
        });

        return await connection.QuerySingleAsync<Vendor>(
            StoredProcedure.VendorsInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Vendor?> UpdateStatusAsync(Guid id, string status, bool isVerified, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Vendor>(
            StoredProcedure.VendorsUpdateStatus,
            new
            {
                p_id = id.ToString(),
                p_status = status,
                p_is_verified = isVerified ? 1 : 0
            },
            commandType: CommandType.StoredProcedure
        );
    }
}
