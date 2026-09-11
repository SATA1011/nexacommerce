import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { VendorService, StoreResponse } from '../../../core/services/vendor.service';

@Component({
  selector: 'app-vendor-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './vendor-dashboard.component.html',
  styleUrl: './vendor-dashboard.component.scss'
})
export class VendorDashboardComponent implements OnInit {
  readonly authService = inject(AuthService);
  private readonly vendorService = inject(VendorService);

  store = signal<StoreResponse | null>(null);
  loading = signal(true);
  storeNotFound = signal(false);

  merchantProducts = [
    { name: 'Pro Wireless Noise-Cancelling Headphones', sku: 'AERO-NC-001', category: 'Audio & Sound', price: 299.99, stock: 45, status: 'Approved' },
    { name: 'True Wireless Studio Earbuds ANC', sku: 'AERO-EB-002', category: 'Audio & Sound', price: 189.00, stock: 8, status: 'Approved' },
    { name: 'Ultra-Fi USB-C Studio DAC & Amp', sku: 'AERO-DAC-003', category: 'Accessories', price: 129.50, stock: 62, status: 'Pending Approval' },
    { name: 'Noise-Isolating Memory Foam Ear Pads', sku: 'AERO-ACC-004', category: 'Accessories', price: 24.99, stock: 120, status: 'Draft' }
  ];

  ngOnInit(): void {
    this.loadStore();
  }

  loadStore(): void {
    this.loading.set(true);
    this.vendorService.getMyStore().subscribe({
      next: (data) => {
        this.store.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 404) {
          this.storeNotFound.set(true);
        }
      }
    });
  }
}
