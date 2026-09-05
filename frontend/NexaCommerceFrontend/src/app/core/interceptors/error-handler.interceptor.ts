import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { TokenStorageService } from '../services/token-storage.service';

export const errorHandlerInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const tokenStorage = inject(TokenStorageService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Clear tokens and redirect to login if session expires
        if (!req.url.includes('/login') && !req.url.includes('/refresh-token')) {
          tokenStorage.clear();
          router.navigate(['/auth/login'], {
            queryParams: { returnUrl: router.url }
          });
        }
      } else if (error.status === 403) {
        console.warn('Access denied to requested resource:', req.url);
      }

      return throwError(() => error);
    })
  );
};
