import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, switchMap, of } from 'rxjs';
import { TokenStorageService } from './token-storage.service';
import {
  AuthResponse,
  LoginRequest,
  RegisterUserRequest,
  RegisterVendorRequest,
  RefreshTokenRequest,
  ApiResponse
} from '../models/auth.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly baseUrl = `${environment.apiUrl}/Account`;
  private readonly rolesUrl = `${environment.apiUrl}/Roles`;

  // Expose signals from tokenStorage
  readonly currentUser = this.tokenStorage.currentUser;
  readonly isAuthenticated = this.tokenStorage.isAuthenticated;
  readonly userRoles = this.tokenStorage.userRoles;

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, credentials).pipe(
      tap((res) => {
        if (res && res.accessToken) {
          this.tokenStorage.saveTokens(res.accessToken, res.refreshToken);
          if (res.user) {
            this.tokenStorage.saveUser(res.user);
            // Fetch user roles to populate permission signals
            this.fetchUserRoles(res.user.id).subscribe();
          }
        }
      })
    );
  }

  registerUser(payload: RegisterUserRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/register-user`, payload);
  }

  registerVendor(payload: RegisterVendorRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/register-vendor`, payload);
  }

  refreshToken(): Observable<AuthResponse> {
    const accessToken = this.tokenStorage.getAccessToken();
    const refreshToken = this.tokenStorage.getRefreshToken();

    if (!accessToken || !refreshToken) {
      this.logout();
      return throwError(() => new Error('No tokens available for refresh'));
    }

    const payload: RefreshTokenRequest = {
      accessToken,
      refreshToken
    };

    return this.http.post<AuthResponse>(`${this.baseUrl}/refresh-token`, payload).pipe(
      tap((res) => {
        if (res && res.accessToken) {
          this.tokenStorage.saveTokens(res.accessToken, res.refreshToken);
          if (res.user) {
            this.tokenStorage.saveUser(res.user);
          }
        }
      }),
      catchError((err) => {
        this.logout();
        return throwError(() => err);
      })
    );
  }

  fetchUserRoles(userId: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.rolesUrl}/user-roles/${userId}`).pipe(
      tap((roles) => {
        if (Array.isArray(roles)) {
          this.tokenStorage.saveRoles(roles);
        }
      }),
      catchError(() => of([]))
    );
  }

  logout(): void {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.baseUrl}/logout`, { refreshToken }).subscribe({
        error: () => {
          // Silent catch on logout failure
        }
      });
    }
    this.tokenStorage.clear();
  }

  hasRole(role: string): boolean {
    return this.tokenStorage.hasRole(role);
  }

  isCustomer(): boolean {
    return this.hasRole('Customer') || this.hasRole('User');
  }

  isVendor(): boolean {
    return this.hasRole('Vendor');
  }

  isAdmin(): boolean {
    return this.hasRole('Admin') || this.hasRole('SuperAdmin');
  }
}
