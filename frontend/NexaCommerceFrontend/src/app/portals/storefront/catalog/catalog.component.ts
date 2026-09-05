import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface ProductItem {
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
  imports: [CommonModule, FormsModule],
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss'
})
export class CatalogComponent {
  searchQuery = '';
  sortBy = 'featured';
  selectedCategory: string | null = null;
  maxPrice = 1500;
  onlyInStock = false;

  filterCategories = ['Electronics', 'Computing', 'Audio & Sound', 'Wearables', 'Accessories'];

  products: ProductItem[] = [
    {
      id: 'p1',
      title: 'Pro Wireless Noise-Cancelling Headphones',
      storeName: 'AeroTech Official',
      category: 'Audio & Sound',
      price: 299.99,
      originalPrice: 349.99,
      rating: 4.9,
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
      rating: 4.8,
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
      rating: 4.7,
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
      rating: 4.9,
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
      rating: 4.6,
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
      rating: 4.8,
      reviewsCount: 304,
      imageUrl: 'https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=600&auto=format&fit=crop&q=80',
      badge: 'Popular',
      inStock: true
    }
  ];

  toggleCategory(cat: string): void {
    this.selectedCategory = this.selectedCategory === cat ? null : cat;
  }

  filteredProducts(): ProductItem[] {
    return this.products.filter((p) => {
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
