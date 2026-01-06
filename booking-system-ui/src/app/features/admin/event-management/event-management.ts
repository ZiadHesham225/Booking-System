import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EventService } from '../../../core/services';
import { Event, EventSearchParams } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-event-management',
  imports: [CommonModule, RouterLink, FormsModule, DatePipe, LoadingSpinner, Pagination],
  templateUrl: './event-management.html',
  styleUrl: './event-management.scss',
})
export class EventManagement implements OnInit {
  private eventService = inject(EventService);

  events = signal<Event[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  
  currentPage = signal(1);
  pageSize = signal(10);
  totalPages = signal(0);
  totalCount = signal(0);
  
  searchTerm = '';
  deleteModal = signal<{ show: boolean; event: Event | null }>({ show: false, event: null });
  deleting = signal(false);

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.loading.set(true);
    const params: EventSearchParams = {
      pageIndex: this.currentPage(),
      pageSize: this.pageSize(),
      keyword: this.searchTerm || undefined
    };

    this.eventService.searchEvents(params).subscribe({
      next: (result) => {
        if (result.isSuccess && result.data) {
          this.events.set(result.data.items);
          this.totalPages.set(result.data.totalPages);
          this.totalCount.set(result.data.totalItems);
        } else {
          this.error.set(result.message || 'Failed to load events');
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'An error occurred');
        this.loading.set(false);
      }
    });
  }

  onSearch(): void {
    this.currentPage.set(1);
    this.loadEvents();
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadEvents();
  }

  openDeleteModal(event: Event): void {
    this.deleteModal.set({ show: true, event });
  }

  closeDeleteModal(): void {
    this.deleteModal.set({ show: false, event: null });
  }

  confirmDelete(): void {
    const event = this.deleteModal().event;
    if (!event || this.deleting()) return;

    this.deleting.set(true);
    this.eventService.deleteEvent(event.eventId).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.loadEvents();
          this.closeDeleteModal();
        } else {
          this.error.set(result.message || 'Failed to delete event');
        }
        this.deleting.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'An error occurred while deleting');
        this.deleting.set(false);
      }
    });
  }

  getImageUrl(event: Event): string {
    if (event.imageUrl) {
      return event.imageUrl.startsWith('http') 
        ? event.imageUrl 
        : `${environment.apiUrl.replace('/api', '')}${event.imageUrl}`;
    }
    return 'assets/images/event-placeholder.jpg';
  }

  isEventActive(event: Event): boolean {
    return new Date(event.startDateTime) > new Date();
  }
}
