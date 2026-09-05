using System.Data;
using System.Text.Json;
using Dapper;
using NexaCommerce.Common.Constants;
using NexaCommerce.Domain.Entities.Identity;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Repository.Identity;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CustomerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Customer>(
            StoredProcedure.CustomersGet,
            new { p_id = id.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Customer>(
            StoredProcedure.CustomersGetByUserId,
            new { p_user_id = userId.ToString() },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<(IEnumerable<Customer> Customers, int TotalCount)> GetAllAsync(
        string? searchTerm, 
        string? status, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            StoredProcedure.CustomersGetAll,
            new 
            { 
                p_search_term = searchTerm, 
                p_status = status,
                p_page_number = pageNumber, 
                p_page_size = pageSize 
            },
            commandType: CommandType.StoredProcedure
        );

        var totalCount = await multi.ReadSingleAsync<int>();
        var customers = await multi.ReadAsync<Customer>();

        return (customers, totalCount);
    }

    public async Task<Customer> InsertOrUpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var jsonPayload = JsonSerializer.Serialize(new
        {
            id = customer.Id.ToString(),
            user_id = customer.UserId.ToString(),
            store_name = customer.StoreName,
            slug = customer.Slug,
            description = customer.Description,
            tax_number = customer.TaxNumber,
            commission_rate = customer.CommissionRate,
            status = customer.Status,
            is_verified = customer.IsVerified ? 1 : 0
        });

        return await connection.QuerySingleAsync<Customer>(
            StoredProcedure.CustomersInsertUpdate,
            new { p_json = jsonPayload },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Customer?> UpdateStatusAsync(Guid id, string status, bool isVerified, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Customer>(
            StoredProcedure.CustomersUpdateStatus,
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
