using System.ComponentModel.DataAnnotations;

namespace NexaCommerce.Contracts.Catalog.Requests;

public class CreateProductImageRequest
{
    [Required]
    public Guid ProductId { get; set; }

    public Guid? VariantId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? ThumbnailUrl { get; set; }

    [MaxLength(255)]
    public string? AltText { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsPrimary { get; set; } = false;
}
