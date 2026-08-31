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
public sealed class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RolesController> _logger;

    public RolesController(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        ILogger<RolesController> logger)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of roles with optional search filtering
    /// </summary>
    [Authorize]
    [HttpPost("get-roles")]
    public async Task<IActionResult> GetRoles([FromBody] GetRolesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (roles, totalCount) = await _roleRepository.GetAllAsync(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );

            var roleResponses = roles.Select(MapToRoleResponse);
            var result = new PaginatedResult<RoleResponse>(roleResponses, totalCount, request.PageNumber, request.PageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching roles list");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while retrieving roles.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [Authorize]
    [HttpPost("create-role")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Role name is required." });
            }

            var newRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                NormalizedName = request.Name.ToUpperInvariant(),
                Description = request.Description
            };

            var createdRole = await _roleRepository.InsertOrUpdateAsync(newRole, cancellationToken);
            _logger.LogInformation("Role '{RoleName}' created successfully with ID {RoleId}", createdRole.Name, createdRole.Id);

            return Ok(MapToRoleResponse(createdRole));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating role '{RoleName}'", request.Name);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while creating the role.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Soft delete a role by ID
    /// </summary>
    [Authorize]
    [HttpPost("delete-role")]
    public async Task<IActionResult> DeleteRole([FromBody] GetUserByIdRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingRole = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingRole is null)
            {
                return NotFound(new { message = $"Role with ID '{request.Id}' was not found." });
            }

            await _roleRepository.SoftDeleteAsync(request.Id, cancellationToken);
            _logger.LogInformation("Soft-deleted role {RoleId}", request.Id);

            return Ok(new { message = $"Role '{existingRole.Name}' successfully deleted." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting role {RoleId}", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An internal server error occurred while deleting role '{request.Id}'.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    [Authorize]
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return NotFound(new { message = $"User with ID '{request.UserId}' was not found." });
            }

            var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
            if (role is null)
            {
                return NotFound(new { message = $"Role with ID '{request.RoleId}' was not found." });
            }

            await _roleRepository.AssignRoleToUserAsync(request.UserId, request.RoleId, cancellationToken);
            _logger.LogInformation("Assigned role '{RoleName}' to user '{Email}'", role.Name, user.Email);

            return Ok(new { message = $"Role '{role.Name}' successfully assigned to user '{user.Email}'." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning role {RoleId} to user {UserId}", request.RoleId, request.UserId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while assigning the role.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [Authorize]
    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _roleRepository.RemoveRoleFromUserAsync(request.UserId, request.RoleId, cancellationToken);
            _logger.LogInformation("Removed role {RoleId} from user {UserId}", request.RoleId, request.UserId);

            return Ok(new { message = "Role successfully removed from user." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing role {RoleId} from user {UserId}", request.RoleId, request.UserId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while removing the role.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Get assigned roles for a user
    /// </summary>
    [Authorize]
    [HttpPost("get-user-roles")]
    public async Task<IActionResult> GetUserRoles([FromBody] GetUserByIdRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var roleNames = await _roleRepository.GetRoleNamesByUserIdAsync(request.Id, cancellationToken);
            return Ok(roleNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving roles for user {UserId}", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while retrieving user roles.", detail = ex.Message });
        }
    }

    private static RoleResponse MapToRoleResponse(Role role) => new(
        role.Id,
        role.Name,
        role.NormalizedName,
        role.Description
    );
}
