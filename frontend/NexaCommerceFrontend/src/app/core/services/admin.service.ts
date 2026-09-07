import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedResult, RoleResponse, UserResponse } from '../models/auth.models';

export interface AssignRoleRequest {
  userId: string;
  roleId: string;
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
}

export interface UserWithRoles extends UserResponse {
  roles?: string[];
  loadingRoles?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly accountUrl = `${environment.apiUrl}/Account`;
  private readonly rolesUrl = `${environment.apiUrl}/Roles`;

  getUsers(searchTerm = '', pageNumber = 1, pageSize = 25): Observable<PaginatedResult<UserResponse>> {
    return this.http.post<PaginatedResult<UserResponse>>(`${this.accountUrl}/get-users`, {
      searchTerm: searchTerm.trim(),
      pageNumber,
      pageSize
    });
  }

  getRoles(searchTerm = '', pageNumber = 1, pageSize = 50): Observable<PaginatedResult<RoleResponse>> {
    return this.http.post<PaginatedResult<RoleResponse>>(`${this.rolesUrl}/get-roles`, {
      searchTerm: searchTerm.trim(),
      pageNumber,
      pageSize
    });
  }

  getUserRoles(userId: string): Observable<string[]> {
    return this.http.post<string[]>(`${this.rolesUrl}/get-user-roles`, { id: userId });
  }

  assignRole(userId: string, roleId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.rolesUrl}/assign-role`, {
      userId,
      roleId
    });
  }

  removeRole(userId: string, roleId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.rolesUrl}/remove-role`, {
      userId,
      roleId
    });
  }

  createRole(payload: CreateRoleRequest): Observable<RoleResponse> {
    return this.http.post<RoleResponse>(`${this.rolesUrl}/create-role`, payload);
  }
}
