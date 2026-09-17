namespace NexaCommerce.Contracts.Catalog.Responses;

public class ProductVariantResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? AttributesJson { get; set; }
    public bool IsActive { get; set; }
}
