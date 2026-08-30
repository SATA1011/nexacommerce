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
public sealed class AccountController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AccountController> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    /// <summary>
    /// User authentication / login endpoint returning JWT access token & refresh token
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "User account is inactive." });
            }

            var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            var userDto = MapToUserResponseDto(user);
            var authResponse = new AuthResponseDto(accessToken, refreshToken, expiresAt, userDto);

            _logger.LogInformation("User '{Email}' logged in successfully.", user.Email);
            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for user '{Email}'", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred during login.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Register / Create a new user account with PBKDF2 password hashing
    /// </summary>
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser is not null)
            {
                return BadRequest(new { message = $"User with email '{request.Email}' already exists." });
            }

            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpperInvariant(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = passwordHash,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                IsEmailConfirmed = false,
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            var createdUser = await _userRepository.InsertOrUpdateAsync(newUser, cancellationToken);
            _logger.LogInformation("Successfully created user account {Email} ({UserId})", createdUser.Email, createdUser.Id);

            var dto = MapToUserResponseDto(createdUser);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user account {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while creating the user account.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Get paginated list of users with optional search filtering
    /// </summary>
    [Authorize]
    [HttpPost("get-users")]
    public async Task<IActionResult> GetUsers([FromBody] GetUsersRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (users, totalCount) = await _userRepository.GetAllAsync(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );

            var dtos = users.Select(MapToUserResponseDto);
            var result = new PaginatedResultDto<UserResponseDto>(dtos, totalCount, request.PageNumber, request.PageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching users list");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while retrieving users.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Get user by unique ID via POST body
    /// </summary>
    [Authorize]
    [HttpPost("get-user-by-id")]
    public async Task<IActionResult> GetUserById([FromBody] GetUserByIdRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user is null)
            {
                return NotFound(new { message = $"User with ID '{request.Id}' was not found." });
            }

            return Ok(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user by ID {UserId}", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An internal server error occurred while retrieving user '{request.Id}'.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Get user by email address via POST body
    /// </summary>
    [Authorize]
    [HttpPost("get-user-by-email")]
    public async Task<IActionResult> GetUserByEmail([FromBody] GetUserByEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                return NotFound(new { message = $"User with Email '{request.Email}' was not found." });
            }

            return Ok(MapToUserResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user by email {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An internal server error occurred while retrieving user '{request.Email}'.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Soft delete user account by ID via POST body
    /// </summary>
    [Authorize]
    [HttpPost("delete-user")]
    public async Task<IActionResult> SoftDeleteUser([FromBody] SoftDeleteUserRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingUser is null)
            {
                return NotFound(new { message = $"User with ID '{request.Id}' was not found." });
            }

            await _userRepository.SoftDeleteAsync(request.Id, cancellationToken);
            _logger.LogInformation("Soft-deleted user account {UserId}", request.Id);

            return Ok(new { message = $"User account '{request.Id}' successfully deleted." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while soft-deleting user account {UserId}", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An internal server error occurred while deleting user '{request.Id}'.", detail = ex.Message });
        }
    }

    private static UserResponseDto MapToUserResponseDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber,
        IsActive = user.IsActive,
        IsEmailConfirmed = user.IsEmailConfirmed,
        CreatedAtUtc = user.CreatedAtUtc,
        LastLoginAtUtc = user.LastLoginAtUtc
    };
}
