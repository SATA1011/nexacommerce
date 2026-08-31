namespace NexaCommerce.Contracts.Identity.Responses;

public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public RoleResponse() { }

    public RoleResponse(Guid id, string name, string normalizedName, string? description)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
        Description = description;
    }
}
