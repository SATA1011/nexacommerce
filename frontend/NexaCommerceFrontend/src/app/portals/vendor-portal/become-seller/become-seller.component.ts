import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { VendorService } from '../../../core/services/vendor.service';

@Component({
  selector: 'app-become-seller',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './become-seller.component.html',
  styleUrl: './become-seller.component.scss'
})
export class BecomeSellerComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly vendorService = inject(VendorService);
  private readonly router = inject(Router);

  // Form fields
  storeName = '';
  slug = '';
  description = '';
  taxNumber = '';

  // State
  loading = signal(false);
  checkingStore = signal(true);
  existingStore = signal<any>(null);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  slugManuallyEdited = false;

  ngOnInit(): void {
    // If user is not logged in, redirect to login then come back
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: '/become-seller' } });
      return;
    }

    // Check if the user already has a store
    this.vendorService.getMyStore().subscribe({
      next: (store) => {
        this.existingStore.set(store);
        this.checkingStore.set(false);
      },
      error: () => {
        // 404 means no store yet — good
        this.existingStore.set(null);
        this.checkingStore.set(false);
      }
    });
  }

  onStoreNameChange(): void {
    if (!this.slugManuallyEdited) {
      this.slug = this.storeName
        .toLowerCase()
        .trim()
        .replace(/[^a-z0-9\s-]/g, '')
        .replace(/\s+/g, '-')
        .replace(/-+/g, '-');
    }
  }

  onSlugChange(): void {
    this.slugManuallyEdited = true;
    this.slug = this.slug
      .toLowerCase()
      .replace(/[^a-z0-9-]/g, '')
      .replace(/-+/g, '-');
  }

  onSubmit(): void {
    if (!this.storeName.trim() || !this.slug.trim()) return;

    this.loading.set(true);
    this.errorMessage.set(null);

    this.vendorService.registerStore({
      storeName: this.storeName.trim(),
      slug: this.slug.trim(),
      description: this.description.trim() || undefined,
      taxNumber: this.taxNumber.trim() || undefined
    }).subscribe({
      next: (store) => {
        this.loading.set(false);
        this.successMessage.set(`🎉 Your store "${store.storeName}" has been submitted for review! You'll be notified once approved.`);
        this.existingStore.set(store);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(
          err.error?.message || err.error?.detail || 'Failed to register store. Please try again.'
        );
      }
    });
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return 'approved';
      case 'rejected': return 'rejected';
      case 'suspended': return 'suspended';
      default: return 'pending';
    }
  }

  getStatusIcon(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return '✅';
      case 'rejected': return '❌';
      case 'suspended': return '⚠️';
      default: return '⏳';
    }
  }
}
