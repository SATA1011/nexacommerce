namespace NexaCommerce.Contracts.Identity.Requests;

public class GetStoresRequest
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
