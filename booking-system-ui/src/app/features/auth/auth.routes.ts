import { Routes } from '@angular/router';
import { guestGuard } from '../../core/guards/auth.guard';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login').then(m => m.Login),
    canActivate: [guestGuard]
  },
  {
    path: 'register',
    loadComponent: () => import('./register/register').then(m => m.Register),
    canActivate: [guestGuard]
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./forgot-password/forgot-password').then(m => m.ForgotPassword),
    canActivate: [guestGuard]
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./reset-password/reset-password').then(m => m.ResetPassword),
    canActivate: [guestGuard]
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' }
];
