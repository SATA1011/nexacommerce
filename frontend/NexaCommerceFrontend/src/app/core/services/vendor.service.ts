import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Response<T> {
  data: T;
  errors: string[];
  success: boolean;
  code: string;
  message: string;
}

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
  description?: string;
  taxNumber?: string;
  commissionRate: number;
  status: string;
  isVerified: boolean;
  createdAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class VendorService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vendor`;

  registerStore(payload: RegisterStoreRequest): Observable<StoreResponse> {
    return this.http.post<Response<StoreResponse>>(`${this.baseUrl}/register-store`, payload)
      .pipe(map(res => res.data));
  }

  getMyStore(): Observable<StoreResponse> {
    return this.http.post<Response<StoreResponse>>(`${this.baseUrl}/my-store`, {})
      .pipe(map(res => res.data));
  }

  getStores(searchTerm?: string, status?: string, pageNumber: number = 1, pageSize: number = 10): Observable<{ items: StoreResponse[]; totalCount: number; pageNumber: number; pageSize: number; hasNextPage: boolean }> {
    return this.http.post<Response<{ items: StoreResponse[]; totalCount: number; pageNumber: number; pageSize: number; hasNextPage: boolean }>>(
      `${this.baseUrl}/get-stores`,
      { searchTerm, status, pageNumber, pageSize }
    ).pipe(map(res => res.data));
  }

  updateStoreStatus(storeId: string, status: string, isVerified: boolean): Observable<StoreResponse> {
    return this.http.post<Response<StoreResponse>>(`${this.baseUrl}/update-store-status`, {
      storeId,
      status,
      isVerified
    }).pipe(map(res => res.data));
  }
}
