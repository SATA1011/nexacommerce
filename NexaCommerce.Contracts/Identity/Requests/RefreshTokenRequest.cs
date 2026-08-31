namespace NexaCommerce.Contracts.Identity.Requests;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
}
