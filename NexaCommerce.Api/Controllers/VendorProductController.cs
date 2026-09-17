using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaCommerce.Contracts.Catalog.Requests;
using NexaCommerce.Contracts.Catalog.Responses;
using NexaCommerce.Contracts.Common;
using NexaCommerce.Domain.Entities.Catalog;
using NexaCommerce.Domain.Entities.Identity;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/vendor/products")]
public sealed class VendorProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<VendorProductController> _logger;

    public VendorProductController(
        IProductRepository productRepository,
        IProductImageRepository productImageRepository,
        IProductVariantRepository productVariantRepository,
        IVendorRepository vendorRepository,
        ILogger<VendorProductController> logger)
    {
        _productRepository = productRepository;
        _productImageRepository = productImageRepository;
        _productVariantRepository = productVariantRepository;
        _vendorRepository = vendorRepository;
        _logger = logger;
    }

    private async Task<Vendor?> GetCurrentVendorAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return await _vendorRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Get all products owned by the authenticated vendor
    /// </summary>
    [HttpPost("get-my-products")]
    public async Task<IActionResult> GetMyProducts(
        [FromBody] GetVendorProductsRequest? request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await GetCurrentVendorAsync(cancellationToken);
            if (vendor is null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, Response<PaginatedResult<ProductResponse>>.Fail("Vendor store not found for authenticated user.", ResponseCode.Forbidden));
            }

            var filter = new ProductFilter
            {
                SearchTerm = request?.SearchTerm,
                Status = request?.Status,
                PageNumber = request?.PageNumber < 1 ? 1 : request?.PageNumber ?? 1,
                PageSize = request?.PageSize is < 1 or > 100 ? 10 : request?.PageSize ?? 10
            };

            var (products, totalCount) = await _productRepository.GetByVendorIdAsync(vendor.Id, filter, cancellationToken);

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

            return Ok(Response<PaginatedResult<ProductResponse>>.Ok(result, "Vendor products retrieved successfully.", ResponseCode.Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get vendor products");
            return StatusCode(StatusCodes.Status500InternalServerError, Response<PaginatedResult<ProductResponse>>.Fail("Failed to fetch vendor products.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Get vendor product details with gallery and variants
    /// </summary>
    [HttpPost("get-product")]
    public async Task<IActionResult> GetProductById([FromBody] GetProductByIdRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await GetCurrentVendorAsync(cancellationToken);
            if (vendor is null) return StatusCode(StatusCodes.Status403Forbidden, Response<ProductDetailResponse>.Fail("Access denied.", ResponseCode.Forbidden));

            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
            {
                return NotFound(Response<ProductDetailResponse>.Fail("Product not found.", ResponseCode.NotFound));
            }

            if (product.VendorId != vendor.Id && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, Response<ProductDetailResponse>.Fail("You do not own this product.", ResponseCode.Forbidden));
            }

            var images = await _productImageRepository.GetByProductIdAsync(product.Id, cancellationToken);
            var variants = await _productVariantRepository.GetByProductIdAsync(product.Id, cancellationToken);

            var detail = new ProductDetailResponse
            {
                Id = product.Id,
                VendorId = product.VendorId,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                Title = product.Title,
                Slug = product.Slug,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Sku = product.Sku,
                Price = product.Price,
                CompareAtPrice = product.CompareAtPrice,
                CostPrice = product.CostPrice,
                StockQuantity = product.StockQuantity,
                PrimaryImageUrl = product.PrimaryImageUrl,
                Status = product.Status,
                RejectionReason = product.RejectionReason,
                IsActive = product.IsActive,
                CreatedAtUtc = product.CreatedAtUtc,
                CategoryName = product.CategoryName,
                BrandName = product.BrandName,
                Images = images.Select(i => new ProductImageResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    VariantId = i.VariantId,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    AltText = i.AltText,
                    SortOrder = i.SortOrder,
                    IsPrimary = i.IsPrimary,
                    CreatedAtUtc = i.CreatedAtUtc
                }).ToList(),
                Variants = variants.Select(v => new ProductVariantResponse
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    Sku = v.Sku,
                    Title = v.Title,
                    Price = v.Price,
                    CompareAtPrice = v.CompareAtPrice,
                    StockQuantity = v.StockQuantity,
                    AttributesJson = v.AttributesJson,
                    IsActive = v.IsActive
                }).ToList()
            };

            return Ok(Response<ProductDetailResponse>.Ok(detail, "Product retrieved successfully.", ResponseCode.Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get product by id {Id}", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<ProductDetailResponse>.Fail("Failed to fetch product.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Create a new product for the authenticated vendor
    /// </summary>
    [HttpPost("create-product")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await GetCurrentVendorAsync(cancellationToken);
            if (vendor is null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, Response<Product>.Fail("Access denied.", ResponseCode.Forbidden));
            }

            var baseSlug = GenerateSlug(request.Title);
            var uniqueSlug = $"{baseSlug}-{Guid.NewGuid().ToString()[..8]}";

            var product = new Product
            {
                Id = Guid.NewGuid(),
                VendorId = vendor.Id,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                Title = request.Title.Trim(),
                Slug = uniqueSlug,
                ShortDescription = request.ShortDescription?.Trim(),
                Description = request.Description?.Trim(),
                Sku = request.Sku?.Trim(),
                Price = request.Price,
                CompareAtPrice = request.CompareAtPrice,
                CostPrice = request.CostPrice,
                StockQuantity = request.StockQuantity,
                PrimaryImageUrl = request.PrimaryImageUrl?.Trim(),
                Status = ProductStatus.Draft,
                IsActive = true
            };

            var createdProduct = await _productRepository.InsertOrUpdateAsync(product, cancellationToken);

            if (request.Images is { Count: > 0 })
            {
                foreach (var img in request.Images)
                {
                    await _productImageRepository.InsertOrUpdateAsync(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = createdProduct.Id,
                        VariantId = img.VariantId,
                        ImageUrl = img.ImageUrl.Trim(),
                        ThumbnailUrl = img.ThumbnailUrl?.Trim(),
                        AltText = img.AltText?.Trim(),
                        SortOrder = img.SortOrder,
                        IsPrimary = img.IsPrimary
                    }, cancellationToken);
                }
            }

            if (request.Variants is { Count: > 0 })
            {
                foreach (var v in request.Variants)
                {
                    await _productVariantRepository.InsertOrUpdateAsync(new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        ProductId = createdProduct.Id,
                        Sku = v.Sku.Trim(),
                        Title = v.Title.Trim(),
                        Price = v.Price,
                        CompareAtPrice = v.CompareAtPrice,
                        StockQuantity = v.StockQuantity,
                        AttributesJson = v.AttributesJson,
                        IsActive = true
                    }, cancellationToken);
                }
            }

            return Ok(Response<Product>.Ok(createdProduct, "Product created successfully as Draft.", ResponseCode.Created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product");
            return StatusCode(StatusCodes.Status500InternalServerError, Response<Product>.Fail("Failed to create product.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Update product details
    /// </summary>
    [HttpPost("update-product")]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await GetCurrentVendorAsync(cancellationToken);
            if (vendor is null) return StatusCode(StatusCodes.Status403Forbidden, Response<Product>.Fail("Access denied.", ResponseCode.Forbidden));

            var existing = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existing is null)
            {
                return NotFound(Response<Product>.Fail("Product not found.", ResponseCode.NotFound));
            }

            if (existing.VendorId != vendor.Id && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, Response<Product>.Fail("You do not own this product.", ResponseCode.Forbidden));
            }

            existing.CategoryId = request.CategoryId;
            existing.BrandId = request.BrandId;
            existing.Title = request.Title.Trim();
            existing.ShortDescription = request.ShortDescription?.Trim();
            existing.Description = request.Description?.Trim();
            existing.Sku = request.Sku?.Trim();
            existing.Price = request.Price;
            existing.CompareAtPrice = request.CompareAtPrice;
            existing.CostPrice = request.CostPrice;
            existing.StockQuantity = request.StockQuantity;
            if (!string.IsNullOrEmpty(request.PrimaryImageUrl))
            {
                existing.PrimaryImageUrl = request.PrimaryImageUrl.Trim();
            }
            existing.IsActive = request.IsActive;

            var updated = await _productRepository.InsertOrUpdateAsync(existing, cancellationToken);
            return Ok(Response<Product>.Ok(updated, "Product updated successfully.", ResponseCode.Updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product {Id}", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<Product>.Fail("Failed to update product.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Submit product for administrator review and approval
    /// </summary>
    [HttpPost("submit-product")]
    public async Task<IActionResult> SubmitForReview([FromBody] SubmitProductRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await GetCurrentVendorAsync(cancellationToken);
            if (vendor is null) return StatusCode(StatusCodes.Status403Forbidden, Response<Product>.Fail("Access denied.", ResponseCode.Forbidden));

            var existing = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existing is null) return NotFound(Response<Product>.Fail("Product not found.", ResponseCode.NotFound));

            if (existing.VendorId != vendor.Id) return StatusCode(StatusCodes.Status403Forbidden, Response<Product>.Fail("You do not own this product.", ResponseCode.Forbidden));

            var updated = await _productRepository.UpdateStatusAsync(request.Id, ProductStatus.PendingApproval, null, cancellationToken);
            return Ok(Response<Product>.Ok(updated!, "Product submitted for review successfully.", ResponseCode.Updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit product {Id} for review", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<Product>.Fail("Failed to submit product.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Soft delete a product
    /// </summary>
    [HttpPost("delete-product")]
    public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await GetCurrentVendorAsync(cancellationToken);
            if (vendor is null) return StatusCode(StatusCodes.Status403Forbidden, Response<object?>.Fail("Access denied.", ResponseCode.Forbidden));

            var existing = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existing is null) return NotFound(Response<object?>.Fail("Product not found.", ResponseCode.NotFound));

            if (existing.VendorId != vendor.Id && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, Response<object?>.Fail("You do not own this product.", ResponseCode.Forbidden));
            }

            await _productRepository.SoftDeleteAsync(request.Id, vendor.Id, cancellationToken);
            return Ok(Response<object?>.Ok(null, "Product deleted successfully.", ResponseCode.Deleted));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product {Id}", request.Id);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<object?>.Fail("Failed to delete product.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Add an image to the product gallery (synchronizes primary image automatically via stored procedure)
    /// </summary>
    [HttpPost("add-image")]
    public async Task<IActionResult> AddProductImage([FromBody] CreateProductImageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await GetCurrentVendorAsync(cancellationToken);
            if (vendor is null) return StatusCode(StatusCodes.Status403Forbidden, Response<ProductImage>.Fail("Access denied.", ResponseCode.Forbidden));

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null) return NotFound(Response<ProductImage>.Fail("Product not found.", ResponseCode.NotFound));

            if (product.VendorId != vendor.Id) return StatusCode(StatusCodes.Status403Forbidden, Response<ProductImage>.Fail("You do not own this product.", ResponseCode.Forbidden));

            var newImage = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                ImageUrl = request.ImageUrl.Trim(),
                ThumbnailUrl = request.ThumbnailUrl?.Trim(),
                AltText = request.AltText?.Trim(),
                SortOrder = request.SortOrder,
                IsPrimary = request.IsPrimary
            };

            var saved = await _productImageRepository.InsertOrUpdateAsync(newImage, cancellationToken);
            return Ok(Response<ProductImage>.Ok(saved, "Image added successfully.", ResponseCode.Created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add image to product {Id}", request.ProductId);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<ProductImage>.Fail("Failed to add product image.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Set a specific gallery image as primary
    /// </summary>
    [HttpPost("set-primary-image")]
    public async Task<IActionResult> SetPrimaryImage([FromBody] SetPrimaryImageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await GetCurrentVendorAsync(cancellationToken);
            if (vendor is null) return StatusCode(StatusCodes.Status403Forbidden, Response<ProductImage>.Fail("Access denied.", ResponseCode.Forbidden));

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null) return NotFound(Response<ProductImage>.Fail("Product not found.", ResponseCode.NotFound));

            if (product.VendorId != vendor.Id) return StatusCode(StatusCodes.Status403Forbidden, Response<ProductImage>.Fail("You do not own this product.", ResponseCode.Forbidden));

            var updated = await _productImageRepository.SetPrimaryAsync(request.ImageId, request.ProductId, cancellationToken);
            return Ok(Response<ProductImage>.Ok(updated!, "Primary image updated successfully.", ResponseCode.Updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set primary image {ImageId} for product {Id}", request.ImageId, request.ProductId);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<ProductImage>.Fail("Failed to set primary image.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Delete a gallery image
    /// </summary>
    [HttpPost("delete-image")]
    public async Task<IActionResult> DeleteProductImage([FromBody] DeleteProductImageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await GetCurrentVendorAsync(cancellationToken);
            if (vendor is null) return StatusCode(StatusCodes.Status403Forbidden, Response<object?>.Fail("Access denied.", ResponseCode.Forbidden));

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null) return NotFound(Response<object?>.Fail("Product not found.", ResponseCode.NotFound));

            if (product.VendorId != vendor.Id) return StatusCode(StatusCodes.Status403Forbidden, Response<object?>.Fail("You do not own this product.", ResponseCode.Forbidden));

            await _productImageRepository.DeleteAsync(request.ImageId, request.ProductId, cancellationToken);
            return Ok(Response<object?>.Ok(null, "Image deleted successfully.", ResponseCode.Deleted));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image {ImageId} for product {Id}", request.ImageId, request.ProductId);
            return StatusCode(StatusCodes.Status500InternalServerError, Response<object?>.Fail("Failed to delete product image.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}
