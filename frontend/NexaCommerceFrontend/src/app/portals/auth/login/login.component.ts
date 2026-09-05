import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  email = '';
  password = '';
  rememberMe = true;
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  onSubmit(): void {
    if (!this.email || !this.password) return;

    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService.login({
      email: this.email,
      password: this.password
    }).subscribe({
      next: () => {
        this.loading.set(false);
        const returnUrl = this.route.snapshot.queryParams['returnUrl'];
        if (returnUrl) {
          this.router.navigateByUrl(returnUrl);
        } else if (this.authService.isAdmin()) {
          this.router.navigate(['/admin/dashboard']);
        } else if (this.authService.isVendor()) {
          this.router.navigate(['/vendor/dashboard']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err) => {
        this.loading.set(false);
        const detail = err.error?.detail || err.error?.message || 'Invalid credentials or service unavailable.';
        this.errorMessage.set(detail);
      }
    });
  }
}
