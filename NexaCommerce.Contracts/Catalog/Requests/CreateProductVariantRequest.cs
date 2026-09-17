using System.ComponentModel.DataAnnotations;

namespace NexaCommerce.Contracts.Catalog.Requests;

public class CreateProductVariantRequest
{
    [Required]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Range(0, 99999999.99)]
    public decimal Price { get; set; }

    [Range(0, 99999999.99)]
    public decimal? CompareAtPrice { get; set; }

    [Range(0, 1000000)]
    public int StockQuantity { get; set; } = 0;

    public string? AttributesJson { get; set; }
}
