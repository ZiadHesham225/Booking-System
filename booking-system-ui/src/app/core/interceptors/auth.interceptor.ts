import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { TokenService } from '../services/token.service';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const tokenService = inject(TokenService);
  const authService = inject(AuthService);
  
  const token = tokenService.getToken();
  
  // Don't add token to auth endpoints (except refresh)
  const isAuthEndpoint = req.url.includes('/auth/') && !req.url.includes('/auth/refresh-token');
  
  if (token && !isAuthEndpoint) {
    req = addToken(req, token);
  }
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/auth/')) {
        // Token expired, try to refresh
        return authService.refreshToken().pipe(
          switchMap(response => {
            if (response.isSuccess && response.data) {
              const newReq = addToken(req, response.data.accessToken);
              return next(newReq);
            }
            authService.logout();
            return throwError(() => new Error('Token refresh failed'));
          }),
          catchError(refreshError => {
            authService.logout();
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};

function addToken(request: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}
