using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaCommerce.Contracts.Catalog.Requests;
using NexaCommerce.Contracts.Catalog.Responses;
using NexaCommerce.Contracts.Common;
using NexaCommerce.Domain.Entities.Catalog;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Api.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
[ApiController]
[Route("api/v1/admin/products")]
public sealed class AdminProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ILogger<AdminProductController> _logger;

    public AdminProductController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        ILogger<AdminProductController> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _logger = logger;
    }

    /// <summary>
    /// Admin moderation queue for reviewing vendor product submissions
    /// </summary>
    [HttpPost("moderation")]
    public async Task<IActionResult> GetProductsForModeration(
        [FromBody] GetModerationProductsRequest? request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = new ProductFilter
            {
                SearchTerm = request?.SearchTerm,
                Status = request?.Status,
                PageNumber = request?.PageNumber < 1 ? 1 : request?.PageNumber ?? 1,
                PageSize = request?.PageSize is < 1 or > 100 ? 15 : request?.PageSize ?? 15
            };

            var (products, totalCount) = await _productRepository.GetForModerationAsync(filter, cancellationToken);

            var items = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                VendorId = p.VendorId,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                Title = p.Title,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Sku = p.Sku,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                StockQuantity = p.StockQuantity,
                PrimaryImageUrl = p.PrimaryImageUrl,
                Status = p.Status,
                IsActive = p.IsActive,
                CreatedAtUtc = p.CreatedAtUtc,
                VendorStoreName = p.VendorStoreName,
                VendorSlug = p.VendorSlug,
                CategoryName = p.CategoryName,
                BrandName = p.BrandName
            });

            var result = new PaginatedResult<ProductResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return Ok(Response<PaginatedResult<ProductResponse>>.Ok(result, "Moderation products retrieved successfully.", ResponseCode.Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch moderation products");
            return StatusCode(StatusCodes.Status500InternalServerError, Response<PaginatedResult<ProductResponse>>.Fail("Failed to fetch products for moderation.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Update product approval status (Approve, Reject, Suspend)
    /// </summary>
    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateStatus(
        [FromBody] UpdateProductStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
            {
                return NotFound(Response<Product>.Fail("Product not found.", ResponseCode.NotFound));
            }

            var updated = await _productRepository.UpdateStatusAsync(request.ProductId, request.Status, request.RejectionReason, cancellationToken);
            return Ok(Response<Product>.Ok(updated!, $"Product '{product.Title}' status updated to {request.Status}.", ResponseCode.Updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product status for {Id}", request.ProductId);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<Product>.Fail("Failed to update product status.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Admin: Create or update a category
    /// </summary>
    [HttpPost("save-category")]
    public async Task<IActionResult> CreateOrUpdateCategory([FromBody] Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            if (category.Id == Guid.Empty)
            {
                category.Id = Guid.NewGuid();
            }

            var saved = await _categoryRepository.InsertOrUpdateAsync(category, cancellationToken);
            return Ok(Response<Category>.Ok(saved, "Category saved successfully.", ResponseCode.Created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save category");
            return StatusCode(StatusCodes.Status500InternalServerError, Response<Category>.Fail("Failed to save category.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Admin: Delete a category
    /// </summary>
    [HttpPost("delete-category")]
    public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _categoryRepository.SoftDeleteAsync(request.Id, cancellationToken);
            return Ok(Response<object?>.Ok(null, "Category deleted successfully.", ResponseCode.Deleted));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete category {Id}", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<object?>.Fail("Failed to delete category.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Admin: Create or update a brand
    /// </summary>
    [HttpPost("save-brand")]
    public async Task<IActionResult> CreateOrUpdateBrand([FromBody] Brand brand, CancellationToken cancellationToken = default)
    {
        try
        {
            if (brand.Id == Guid.Empty)
            {
                brand.Id = Guid.NewGuid();
            }

            var saved = await _brandRepository.InsertOrUpdateAsync(brand, cancellationToken);
            return Ok(Response<Brand>.Ok(saved, "Brand saved successfully.", ResponseCode.Created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save brand");
            return StatusCode(StatusCodes.Status500InternalServerError, Response<Brand>.Fail("Failed to save brand.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Admin: Delete a brand
    /// </summary>
    [HttpPost("delete-brand")]
    public async Task<IActionResult> DeleteBrand([FromBody] DeleteBrandRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _brandRepository.SoftDeleteAsync(request.Id, cancellationToken);
            return Ok(Response<object?>.Ok(null, "Brand deleted successfully.", ResponseCode.Deleted));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete brand {Id}", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<object?>.Fail("Failed to delete brand.", ResponseCode.Failed, new() { ex.Message }));
        }
    }
}
