namespace NexaCommerce.Contracts.Identity.Requests;

public class GetUsersRequest
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
