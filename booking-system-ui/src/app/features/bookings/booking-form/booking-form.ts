import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookingService, EventService, CouponService, AuthService } from '../../../core/services';
import { Event, CreateBookingRequest, Coupon } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { environment } from '../../../../environments/environment';

interface TicketSelection {
  ticketTypeId: number;
  quantity: number;
}

@Component({
  selector: 'app-booking-form',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, DatePipe, CurrencyPipe, LoadingSpinner],
  templateUrl: './booking-form.html',
  styleUrl: './booking-form.scss',
})
export class BookingForm implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private bookingService = inject(BookingService);
  private eventService = inject(EventService);
  private couponService = inject(CouponService);
  private authService = inject(AuthService);

  event = signal<Event | null>(null);
  ticketSelections = signal<TicketSelection[]>([]);
  loading = signal(true);
  submitting = signal(false);
  error = signal<string | null>(null);
  
  appliedCoupon = signal<Coupon | null>(null);
  couponError = signal<string | null>(null);
  couponLoading = signal(false);

  bookingForm!: FormGroup;

  ngOnInit(): void {
    this.bookingForm = this.fb.group({
      couponCode: ['']
    });

    const eventId = this.route.snapshot.queryParamMap.get('eventId');
    const ticketsParam = this.route.snapshot.queryParamMap.get('tickets');

    if (eventId && ticketsParam) {
      try {
        this.ticketSelections.set(JSON.parse(ticketsParam));
        this.loadEvent(+eventId);
      } catch {
        this.router.navigate(['/events']);
      }
    } else {
      this.router.navigate(['/events']);
    }
  }

  loadEvent(id: number): void {
    this.eventService.getEventById(id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.event.set(response.data);
        } else {
          this.error.set(response.message || 'Event not found');
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'Failed to load event');
        this.loading.set(false);
      }
    });
  }

  get selectedTickets() {
    const e = this.event();
    if (!e?.eventTicketTypes) return [];

    return this.ticketSelections().map(selection => {
      const ticketType = e.eventTicketTypes!.find(tt => tt.id === selection.ticketTypeId);
      return {
        ...selection,
        ticketType,
        subtotal: ticketType ? ticketType.price * selection.quantity : 0
      };
    }).filter(s => s.ticketType);
  }

  get subtotal(): number {
    return this.selectedTickets.reduce((sum, t) => sum + t.subtotal, 0);
  }

  get discount(): number {
    const coupon = this.appliedCoupon();
    if (!coupon) return 0;
    
    return this.subtotal * (coupon.discountPercent / 100);
  }

  get totalPrice(): number {
    return Math.max(0, this.subtotal - this.discount);
  }

  get totalQuantity(): number {
    return this.ticketSelections().reduce((sum, s) => sum + s.quantity, 0);
  }

  get imageUrl(): string {
    const e = this.event();
    if (e?.imageUrl) {
      return e.imageUrl.startsWith('http') 
        ? e.imageUrl 
        : `${environment.apiUrl.replace('/api', '')}${e.imageUrl}`;
    }
    return 'assets/images/event-placeholder.jpg';
  }

  applyCoupon(): void {
    const code = this.bookingForm.get('couponCode')?.value?.trim();
    if (!code) return;

    this.couponLoading.set(true);
    this.couponError.set(null);

    this.couponService.validateCoupon({ couponCode: code, orderValue: this.subtotal }).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          if (response.data.isValid) {
            this.appliedCoupon.set({
              couponId: 0,
              code: code,
              discountPercent: response.data.discountPercent,
              timesUsed: 0,
              isActive: true
            });
          } else {
            this.couponError.set(response.data.message || 'Invalid coupon code');
          }
        } else {
          this.couponError.set(response.message || 'Invalid coupon code');
        }
        this.couponLoading.set(false);
      },
      error: (err: any) => {
        this.couponError.set(err?.message || 'Failed to validate coupon');
        this.couponLoading.set(false);
      }
    });
  }

  removeCoupon(): void {
    this.appliedCoupon.set(null);
    this.bookingForm.get('couponCode')?.setValue('');
  }

  submitBooking(): void {
    if (this.submitting() || !this.event()) return;

    this.submitting.set(true);
    this.error.set(null);

    const selectedTicket = this.selectedTickets[0];
    const request: CreateBookingRequest = {
      eventId: this.event()!.eventId,
      ticketTypeId: selectedTicket.ticketType!.ticketTypeId,
      numTickets: this.totalQuantity,
      couponCode: this.appliedCoupon()?.code
    };

    this.bookingService.createBooking(request).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.router.navigate(['/bookings', 'confirmation', response.data.bookingId]);
        } else {
          this.error.set(response.message || 'Failed to create booking');
          this.submitting.set(false);
        }
      },
      error: (err: any) => {
        this.error.set(err?.message || 'An error occurred while creating your booking');
        this.submitting.set(false);
      }
    });
  }
}
