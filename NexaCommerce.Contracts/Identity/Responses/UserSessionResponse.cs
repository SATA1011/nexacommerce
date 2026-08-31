namespace NexaCommerce.Contracts.Identity.Responses;

public class UserSessionResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? DeviceName { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastActivityAtUtc { get; set; }
    public bool IsRevoked { get; set; }

    public UserSessionResponse() { }

    public UserSessionResponse(Guid id, Guid userId, string? deviceName, string ipAddress, string? userAgent, DateTime createdAtUtc, DateTime lastActivityAtUtc, bool isRevoked)
    {
        Id = id;
        UserId = userId;
        DeviceName = deviceName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAtUtc = createdAtUtc;
        LastActivityAtUtc = lastActivityAtUtc;
        IsRevoked = isRevoked;
    }
}
