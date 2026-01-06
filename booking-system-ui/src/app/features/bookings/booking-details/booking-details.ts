import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookingService } from '../../../core/services';
import { Booking } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-booking-details',
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe, LoadingSpinner],
  templateUrl: './booking-details.html',
  styleUrl: './booking-details.scss',
})
export class BookingDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private bookingService = inject(BookingService);

  booking = signal<Booking | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  cancelling = signal(false);
  showCancelModal = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadBooking(+id);
    } else {
      this.router.navigate(['/bookings']);
    }
  }

  loadBooking(id: number): void {
    this.loading.set(true);
    this.bookingService.getBookingById(id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.booking.set(response.data);
        } else {
          this.error.set(response.message || 'Booking not found');
        }
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load booking details');
        this.loading.set(false);
      }
    });
  }

  openCancelModal(): void {
    this.showCancelModal.set(true);
  }

  closeCancelModal(): void {
    this.showCancelModal.set(false);
  }

  confirmCancel(): void {
    if (!this.booking() || this.cancelling()) return;

    this.cancelling.set(true);
    this.bookingService.deleteBooking(this.booking()!.bookingId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.router.navigate(['/bookings']);
        } else {
          this.error.set(response.message || 'Failed to cancel booking');
          this.cancelling.set(false);
        }
      },
      error: () => {
        this.error.set('An error occurred while cancelling');
        this.cancelling.set(false);
      }
    });
  }
}
