namespace NexaCommerce.Contracts.Catalog.Responses;

public class ProductResponse
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? VendorStoreName { get; set; }
    public string? VendorSlug { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
}
