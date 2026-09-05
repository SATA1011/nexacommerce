using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaCommerce.Contracts.Common;
using NexaCommerce.Contracts.Identity.Requests;
using NexaCommerce.Contracts.Identity.Responses;
using NexaCommerce.Domain.Entities.Identity;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CustomerController> _logger;


    public CustomerController(
        ICustomerRepository customerRepository,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        ILogger<CustomerController> logger)
    {
        _customerRepository = customerRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Register / Apply to become a customer seller with a store profile (Customer store onboarding)
    /// </summary>
    [Authorize]
    [HttpPost("register-store")]
    public async Task<IActionResult> RegisterStore([FromBody] RegisterStoreRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user identity in token." });
            }

            if (string.IsNullOrWhiteSpace(request.StoreName) || string.IsNullOrWhiteSpace(request.Slug))
            {
                return BadRequest(new { message = "Store name and slug are required." });
            }

            var existingStore = await _customerRepository.GetByUserIdAsync(userId, cancellationToken);
            if (existingStore is not null)
            {
                return BadRequest(new { message = "You have already registered a store.", storeId = existingStore.Id });
            }

            var newCustomerStore = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreName = request.StoreName.Trim(),
                Slug = request.Slug.Trim().ToLowerInvariant(),
                Description = request.Description?.Trim(),
                TaxNumber = request.TaxNumber?.Trim(),
                CommissionRate = 10.00m,
                Status = CustomerStatus.Pending.ToString(),
                IsVerified = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            var createdStore = await _customerRepository.InsertOrUpdateAsync(newCustomerStore, cancellationToken);

            // Assign Customer role (seller privileges) to user
            try
            {
                var customerRole = await _roleRepository.GetByNameAsync("Customer", cancellationToken);
                if (customerRole is not null)
                {
                    await _roleRepository.AssignRoleToUserAsync(userId, customerRole.Id, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Customer role not found in database when registering store for user {UserId}.", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not assign Customer role to user {UserId}.", userId);
            }

            _logger.LogInformation("Store '{StoreName}' registered successfully for User {UserId} in Pending status.", createdStore.StoreName, userId);

            var response = MapToStoreResponse(createdStore);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during store registration for store '{StoreName}'", request.StoreName);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while registering the store.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Get the logged-in customer's registered store profile
    /// </summary>
    [Authorize]
    [HttpPost("my-store")]
    public async Task<IActionResult> GetMyStore(CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user identity in token." });
            }

            var store = await _customerRepository.GetByUserIdAsync(userId, cancellationToken);
            if (store is null)
            {
                return NotFound(new { message = "No store profile found for the current user." });
            }

            return Ok(MapToStoreResponse(store));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching my store profile");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Admin: Get paginated list of all registered stores with optional search and status filtering
    /// </summary>
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("get-stores")]
    public async Task<IActionResult> GetStores([FromBody] GetStoresRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (stores, totalCount) = await _customerRepository.GetAllAsync(
                request.SearchTerm,
                request.Status,
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );

            var storeResponses = stores.Select(MapToStoreResponse);
            var result = new PaginatedResult<StoreResponse>(storeResponses, totalCount, request.PageNumber, request.PageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving stores list");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while retrieving stores.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Admin: Update store status (Approve, Reject, Suspend) and verification flag
    /// </summary>
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("update-store-status")]
    public async Task<IActionResult> UpdateStoreStatus([FromBody] UpdateStoreStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.StoreId == Guid.Empty)
            {
                return BadRequest(new { message = "Valid StoreId is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Status is required (Pending, Approved, Rejected, Suspended)." });
            }

            var updatedStore = await _customerRepository.UpdateStatusAsync(
                request.StoreId,
                request.Status.Trim(),
                request.IsVerified,
                cancellationToken
            );

            if (updatedStore is null)
            {
                return NotFound(new { message = $"Store with ID '{request.StoreId}' not found." });
            }

            _logger.LogInformation("Store {StoreId} status updated to '{Status}' (Verified: {IsVerified})", request.StoreId, request.Status, request.IsVerified);
            return Ok(MapToStoreResponse(updatedStore));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating status for store {StoreId}", request.StoreId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while updating store status.", detail = ex.Message });
        }
    }

    private static StoreResponse MapToStoreResponse(Customer store) =>
        new()
        {
            Id = store.Id,
            UserId = store.UserId,
            StoreName = store.StoreName,
            Slug = store.Slug,
            Description = store.Description,
            TaxNumber = store.TaxNumber,
            CommissionRate = store.CommissionRate,
            Status = store.Status,
            IsVerified = store.IsVerified,
            CreatedAtUtc = store.CreatedAtUtc,
            UpdatedAtUtc = store.UpdatedAtUtc
        };
}
