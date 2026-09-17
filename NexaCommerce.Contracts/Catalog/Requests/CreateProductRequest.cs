using System.ComponentModel.DataAnnotations;

namespace NexaCommerce.Contracts.Catalog.Requests;

public class CreateProductRequest
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Sku { get; set; }

    [Range(0, 99999999.99)]
    public decimal Price { get; set; }

    [Range(0, 99999999.99)]
    public decimal? CompareAtPrice { get; set; }

    [Range(0, 99999999.99)]
    public decimal? CostPrice { get; set; }

    [Range(0, 1000000)]
    public int StockQuantity { get; set; } = 0;

    public string? PrimaryImageUrl { get; set; }

    public List<CreateProductImageRequest>? Images { get; set; }
    public List<CreateProductVariantRequest>? Variants { get; set; }
}
