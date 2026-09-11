using System.Security.Claims;
using System.Text.RegularExpressions;
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
public sealed class VendorController : ControllerBase
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<VendorController> _logger;


    public VendorController(
        IVendorRepository vendorRepository,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<VendorController> logger)
    {
        _vendorRepository = vendorRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Guest merchant onboarding: creates user account, assigns Vendor role, and creates store profile in Pending status
    /// </summary>
    [HttpPost("register")]
    [HttpPost("register-vendor")]
    public async Task<IActionResult> Register([FromBody] RegisterVendorRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new { message = "First name and last name are required." });
            }

            if (string.IsNullOrWhiteSpace(request.StoreName))
            {
                return BadRequest(new { message = "Store name is required." });
            }

            var existingUser = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
            if (existingUser is not null)
            {
                return BadRequest(new { message = $"User with email '{request.Email}' already exists." });
            }

            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim(),
                NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                PasswordHash = passwordHash,
                PhoneNumber = request.PhoneNumber?.Trim(),
                IsActive = true,
                IsEmailConfirmed = false,
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            var createdUser = await _userRepository.InsertOrUpdateAsync(newUser, cancellationToken);

            // Assign Vendor and User roles
            try
            {
                var vendorRole = await _roleRepository.GetByNameAsync("Vendor", cancellationToken);
                if (vendorRole is not null)
                {
                    await _roleRepository.AssignRoleToUserAsync(createdUser.Id, vendorRole.Id, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Vendor role not found in database for user {UserId}.", createdUser.Id);
                }

                var userRole = await _roleRepository.GetByNameAsync("User", cancellationToken);
                if (userRole is not null)
                {
                    await _roleRepository.AssignRoleToUserAsync(createdUser.Id, userRole.Id, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not assign roles to user {UserId}.", createdUser.Id);
            }

            // Generate clean URL slug from store name
            var baseSlug = Regex.Replace(request.StoreName.Trim().ToLowerInvariant(), @"[^a-z0-9\s-]", "");
            var slug = Regex.Replace(baseSlug, @"\s+", "-").Trim('-');
            if (string.IsNullOrWhiteSpace(slug))
            {
                slug = $"store-{Guid.NewGuid().ToString()[..8]}";
            }

            // Create vendor store profile
            var newVendorStore = new Vendor
            {
                Id = Guid.NewGuid(),
                UserId = createdUser.Id,
                StoreName = request.StoreName.Trim(),
                Slug = slug,
                Description = !string.IsNullOrWhiteSpace(request.BusinessAddress)
                    ? $"Address: {request.BusinessAddress.Trim()}"
                    : null,
                TaxNumber = request.TaxNumber?.Trim(),
                CommissionRate = 10.00m,
                Status = VendorStatus.Pending.ToString(),
                IsVerified = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            var createdStore = await _vendorRepository.InsertOrUpdateAsync(newVendorStore, cancellationToken);

            _logger.LogInformation("Successfully registered merchant account {Email} ({UserId}) with store '{StoreName}' ({StoreId})",
                createdUser.Email, createdUser.Id, createdStore.StoreName, createdStore.Id);

            return Ok(new
            {
                success = true,
                message = "Merchant registration submitted successfully. Your store is pending administrator review.",
                userId = createdUser.Id,
                store = MapToStoreResponse(createdStore)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while registering merchant account {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while registering the merchant account.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Register / Apply to become a vendor with a store profile (Vendor store onboarding)
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

            var existingStore = await _vendorRepository.GetByUserIdAsync(userId, cancellationToken);
            if (existingStore is not null)
            {
                return BadRequest(new { message = "You have already registered a store.", storeId = existingStore.Id });
            }

            var newVendorStore = new Vendor
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreName = request.StoreName.Trim(),
                Slug = request.Slug.Trim().ToLowerInvariant(),
                Description = request.Description?.Trim(),
                TaxNumber = request.TaxNumber?.Trim(),
                CommissionRate = 10.00m,
                Status = VendorStatus.Pending.ToString(),
                IsVerified = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            var createdStore = await _vendorRepository.InsertOrUpdateAsync(newVendorStore, cancellationToken);

            // Assign Vendor role to user
            try
            {
                var vendorRole = await _roleRepository.GetByNameAsync("Vendor", cancellationToken);
                if (vendorRole is not null)
                {
                    await _roleRepository.AssignRoleToUserAsync(userId, vendorRole.Id, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Vendor role not found in database when registering store for user {UserId}.", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not assign Vendor role to user {UserId}.", userId);
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
    /// Get the logged-in vendor's registered store profile
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

            var store = await _vendorRepository.GetByUserIdAsync(userId, cancellationToken);
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
            var searchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
            var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim();

            var (stores, totalCount) = await _vendorRepository.GetAllAsync(
                searchTerm,
                status,
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

            var updatedStore = await _vendorRepository.UpdateStatusAsync(
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

    private static StoreResponse MapToStoreResponse(Vendor store) =>
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
