import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { TokenService } from '../services/token.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (tokenService.getToken() && !tokenService.isTokenExpired() && tokenService.isAdmin()) {
    return true;
  }

  // User is not admin, redirect to home
  router.navigate(['/']);
  return false;
};
