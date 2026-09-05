import { Routes } from '@angular/router';
import { HomeComponent } from './portals/storefront/home/home.component';
import { CatalogComponent } from './portals/storefront/catalog/catalog.component';
import { LoginComponent } from './portals/auth/login/login.component';
import { RegisterComponent } from './portals/auth/register/register.component';
import { VendorDashboardComponent } from './portals/vendor-portal/dashboard/vendor-dashboard.component';
import { AdminDashboardComponent } from './portals/admin-console/dashboard/admin-dashboard.component';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  // Public Storefront
  {
    path: '',
    component: HomeComponent,
    title: 'NexaCommerce — Modern Multi-Vendor Marketplace'
  },
  {
    path: 'catalog',
    component: CatalogComponent,
    title: 'Explore Products — NexaCommerce'
  },

  // Authentication
  {
    path: 'auth/login',
    component: LoginComponent,
    title: 'Sign In — NexaCommerce'
  },
  {
    path: 'auth/register',
    component: RegisterComponent,
    title: 'Create Account — NexaCommerce'
  },

  // Seller / Vendor Portal (Protected)
  {
    path: 'vendor/dashboard',
    component: VendorDashboardComponent,
    canActivate: [roleGuard],
    data: { roles: ['Vendor', 'Admin', 'SuperAdmin'] },
    title: 'Seller Studio — NexaCommerce'
  },

  // Platform Admin Console (Protected)
  {
    path: 'admin/dashboard',
    component: AdminDashboardComponent,
    canActivate: [roleGuard],
    data: { roles: ['Admin', 'SuperAdmin'] },
    title: 'Operations Console — NexaCommerce'
  },

  // Wildcard fallback
  {
    path: '**',
    redirectTo: ''
  }
];
