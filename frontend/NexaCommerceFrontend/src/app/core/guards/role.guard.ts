import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenStorageService } from '../services/token-storage.service';

export const roleGuard: CanActivateFn = (route) => {
  const tokenStorage = inject(TokenStorageService);
  const router = inject(Router);

  if (!tokenStorage.isAuthenticated()) {
    return router.createUrlTree(['/auth/login']);
  }

  const expectedRoles = (route.data?.['roles'] as string[]) || [];

  if (expectedRoles.length === 0) {
    return true;
  }

  const userRoles = tokenStorage.getRoles();
  const hasMatchingRole = expectedRoles.some((expected) =>
    userRoles.some((userRole) => userRole.toLowerCase() === expected.toLowerCase())
  );

  if (hasMatchingRole) {
    return true;
  }

  // If user lacks permission, redirect to unauthorized storefront or their own role home
  return router.createUrlTree(['/']);
};
