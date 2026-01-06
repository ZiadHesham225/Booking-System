import { Routes } from '@angular/router';
import { authGuard, adminGuard } from '../../core/guards';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./dashboard/dashboard').then(m => m.Dashboard),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'events',
    loadComponent: () => import('./event-management/event-management').then(m => m.EventManagement),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'events/new',
    loadComponent: () => import('./event-form/event-form').then(m => m.EventForm),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'events/:id',
    loadComponent: () => import('./event-form/event-form').then(m => m.EventForm),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'categories',
    loadComponent: () => import('./category-management/category-management').then(m => m.CategoryManagement),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'coupons',
    loadComponent: () => import('./coupon-management/coupon-management').then(m => m.CouponManagement),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'ticket-types',
    loadComponent: () => import('./ticket-type-management/ticket-type-management').then(m => m.TicketTypeManagement),
    canActivate: [authGuard, adminGuard]
  }
];
