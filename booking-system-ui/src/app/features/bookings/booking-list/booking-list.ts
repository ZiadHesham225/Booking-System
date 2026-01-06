import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BookingService } from '../../../core/services';
import { Booking } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-booking-list',
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe, LoadingSpinner],
  templateUrl: './booking-list.html',
  styleUrl: './booking-list.scss',
})
export class BookingList implements OnInit {
  private bookingService = inject(BookingService);

  bookings = signal<Booking[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  activeTab = signal<'all' | 'recent'>('all');

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    this.loading.set(true);
    this.error.set(null);

    this.bookingService.getUserBookings().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.bookings.set(response.data);
        } else {
          this.error.set(response.message || 'Failed to load bookings');
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'An error occurred while loading your bookings');
        this.loading.set(false);
      }
    });
  }

  get filteredBookings(): Booking[] {
    const now = new Date();
    const thirtyDaysAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
    
    return this.bookings().filter(booking => {
      const bookingDate = new Date(booking.bookingDate);
      
      switch (this.activeTab()) {
        case 'recent':
          return bookingDate >= thirtyDaysAgo;
        case 'all':
        default:
          return true;
      }
    });
  }

  setTab(tab: 'all' | 'recent'): void {
    this.activeTab.set(tab);
  }
}
