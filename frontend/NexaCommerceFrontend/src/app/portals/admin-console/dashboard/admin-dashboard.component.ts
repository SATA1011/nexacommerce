import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { AdminService, UserWithRoles } from '../../../core/services/admin.service';
import { VendorService, StoreResponse } from '../../../core/services/vendor.service';
import { RoleResponse } from '../../../core/models/auth.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  readonly authService = inject(AuthService);
  private readonly adminService = inject(AdminService);
  private readonly vendorService = inject(VendorService);

  // Navigation tab state
  activeTab = signal<'users' | 'vendors' | 'overview'>('users');

  // Users state
  users = signal<UserWithRoles[]>([]);
  loadingUsers = signal<boolean>(false);
  totalUsers = signal<number>(0);
  searchTerm = '';
  pageNumber = signal<number>(1);
  pageSize = signal<number>(15);

  // Roles state
  roles = signal<RoleResponse[]>([]);
  loadingRoles = signal<boolean>(false);

  // Role Assignment Modal state
  selectedUser = signal<UserWithRoles | null>(null);
  selectedUserRoles = signal<string[]>([]);
  modalLoading = signal<boolean>(false);
  modalFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);

  // Vendors state
  vendors = signal<StoreResponse[]>([]);
  loadingVendors = signal<boolean>(false);
  totalVendors = signal<number>(0);
  vendorSearchTerm = '';
  vendorStatusFilter = '';
  vendorActionFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);

  // Overview metrics
  get pendingStores(): StoreResponse[] {
    return this.vendors().filter(v => v.status?.toLowerCase() === 'pending');
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadRoles();
    this.loadVendors();
  }

  loadUsers(): void {
    this.loadingUsers.set(true);
    this.adminService.getUsers(this.searchTerm, this.pageNumber(), this.pageSize()).subscribe({
      next: (res) => {
        this.loadingUsers.set(false);
        const usersList: UserWithRoles[] = res.items || [];
        this.totalUsers.set(res.totalCount || usersList.length);
        this.users.set(usersList);

        // Fetch roles for each user to display badges
        usersList.forEach((user, index) => {
          this.adminService.getUserRoles(user.id).subscribe({
            next: (userRoles) => {
              this.users.update(current => {
                const updated = [...current];
                if (updated[index]) {
                  updated[index] = { ...updated[index], roles: userRoles };
                }
                return updated;
              });
            }
          });
        });
      },
      error: () => {
        this.loadingUsers.set(false);
      }
    });
  }

  loadRoles(): void {
    this.loadingRoles.set(true);
    this.adminService.getRoles().subscribe({
      next: (res) => {
        this.loadingRoles.set(false);
        this.roles.set(res.items || []);
      },
      error: () => {
        this.loadingRoles.set(false);
      }
    });
  }

  onSearch(): void {
    this.pageNumber.set(1);
    this.loadUsers();
  }

  openRoleModal(user: UserWithRoles): void {
    this.selectedUser.set(user);
    this.modalFeedback.set(null);
    this.modalLoading.set(true);

    this.adminService.getUserRoles(user.id).subscribe({
      next: (roles) => {
        this.selectedUserRoles.set(roles || []);
        this.modalLoading.set(false);
      },
      error: () => {
        this.selectedUserRoles.set([]);
        this.modalLoading.set(false);
      }
    });
  }

  closeRoleModal(): void {
    this.selectedUser.set(null);
    this.selectedUserRoles.set([]);
    this.modalFeedback.set(null);
  }

  hasRole(roleName: string): boolean {
    return this.selectedUserRoles().some(r => r.toLowerCase() === roleName.toLowerCase());
  }

  toggleRole(role: RoleResponse): void {
    const user = this.selectedUser();
    if (!user) return;

    this.modalLoading.set(true);
    this.modalFeedback.set(null);

    const isAssigned = this.hasRole(role.name);

    if (isAssigned) {
      this.adminService.removeRole(user.id, role.id).subscribe({
        next: () => {
          this.selectedUserRoles.update(roles => roles.filter(r => r.toLowerCase() !== role.name.toLowerCase()));
          this.modalFeedback.set({ type: 'success', message: `Role "${role.name}" revoked successfully.` });
          this.modalLoading.set(false);
          this.updateUserRolesInTable(user.id, this.selectedUserRoles());
        },
        error: (err) => {
          this.modalFeedback.set({ type: 'error', message: err.error?.message || `Failed to revoke role "${role.name}".` });
          this.modalLoading.set(false);
        }
      });
    } else {
      this.adminService.assignRole(user.id, role.id).subscribe({
        next: () => {
          this.selectedUserRoles.update(roles => [...roles, role.name]);
          this.modalFeedback.set({ type: 'success', message: `Role "${role.name}" assigned successfully.` });
          this.modalLoading.set(false);
          this.updateUserRolesInTable(user.id, this.selectedUserRoles());
        },
        error: (err) => {
          this.modalFeedback.set({ type: 'error', message: err.error?.message || `Failed to assign role "${role.name}".` });
          this.modalLoading.set(false);
        }
      });
    }
  }

  private updateUserRolesInTable(userId: string, roles: string[]): void {
    this.users.update(current =>
      current.map(u => u.id === userId ? { ...u, roles: [...roles] } : u)
    );
  }

  getInitials(firstName: string, lastName: string): string {
    const first = firstName ? firstName[0].toUpperCase() : '';
    const last = lastName ? lastName[0].toUpperCase() : '';
    return (first + last) || 'U';
  }

  loadVendors(): void {
    this.loadingVendors.set(true);
    this.vendorService.getStores(this.vendorSearchTerm, this.vendorStatusFilter).subscribe({
      next: (res) => {
        this.loadingVendors.set(false);
        this.vendors.set(res.items || []);
        this.totalVendors.set(res.totalCount ?? (res.items ? res.items.length : 0));
      },
      error: () => {
        this.loadingVendors.set(false);
      }
    });
  }

  onVendorSearch(): void {
    this.loadVendors();
  }

  updateVendorStatus(storeId: string, status: string, isVerified: boolean): void {
    this.vendorActionFeedback.set(null);
    this.vendorService.updateStoreStatus(storeId, status, isVerified).subscribe({
      next: (updated) => {
        this.vendorActionFeedback.set({
          type: 'success',
          message: `Store "${updated.storeName}" is now ${status}!`
        });
        this.loadVendors();
        setTimeout(() => this.vendorActionFeedback.set(null), 4000);
      },
      error: (err) => {
        this.vendorActionFeedback.set({
          type: 'error',
          message: err.error?.message || 'Failed to update store status.'
        });
      }
    });
  }
}
