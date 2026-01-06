import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookingService } from '../../../core/services';
import { Booking } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-booking-confirmation',
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe, LoadingSpinner],
  templateUrl: './booking-confirmation.html',
  styleUrl: './booking-confirmation.scss',
})
export class BookingConfirmation implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private bookingService = inject(BookingService);

  booking = signal<Booking | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadBooking(+id);
    } else {
      this.router.navigate(['/bookings']);
    }
  }

  loadBooking(id: number): void {
    this.bookingService.getBookingById(id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.booking.set(response.data);
        } else {
          this.error.set(response.message || 'Booking not found');
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'Failed to load booking');
        this.loading.set(false);
      }
    });
  }
}
