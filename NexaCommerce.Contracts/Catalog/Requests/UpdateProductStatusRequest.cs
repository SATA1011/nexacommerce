using System.ComponentModel.DataAnnotations;

namespace NexaCommerce.Contracts.Catalog.Requests;

public class UpdateProductStatusRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
