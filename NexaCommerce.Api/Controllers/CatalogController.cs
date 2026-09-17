using Microsoft.AspNetCore.Mvc;
using NexaCommerce.Contracts.Catalog.Requests;
using NexaCommerce.Contracts.Catalog.Responses;
using NexaCommerce.Contracts.Common;
using NexaCommerce.Domain.Entities.Catalog;
using NexaCommerce.Domain.Interfaces;

namespace NexaCommerce.Api.Controllers;

[ApiController]
[Route("api/v1/catalog")]
public sealed class CatalogController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(
        IProductRepository productRepository,
        IProductImageRepository productImageRepository,
        IProductVariantRepository productVariantRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        ILogger<CatalogController> logger)
    {
        _productRepository = productRepository;
        _productImageRepository = productImageRepository;
        _productVariantRepository = productVariantRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _logger = logger;
    }

    /// <summary>
    /// Public storefront product search & browse with faceted filtering
    /// </summary>
    [HttpPost("get-products")]
    public async Task<IActionResult> GetProducts(
        [FromBody] GetProductsRequest? request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = new ProductFilter
            {
                SearchTerm = request?.SearchTerm,
                CategoryId = request?.CategoryId,
                BrandId = request?.BrandId,
                VendorSlug = request?.VendorSlug,
                MinPrice = request?.MinPrice,
                MaxPrice = request?.MaxPrice,
                SortBy = request?.SortBy,
                PageNumber = request?.PageNumber < 1 ? 1 : request?.PageNumber ?? 1,
                PageSize = request?.PageSize is < 1 or > 100 ? 12 : request?.PageSize ?? 12
            };

            var (products, totalCount) = await _productRepository.GetAllAsync(filter, cancellationToken);

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

            return Ok(Response<PaginatedResult<ProductResponse>>.Ok(result, "Products retrieved successfully.", ResponseCode.Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch public catalog products");
            return StatusCode(StatusCodes.Status500InternalServerError,
                Response<PaginatedResult<ProductResponse>>.Fail("Failed to fetch products.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Public storefront product details by URL slug
    /// </summary>
    [HttpPost("get-product-by-slug")]
    public async Task<IActionResult> GetProductBySlug([FromBody] GetProductBySlugRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Slug))
            {
                return BadRequest(Response<ProductDetailResponse>.Fail("Slug is required.", ResponseCode.Invalid));
            }

            var product = await _productRepository.GetBySlugAsync(request.Slug.Trim(), cancellationToken);
            if (product is null)
            {
                return NotFound(Response<ProductDetailResponse>.Fail($"Product with slug '{request.Slug}' was not found.", ResponseCode.NotFound));
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
                IsActive = product.IsActive,
                CreatedAtUtc = product.CreatedAtUtc,
                VendorStoreName = product.VendorStoreName,
                VendorSlug = product.VendorSlug,
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

            return Ok(Response<ProductDetailResponse>.Ok(detail, "Product details retrieved successfully.", ResponseCode.Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch product by slug {Slug}", request?.Slug);
            return StatusCode(StatusCodes.Status500InternalServerError,
                Response<ProductDetailResponse>.Fail("Failed to fetch product details.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Get all active product categories
    /// </summary>
    [HttpGet("get-categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            var response = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                ParentId = c.ParentId,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive
            });

            return Ok(Response<IEnumerable<CategoryResponse>>.Ok(response, "Categories retrieved successfully.", ResponseCode.Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch categories");
            return StatusCode(StatusCodes.Status500InternalServerError,
                Response<IEnumerable<CategoryResponse>>.Fail("Failed to fetch categories.", ResponseCode.Failed, new() { ex.Message }));
        }
    }

    /// <summary>
    /// Get all active brands
    /// </summary>
    [HttpGet("get-brands")]
    public async Task<IActionResult> GetBrands(CancellationToken cancellationToken = default)
    {
        try
        {
            var brands = await _brandRepository.GetAllAsync(cancellationToken);
            var response = brands.Select(b => new BrandResponse
            {
                Id = b.Id,
                Name = b.Name,
                Slug = b.Slug,
                Description = b.Description,
                LogoUrl = b.LogoUrl,
                IsActive = b.IsActive
            });

            return Ok(Response<IEnumerable<BrandResponse>>.Ok(response, "Brands retrieved successfully.", ResponseCode.Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch brands");
            return StatusCode(StatusCodes.Status500InternalServerError,
                Response<IEnumerable<BrandResponse>>.Fail("Failed to fetch brands.", ResponseCode.Failed, new() { ex.Message }));
        }
    }
}
