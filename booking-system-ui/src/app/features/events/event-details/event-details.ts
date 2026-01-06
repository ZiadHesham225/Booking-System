import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EventService } from '../../../core/services';
import { Event, EventTicketType } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-event-details',
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe, LoadingSpinner],
  templateUrl: './event-details.html',
  styleUrl: './event-details.scss',
})
export class EventDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private eventService = inject(EventService);

  event = signal<Event | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  selectedTickets = signal<Map<number, number>>(new Map());

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadEvent(+id);
    } else {
      this.router.navigate(['/events']);
    }
  }

  loadEvent(id: number): void {
    this.loading.set(true);
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
        this.error.set(err?.message || 'Failed to load event details');
        this.loading.set(false);
      }
    });
  }

  get imageUrl(): string {
    const e = this.event();
    if (e?.imageUrl) {
      return e.imageUrl.startsWith('http') 
        ? e.imageUrl 
        : `${environment.apiUrl.replace('/api', '')}/${e.imageUrl}`;
    }
    return 'assets/images/event-placeholder.jpg';
  }

  get isUpcoming(): boolean {
    const e = this.event();
    return e ? new Date(e.startDateTime) > new Date() : false;
  }

  get totalSelected(): number {
    let total = 0;
    this.selectedTickets().forEach(qty => total += qty);
    return total;
  }

  get totalPrice(): number {
    let total = 0;
    const e = this.event();
    if (e?.eventTicketTypes) {
      this.selectedTickets().forEach((qty, id) => {
        const tt = e.eventTicketTypes?.find(t => t.id === id);
        if (tt) total += tt.price * qty;
      });
    }
    return total;
  }

  updateTicketQuantity(ticketType: EventTicketType, change: number): void {
    const current = this.selectedTickets().get(ticketType.id) || 0;
    const newQty = Math.max(0, Math.min(current + change, ticketType.availableSeats));
    
    const updated = new Map(this.selectedTickets());
    if (newQty === 0) {
      updated.delete(ticketType.id);
    } else {
      updated.set(ticketType.id, newQty);
    }
    this.selectedTickets.set(updated);
  }

  getSelectedQuantity(ticketTypeId: number): number {
    return this.selectedTickets().get(ticketTypeId) || 0;
  }

  proceedToBooking(): void {
    if (this.totalSelected > 0 && this.event()) {
      const ticketSelection = Array.from(this.selectedTickets().entries())
        .map(([id, qty]) => ({ ticketTypeId: id, quantity: qty }));
      
      this.router.navigate(['/bookings', 'new'], {
        queryParams: {
          eventId: this.event()!.eventId,
          tickets: JSON.stringify(ticketSelection)
        }
      });
    }
  }
}
