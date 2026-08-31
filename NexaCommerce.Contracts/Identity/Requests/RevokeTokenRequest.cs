namespace NexaCommerce.Contracts.Identity.Requests;

public class RevokeTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
