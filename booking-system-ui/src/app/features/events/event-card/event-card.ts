import { Component, Input } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Event } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-event-card',
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe],
  templateUrl: './event-card.html',
  styleUrl: './event-card.scss',
})
export class EventCard {
  @Input({ required: true }) event!: Event;

  get imageUrl(): string {
    if (this.event.imageUrl) {
      return this.event.imageUrl.startsWith('http') 
        ? this.event.imageUrl 
        : `${environment.apiUrl.replace('/api', '')}/${this.event.imageUrl}`;
    }
    return 'assets/images/event-placeholder.jpg';
  }

  get isUpcoming(): boolean {
    return new Date(this.event.startDateTime) > new Date();
  }

  get availableTickets(): number {
    return this.event.eventTicketTypes?.reduce((sum, tt) => sum + tt.availableSeats, 0) || 0;
  }

  get minPrice(): number {
    if (!this.event.eventTicketTypes?.length) return 0;
    return Math.min(...this.event.eventTicketTypes.map(tt => tt.price));
  }
}
