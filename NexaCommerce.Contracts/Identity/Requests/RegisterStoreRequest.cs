namespace NexaCommerce.Contracts.Identity.Requests;

public class RegisterStoreRequest
{
    public string StoreName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TaxNumber { get; set; }
}
