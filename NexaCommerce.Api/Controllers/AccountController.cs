using System.Security.Cryptography;
using System.Text;
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
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserSessionRepository userSessionRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AccountController> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userSessionRepository = userSessionRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    /// <summary>
    /// User authentication / login endpoint returning JWT access token & refresh token
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
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

            var userRoles = await _roleRepository.GetRoleNamesByUserIdAsync(user.Id, cancellationToken);
            var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, userRoles);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var tokenHash = HashToken(refreshToken);

            // Persist Refresh Token
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                CreatedByIp = clientIp,
                CreatedAtUtc = DateTime.UtcNow
            };
            await _refreshTokenRepository.InsertOrUpdateAsync(refreshTokenEntity, cancellationToken);

            // Persist User Session
            var userAgent = Request.Headers.UserAgent.ToString();
            var userSessionEntity = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                DeviceName = "Web Browser",
                IpAddress = clientIp,
                UserAgent = string.IsNullOrWhiteSpace(userAgent) ? "Unknown" : userAgent,
                CreatedAtUtc = DateTime.UtcNow,
                LastActivityAtUtc = DateTime.UtcNow
            };
            await _userSessionRepository.InsertOrUpdateAsync(userSessionEntity, cancellationToken);

            var userResponse = MapToUserResponse(user);
            var authResponse = new AuthResponse(accessToken, refreshToken, expiresAt, userResponse);

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
    /// Refresh access token using active refresh token (with token rotation)
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            var oldTokenHash = HashToken(request.RefreshToken);
            var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(oldTokenHash, cancellationToken);
            if (existingToken is null || !existingToken.IsActive)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                return Unauthorized(new { message = "User account is inactive or not found." });
            }

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var userRoles = await _roleRepository.GetRoleNamesByUserIdAsync(user.Id, cancellationToken);
            var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, userRoles);
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var newTokenHash = HashToken(newRefreshToken);

            // Revoke old refresh token with rotation replacement link
            await _refreshTokenRepository.RevokeAsync(oldTokenHash, clientIp, newTokenHash, "Refreshed token", cancellationToken);

            // Persist new refresh token
            var newTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newTokenHash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                CreatedByIp = clientIp,
                CreatedAtUtc = DateTime.UtcNow
            };
            await _refreshTokenRepository.InsertOrUpdateAsync(newTokenEntity, cancellationToken);

            // Update session activity
            var userAgent = Request.Headers.UserAgent.ToString();
            var session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                DeviceName = request.DeviceName ?? "Web Browser",
                IpAddress = clientIp,
                UserAgent = string.IsNullOrWhiteSpace(userAgent) ? "Unknown" : userAgent,
                CreatedAtUtc = DateTime.UtcNow,
                LastActivityAtUtc = DateTime.UtcNow
            };
            await _userSessionRepository.InsertOrUpdateAsync(session, cancellationToken);

            var expiresAt = DateTime.UtcNow.AddMinutes(60);
            var userResponse = MapToUserResponse(user);

            return Ok(new AuthResponse(newAccessToken, newRefreshToken, expiresAt, userResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while refreshing token");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while refreshing the token.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Revoke specified refresh token
    /// </summary>
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            var tokenHash = HashToken(request.RefreshToken);
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            await _refreshTokenRepository.RevokeAsync(tokenHash, clientIp, null, "Revoked by request", cancellationToken);

            return Ok(new { message = "Token successfully revoked." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while revoking refresh token");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while revoking the token.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Get active sessions for current authenticated user
    /// </summary>
    [Authorize]
    [HttpPost("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid token claims." });
            }

            var sessions = await _userSessionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
            var responses = sessions.Select(s => new UserSessionResponse(
                s.Id, s.UserId, s.DeviceName, s.IpAddress, s.UserAgent, s.CreatedAtUtc, s.LastActivityAtUtc, s.IsRevoked
            ));

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user sessions");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while retrieving user sessions.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Revoke all active sessions for current authenticated user
    /// </summary>
    [Authorize]
    [HttpPost("revoke-sessions")]
    public async Task<IActionResult> RevokeSessions(CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid token claims." });
            }

            await _userSessionRepository.RevokeAllByUserIdAsync(userId, cancellationToken);
            return Ok(new { message = "All active user sessions revoked successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while revoking user sessions");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal server error occurred while revoking user sessions.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Register / Create a new user account with PBKDF2 password hashing
    /// </summary>
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
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

            var userResponse = MapToUserResponse(createdUser);
            return Ok(userResponse);
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
    public async Task<IActionResult> GetUsers([FromBody] GetUsersRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (users, totalCount) = await _userRepository.GetAllAsync(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );

            var userResponses = users.Select(MapToUserResponse);
            var result = new PaginatedResult<UserResponse>(userResponses, totalCount, request.PageNumber, request.PageSize);
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
    public async Task<IActionResult> GetUserById([FromBody] GetUserByIdRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user is null)
            {
                return NotFound(new { message = $"User with ID '{request.Id}' was not found." });
            }

            return Ok(MapToUserResponse(user));
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
    public async Task<IActionResult> GetUserByEmail([FromBody] GetUserByEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                return NotFound(new { message = $"User with Email '{request.Email}' was not found." });
            }

            return Ok(MapToUserResponse(user));
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
    public async Task<IActionResult> SoftDeleteUser([FromBody] SoftDeleteUserRequest request, CancellationToken cancellationToken = default)
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

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static UserResponse MapToUserResponse(User user) => new()
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
