import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  isVendorTab = signal(false);
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  phoneNumber = '';
  storeName = '';
  taxNumber = '';
  businessAddress = '';

  loading = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  ngOnInit(): void {
    const type = this.route.snapshot.queryParams['type'];
    if (type === 'vendor') {
      this.isVendorTab.set(true);
    }
  }

  onSubmit(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    if (this.isVendorTab()) {
      this.authService.registerVendor({
        firstName: this.firstName,
        lastName: this.lastName,
        email: this.email,
        password: this.password,
        phoneNumber: this.phoneNumber || undefined,
        storeName: this.storeName,
        taxNumber: this.taxNumber || undefined,
        businessAddress: this.businessAddress || undefined
      }).subscribe({
        next: () => {
          this.loading.set(false);
          this.successMessage.set('Merchant registration submitted successfully!');
          setTimeout(() => this.router.navigate(['/auth/login']), 1800);
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMessage.set(err.error?.detail || err.error?.message || 'Failed to submit merchant registration.');
        }
      });
    } else {
      this.authService.registerUser({
        firstName: this.firstName,
        lastName: this.lastName,
        email: this.email,
        password: this.password,
        phoneNumber: this.phoneNumber || undefined
      }).subscribe({
        next: () => {
          this.loading.set(false);
          this.successMessage.set('Customer account created successfully!');
          setTimeout(() => this.router.navigate(['/auth/login']), 1800);
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMessage.set(err.error?.detail || err.error?.message || 'Failed to create customer account.');
        }
      });
    }
  }
}
