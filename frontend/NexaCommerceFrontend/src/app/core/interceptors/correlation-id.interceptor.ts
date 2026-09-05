import { HttpInterceptorFn } from '@angular/common/http';

function generateGuid(): string {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return crypto.randomUUID();
  }
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  // Check if header is already present
  if (!req.headers.has('X-Correlation-ID')) {
    const correlationId = generateGuid();
    const clonedReq = req.clone({
      setHeaders: {
        'X-Correlation-ID': correlationId
      }
    });
    return next(clonedReq);
  }

  return next(req);
};
