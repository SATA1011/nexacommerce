import { Injectable, signal, computed } from '@angular/core';
import { UserResponse } from '../models/auth.models';

const ACCESS_TOKEN_KEY = 'nexa_access_token';
const REFRESH_TOKEN_KEY = 'nexa_refresh_token';
const USER_KEY = 'nexa_user_data';
const USER_ROLES_KEY = 'nexa_user_roles';

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private readonly tokenSignal = signal<string | null>(this.getStoredAccessToken());
  private readonly userSignal = signal<UserResponse | null>(this.getStoredUser());
  private readonly rolesSignal = signal<string[]>(this.getStoredRoles());

  // Public readonly Signals for reactive components
  readonly accessToken = this.tokenSignal.asReadonly();
  readonly currentUser = this.userSignal.asReadonly();
  readonly userRoles = this.rolesSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.tokenSignal());

  getAccessToken(): string | null {
    return this.tokenSignal();
  }

  getRefreshToken(): string | null {
    try {
      return localStorage.getItem(REFRESH_TOKEN_KEY);
    } catch {
      return null;
    }
  }

  getUser(): UserResponse | null {
    return this.userSignal();
  }

  getRoles(): string[] {
    return this.rolesSignal();
  }

  saveTokens(accessToken: string, refreshToken: string): void {
    try {
      localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
      localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
      this.tokenSignal.set(accessToken);
    } catch (e) {
      console.error('Failed to store tokens in localStorage', e);
    }
  }

  saveUser(user: UserResponse, roles: string[] = []): void {
    try {
      localStorage.setItem(USER_KEY, JSON.stringify(user));
      localStorage.setItem(USER_ROLES_KEY, JSON.stringify(roles));
      this.userSignal.set(user);
      this.rolesSignal.set(roles);
    } catch (e) {
      console.error('Failed to store user profile in localStorage', e);
    }
  }

  saveRoles(roles: string[]): void {
    try {
      localStorage.setItem(USER_ROLES_KEY, JSON.stringify(roles));
      this.rolesSignal.set(roles);
    } catch (e) {
      console.error('Failed to store roles in localStorage', e);
    }
  }

  clear(): void {
    try {
      localStorage.removeItem(ACCESS_TOKEN_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
      localStorage.removeItem(USER_ROLES_KEY);
    } catch (e) {
      console.error('Failed to clear credentials', e);
    }
    this.tokenSignal.set(null);
    this.userSignal.set(null);
    this.rolesSignal.set([]);
  }

  hasRole(role: string): boolean {
    const roles = this.rolesSignal();
    return roles.some(r => r.toLowerCase() === role.toLowerCase());
  }

  private getStoredAccessToken(): string | null {
    try {
      return localStorage.getItem(ACCESS_TOKEN_KEY);
    } catch {
      return null;
    }
  }

  private getStoredUser(): UserResponse | null {
    try {
      const data = localStorage.getItem(USER_KEY);
      return data ? JSON.parse(data) : null;
    } catch {
      return null;
    }
  }

  private getStoredRoles(): string[] {
    try {
      const data = localStorage.getItem(USER_ROLES_KEY);
      return data ? JSON.parse(data) : [];
    } catch {
      return [];
    }
  }
}
