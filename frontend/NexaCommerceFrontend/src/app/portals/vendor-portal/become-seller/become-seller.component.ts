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
  readonly vendorService = inject(VendorService);
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Guest Account fields (when not logged in)
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  phoneNumber = '';
  showPassword = signal(false);

  // Store fields
  storeName = '';
  slug = '';
  description = '';
  taxNumber = '';
  businessAddress = '';

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

    this.loading.set(true);
    this.errorMessage.set(null);

    if (this.isAuthenticated()) {
      // Logged-in user: register store directly
      const payload: RegisterStoreRequest = {
        storeName: this.storeName.trim(),
        slug: this.slug.trim(),
        description: this.description.trim() || undefined,
        taxNumber: this.taxNumber.trim() || undefined,
      };

      this.vendorService.registerStore(payload).subscribe({
        next: (store) => {
          this.loading.set(false);
          this.successMessage.set(`Store "${store.storeName}" registered! Status: ${store.status}. Admin will review shortly.`);
          setTimeout(() => this.router.navigate(['/vendor/dashboard']), 2200);
        },
        error: (err) => {
          this.loading.set(false);
          const msg = err.error?.message || err.error?.detail || 'Failed to register store. Please try again.';
          this.errorMessage.set(msg);
        }
      });
    } else {
      // Guest user: register account and store together
      if (!this.firstName || !this.lastName || !this.email || !this.password) {
        this.loading.set(false);
        this.errorMessage.set('Please fill in your name, email, and password.');
        return;
      }

      this.authService.registerVendor({
        firstName: this.firstName.trim(),
        lastName: this.lastName.trim(),
        email: this.email.trim(),
        password: this.password,
        phoneNumber: this.phoneNumber?.trim() || undefined,
        storeName: this.storeName.trim(),
        taxNumber: this.taxNumber?.trim() || undefined,
        businessAddress: this.businessAddress?.trim() || undefined
      }).subscribe({
        next: () => {
          this.loading.set(false);
          this.successMessage.set(`Seller account and store "${this.storeName}" submitted! Admin will review shortly.`);
          setTimeout(() => this.router.navigate(['/auth/login']), 2500);
        },
        error: (err) => {
          this.loading.set(false);
          const msg = err.error?.message || err.error?.detail || 'Failed to register seller account. Please try again.';
          this.errorMessage.set(msg);
        }
      });
    }
  }
}
