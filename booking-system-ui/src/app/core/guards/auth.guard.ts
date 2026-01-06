import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { TokenService } from '../services/token.service';

export const authGuard: CanActivateFn = (route, state) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (tokenService.getToken() && !tokenService.isTokenExpired()) {
    return true;
  }

  // Store the attempted URL for redirecting after login
  router.navigate(['/auth/login'], {
    queryParams: { returnUrl: state.url }
  });
  
  return false;
};

export const guestGuard: CanActivateFn = (route, state) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (!tokenService.getToken() || tokenService.isTokenExpired()) {
    return true;
  }

  // User is already logged in, redirect to home
  router.navigate(['/']);
  return false;
};
