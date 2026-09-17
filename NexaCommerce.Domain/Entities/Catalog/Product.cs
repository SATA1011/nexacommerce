namespace NexaCommerce.Domain.Entities.Catalog;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VendorId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; } = 0.00m;
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; } = 0;
    public string? PrimaryImageUrl { get; set; }
    public string Status { get; set; } = ProductStatus.Draft;
    public string? RejectionReason { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Joined metadata populated by Stored Procedures
    public string? VendorStoreName { get; set; }
    public string? VendorSlug { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
}
