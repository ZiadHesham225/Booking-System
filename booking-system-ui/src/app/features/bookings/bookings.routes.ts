import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards';

export const BOOKINGS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./booking-list/booking-list').then(m => m.BookingList),
    canActivate: [authGuard]
  },
  {
    path: 'new',
    loadComponent: () => import('./booking-form/booking-form').then(m => m.BookingForm),
    canActivate: [authGuard]
  },
  {
    path: 'confirmation/:id',
    loadComponent: () => import('./booking-confirmation/booking-confirmation').then(m => m.BookingConfirmation),
    canActivate: [authGuard]
  },
  {
    path: ':id',
    loadComponent: () => import('./booking-details/booking-details').then(m => m.BookingDetails),
    canActivate: [authGuard]
  }
];
