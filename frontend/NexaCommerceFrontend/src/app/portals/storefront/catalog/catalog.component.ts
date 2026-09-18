import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SliderModule } from 'primeng/slider';
import { RatingModule } from 'primeng/rating';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { CatalogService, ProductItem as ApiProductItem, CategoryItem } from '../../../core/services/catalog.service';

interface DisplayProductItem {
  id: string;
  title: string;
  storeName: string;
  category: string;
  price: number;
  originalPrice?: number;
  rating: number;
  reviewsCount: number;
  imageUrl: string;
  badge?: string;
  inStock: boolean;
}

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SliderModule,
    RatingModule,
    SelectModule,
    CheckboxModule,
    ButtonModule,
    TagModule,
    InputTextModule
  ],
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss'
})
export class CatalogComponent implements OnInit {
  private readonly catalogService = inject(CatalogService);

  searchQuery = '';
  sortBy = 'featured';
  selectedCategory: string | null = null;
  maxPrice = 1500;
  onlyInStock = false;
  isLoading = signal<boolean>(false);

  filterCategories: string[] = ['Electronics', 'Computing', 'Audio & Sound', 'Wearables', 'Accessories'];

  sortOptions = [
    { label: 'Featured First', value: 'featured' },
    { label: 'Price: Low to High', value: 'price-asc' },
    { label: 'Price: High to Low', value: 'price-desc' },
    { label: 'Highest Rated', value: 'rating' }
  ];

  // Default fallback showcase products in case database has no approved vendor listings yet
  defaultProducts: DisplayProductItem[] = [
    {
      id: 'p1',
      title: 'Pro Wireless Noise-Cancelling Headphones',
      storeName: 'AeroTech Official',
      category: 'Audio & Sound',
      price: 299.99,
      originalPrice: 349.99,
      rating: 5,
      reviewsCount: 142,
      imageUrl: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&auto=format&fit=crop&q=80',
      badge: 'Best Seller',
      inStock: true
    },
    {
      id: 'p2',
      title: 'Ultra-Slim 4K OLED Portable Monitor',
      storeName: 'VisionDisplay Store',
      category: 'Computing',
      price: 489.00,
      originalPrice: 529.00,
      rating: 5,
      reviewsCount: 88,
      imageUrl: 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=600&auto=format&fit=crop&q=80',
      badge: 'Trending',
      inStock: true
    },
    {
      id: 'p3',
      title: 'Minimalist Mechanical Keyboard RGB',
      storeName: 'KeyCraft Studio',
      category: 'Accessories',
      price: 139.50,
      rating: 5,
      reviewsCount: 231,
      imageUrl: 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&auto=format&fit=crop&q=80',
      inStock: true
    },
    {
      id: 'p4',
      title: 'Titanium Smart Health Watch Series X',
      storeName: 'PulseGadgets',
      category: 'Wearables',
      price: 349.00,
      originalPrice: 399.00,
      rating: 5,
      reviewsCount: 76,
      imageUrl: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600&auto=format&fit=crop&q=80',
      badge: 'New Arrival',
      inStock: true
    },
    {
      id: 'p5',
      title: 'Ergonomic Vertical Wireless Mouse',
      storeName: 'KeyCraft Studio',
      category: 'Accessories',
      price: 69.99,
      rating: 4,
      reviewsCount: 119,
      imageUrl: 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=600&auto=format&fit=crop&q=80',
      inStock: true
    },
    {
      id: 'p6',
      title: 'True Wireless Studio Earbuds with ANC',
      storeName: 'AeroTech Official',
      category: 'Audio & Sound',
      price: 189.00,
      originalPrice: 219.00,
      rating: 5,
      reviewsCount: 304,
      imageUrl: 'https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=600&auto=format&fit=crop&q=80',
      badge: 'Popular',
      inStock: true
    }
  ];

  products = signal<DisplayProductItem[]>(this.defaultProducts);

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.catalogService.getCategories().subscribe({
      next: (categories: CategoryItem[]) => {
        if (categories && categories.length > 0) {
          this.filterCategories = categories.map(c => c.name);
        }
      },
      error: () => {}
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.catalogService.getProducts({
      searchTerm: this.searchQuery || undefined,
      maxPrice: this.maxPrice || undefined,
      sortBy: this.sortBy
    }).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.items && res.items.length > 0) {
          const mapped: DisplayProductItem[] = res.items.map(p => ({
            id: p.id,
            title: p.title,
            storeName: p.vendorStoreName || 'Marketplace Merchant',
            category: p.categoryName || 'General',
            price: p.price,
            originalPrice: p.compareAtPrice,
            rating: 5,
            reviewsCount: 12,
            imageUrl: p.primaryImageUrl || 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600&auto=format&fit=crop&q=80',
            badge: 'Verified Seller',
            inStock: p.stockQuantity > 0
          }));
          this.products.set(mapped);
        } else {
          this.products.set(this.defaultProducts);
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.products.set(this.defaultProducts);
      }
    });
  }

  toggleCategory(cat: string): void {
    this.selectedCategory = this.selectedCategory === cat ? null : cat;
  }

  filteredProducts(): DisplayProductItem[] {
    return this.products().filter((p) => {
      const matchesSearch = !this.searchQuery ||
        p.title.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        p.storeName.toLowerCase().includes(this.searchQuery.toLowerCase());

      const matchesCat = !this.selectedCategory || p.category === this.selectedCategory;
      const matchesPrice = p.price <= this.maxPrice;
      const matchesStock = !this.onlyInStock || p.inStock;

      return matchesSearch && matchesCat && matchesPrice && matchesStock;
    });
  }
}
