namespace NexaCommerce.Contracts.Identity.Responses;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserResponse User { get; set; } = default!;

    public AuthResponse() { }

    public AuthResponse(string accessToken, string refreshToken, DateTime expiresAtUtc, UserResponse user)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAtUtc = expiresAtUtc;
        User = user;
    }
}
