import { Routes } from '@angular/router';

export const EVENTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./event-list/event-list').then(m => m.EventList)
  },
  {
    path: ':id',
    loadComponent: () => import('./event-details/event-details').then(m => m.EventDetails)
  }
];
