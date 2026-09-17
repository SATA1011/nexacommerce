import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Response<T> {
  data: T;
  errors: string[];
  success: boolean;
  code: string;
  message: string;
}

export interface ProductItem {
  id: string;
  vendorId: string;
  categoryId?: string;
  brandId?: string;
  title: string;
  slug: string;
  shortDescription?: string;
  sku?: string;
  price: number;
  compareAtPrice?: number;
  stockQuantity: number;
  primaryImageUrl?: string;
  status: string;
  isActive: boolean;
  createdAtUtc: string;
  vendorStoreName?: string;
  vendorSlug?: string;
  categoryName?: string;
  brandName?: string;
}

export interface ProductImageItem {
  id: string;
  productId: string;
  variantId?: string;
  imageUrl: string;
  thumbnailUrl?: string;
  altText?: string;
  sortOrder: number;
  isPrimary: boolean;
  createdAtUtc: string;
}

export interface ProductVariantItem {
  id: string;
  productId: string;
  sku: string;
  title: string;
  price: number;
  compareAtPrice?: number;
  stockQuantity: number;
  attributesJson?: string;
  isActive: boolean;
}

export interface ProductDetailItem extends ProductItem {
  description?: string;
  costPrice?: number;
  rejectionReason?: string;
  images: ProductImageItem[];
  variants: ProductVariantItem[];
}

export interface CategoryItem {
  id: string;
  parentId?: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface BrandItem {
  id: string;
  name: string;
  slug: string;
  description?: string;
  logoUrl?: string;
  isActive: boolean;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateProductPayload {
  title: string;
  categoryId?: string;
  brandId?: string;
  shortDescription?: string;
  description?: string;
  sku?: string;
  price: number;
  compareAtPrice?: number;
  costPrice?: number;
  stockQuantity: number;
  primaryImageUrl?: string;
  images?: { imageUrl: string; altText?: string; sortOrder?: number; isPrimary?: boolean }[];
}

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly catalogUrl = `${environment.apiUrl}/catalog`;
  private readonly vendorUrl = `${environment.apiUrl}/vendor/products`;
  private readonly adminUrl = `${environment.apiUrl}/admin/products`;

  // Public Storefront Catalog (POST for multi-parameter query)
  getProducts(payload?: {
    searchTerm?: string;
    categoryId?: string;
    brandId?: string;
    vendorSlug?: string;
    minPrice?: number;
    maxPrice?: number;
    sortBy?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<PaginatedResult<ProductItem>> {
    return this.http.post<Response<PaginatedResult<ProductItem>>>(`${this.catalogUrl}/get-products`, payload || {})
      .pipe(map(res => res.data));
  }

  getProductBySlug(slug: string): Observable<ProductDetailItem> {
    return this.http.post<Response<ProductDetailItem>>(`${this.catalogUrl}/get-product-by-slug`, { slug })
      .pipe(map(res => res.data));
  }

  getCategories(): Observable<CategoryItem[]> {
    return this.http.get<Response<CategoryItem[]>>(`${this.catalogUrl}/get-categories`)
      .pipe(map(res => res.data));
  }

  getBrands(): Observable<BrandItem[]> {
    return this.http.get<Response<BrandItem[]>>(`${this.catalogUrl}/get-brands`)
      .pipe(map(res => res.data));
  }

  // Vendor Portal (POST for multi-parameter query)
  getMyProducts(payload?: {
    searchTerm?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<PaginatedResult<ProductItem>> {
    return this.http.post<Response<PaginatedResult<ProductItem>>>(`${this.vendorUrl}/get-my-products`, payload || {})
      .pipe(map(res => res.data));
  }

  getVendorProduct(id: string): Observable<ProductDetailItem> {
    return this.http.post<Response<ProductDetailItem>>(`${this.vendorUrl}/get-product`, { id })
      .pipe(map(res => res.data));
  }

  createProduct(payload: CreateProductPayload): Observable<ProductItem> {
    return this.http.post<Response<ProductItem>>(`${this.vendorUrl}/create-product`, payload)
      .pipe(map(res => res.data));
  }

  updateProduct(id: string, payload: Partial<CreateProductPayload> & { isActive?: boolean }): Observable<ProductItem> {
    return this.http.post<Response<ProductItem>>(`${this.vendorUrl}/update-product`, { id, ...payload })
      .pipe(map(res => res.data));
  }

  submitForReview(id: string): Observable<Response<ProductItem>> {
    return this.http.post<Response<ProductItem>>(`${this.vendorUrl}/submit-product`, { id });
  }

  deleteProduct(id: string): Observable<Response<object>> {
    return this.http.post<Response<object>>(`${this.vendorUrl}/delete-product`, { id });
  }

  addProductImage(productId: string, image: { imageUrl: string; thumbnailUrl?: string; altText?: string; sortOrder?: number; isPrimary?: boolean }): Observable<ProductImageItem> {
    return this.http.post<Response<ProductImageItem>>(`${this.vendorUrl}/add-image`, { productId, ...image })
      .pipe(map(res => res.data));
  }

  setPrimaryImage(productId: string, imageId: string): Observable<ProductImageItem> {
    return this.http.post<Response<ProductImageItem>>(`${this.vendorUrl}/set-primary-image`, { productId, imageId })
      .pipe(map(res => res.data));
  }

  deleteProductImage(productId: string, imageId: string): Observable<Response<object>> {
    return this.http.post<Response<object>>(`${this.vendorUrl}/delete-image`, { productId, imageId });
  }

  // Admin Console (POST for multi-parameter query)
  getModerationProducts(payload?: {
    searchTerm?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<PaginatedResult<ProductItem>> {
    return this.http.post<Response<PaginatedResult<ProductItem>>>(`${this.adminUrl}/moderation`, payload || {})
      .pipe(map(res => res.data));
  }

  updateProductStatus(id: string, status: string, rejectionReason?: string): Observable<Response<ProductItem>> {
    return this.http.post<Response<ProductItem>>(`${this.adminUrl}/update-status`, {
      productId: id,
      status,
      rejectionReason
    });
  }
}
