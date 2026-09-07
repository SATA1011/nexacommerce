import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface RegisterStoreRequest {
  storeName: string;
  slug: string;
  description?: string;
  taxNumber?: string;
}

export interface StoreResponse {
  id: string;
  userId: string;
  storeName: string;
  slug: string;
  description: string | null;
  taxNumber: string | null;
  commissionRate: number;
  status: string;
  isVerified: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class VendorService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Customer`;

  /** Register a new store (authenticated user becomes a seller) */
  registerStore(request: RegisterStoreRequest): Observable<StoreResponse> {
    return this.http.post<StoreResponse>(`${this.baseUrl}/register-store`, request);
  }

  /** Get the current user's store profile */
  getMyStore(): Observable<StoreResponse> {
    return this.http.post<StoreResponse>(`${this.baseUrl}/my-store`, {});
  }
}
