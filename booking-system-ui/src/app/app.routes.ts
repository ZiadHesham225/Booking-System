import { Routes } from '@angular/router';
import { authGuard, guestGuard, adminGuard } from './core/guards';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'events',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'events',
    loadChildren: () => import('./features/events/events.routes').then(m => m.EVENTS_ROUTES)
  },
  {
    path: 'bookings',
    canActivate: [authGuard],
    loadChildren: () => import('./features/bookings/bookings.routes').then(m => m.BOOKINGS_ROUTES)
  },
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES)
  },
  {
    path: '**',
    redirectTo: 'events'
  }
];
