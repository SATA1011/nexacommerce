namespace NexaCommerce.Contracts.Identity.Requests;

public class UpdateStoreStatusRequest
{
    public Guid StoreId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
}
