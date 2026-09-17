using System.ComponentModel.DataAnnotations;

namespace NexaCommerce.Contracts.Catalog.Requests;

public class UpdateProductRequest
{
    [Required]
    public Guid Id { get; set; }

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
    public int StockQuantity { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
