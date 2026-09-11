namespace NexaCommerce.Contracts.Identity.Requests;

public class RegisterVendorRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? BusinessAddress { get; set; }
}
