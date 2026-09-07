import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { VendorService, RegisterStoreRequest } from '../../../core/services/vendor.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-become-seller',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './become-seller.component.html',
  styleUrl: './become-seller.component.scss'
})
export class BecomeSellerComponent {
  private readonly vendorService = inject(VendorService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Form fields
  storeName = '';
  slug = '';
  description = '';
  taxNumber = '';

  // UI state
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isAuthenticated = this.authService.isAuthenticated;

  // Auto-generate slug from store name
  onStoreNameChange(): void {
    this.slug = this.storeName
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9\s-]/g, '')
      .replace(/\s+/g, '-')
      .replace(/-+/g, '-');
  }

  onSubmit(): void {
    if (!this.storeName || !this.slug) return;

    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: '/vendor/become-seller' } });
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const payload: RegisterStoreRequest = {
      storeName: this.storeName.trim(),
      slug: this.slug.trim(),
      description: this.description.trim() || undefined,
      taxNumber: this.taxNumber.trim() || undefined,
    };

    this.vendorService.registerStore(payload).subscribe({
      next: (store) => {
        this.loading.set(false);
        this.successMessage.set(`Store "${store.storeName}" registered! Status: ${store.status}. Admin will review and approve shortly.`);
        // Navigate to vendor dashboard after 2 seconds
        setTimeout(() => this.router.navigate(['/vendor/dashboard']), 2500);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err.error?.message || err.error?.detail || 'Failed to register store. Please try again.';
        this.errorMessage.set(msg);
      }
    });
  }
}
