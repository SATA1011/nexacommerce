namespace NexaCommerce.Contracts.Identity.Requests;

public class GetUsersRequestDto
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
