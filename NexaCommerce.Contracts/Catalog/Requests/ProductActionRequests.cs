using System.ComponentModel.DataAnnotations;

namespace NexaCommerce.Contracts.Catalog.Requests;

public class GetProductByIdRequest
{
    [Required]
    public Guid Id { get; set; }
}

public class GetProductBySlugRequest
{
    [Required]
    public string Slug { get; set; } = string.Empty;
}

public class SubmitProductRequest
{
    [Required]
    public Guid Id { get; set; }
}

public class DeleteProductRequest
{
    [Required]
    public Guid Id { get; set; }
}

public class SetPrimaryImageRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid ImageId { get; set; }
}

public class DeleteProductImageRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid ImageId { get; set; }
}

public class DeleteCategoryRequest
{
    [Required]
    public Guid Id { get; set; }
}

public class DeleteBrandRequest
{
    [Required]
    public Guid Id { get; set; }
}
