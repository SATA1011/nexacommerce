export interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  isActive: boolean;
  isEmailConfirmed: boolean;
  createdAtUtc: string;
  lastLoginAtUtc?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: UserResponse;
}

export interface LoginRequest {
  email: string;
  password?: string;
  deviceFingerprint?: string;
  ipAddress?: string;
}

export interface RegisterUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password?: string;
  phoneNumber?: string;
}

export interface RegisterVendorRequest {
  firstName: string;
  lastName: string;
  email: string;
  password?: string;
  phoneNumber?: string;
  storeName: string;
  taxNumber?: string;
  businessAddress?: string;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
  deviceFingerprint?: string;
}

export interface RoleResponse {
  id: string;
  name: string;
  normalizedName: string;
  description?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiResponse<T = unknown> {
  success?: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}
