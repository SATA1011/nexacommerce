namespace NexaCommerce.Contracts.Identity.Responses;

public class StoreResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TaxNumber { get; set; }
    public decimal CommissionRate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
