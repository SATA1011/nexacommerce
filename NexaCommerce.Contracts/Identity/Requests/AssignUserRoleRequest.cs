namespace NexaCommerce.Contracts.Identity.Requests;

public class AssignUserRoleRequest
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
