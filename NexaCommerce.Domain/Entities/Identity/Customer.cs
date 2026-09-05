namespace NexaCommerce.Domain.Entities.Identity;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TaxNumber { get; set; }
    public decimal CommissionRate { get; set; } = 10.00m;
    public string Status { get; set; } = CustomerStatus.Pending.ToString();
    public bool IsVerified { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; } = false;
}
