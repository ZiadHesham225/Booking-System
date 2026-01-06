import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventService, CategoryService } from '../../../core/services';
import { Event, Category, EventSearchParams } from '../../../core/models';
import { EventCard } from '../event-card/event-card';
import { EventSearch } from '../event-search/event-search';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { Pagination } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-event-list',
  imports: [CommonModule, EventCard, EventSearch, LoadingSpinner, Pagination],
  templateUrl: './event-list.html',
  styleUrl: './event-list.scss',
})
export class EventList implements OnInit {
  private eventService = inject(EventService);
  private categoryService = inject(CategoryService);

  events = signal<Event[]>([]);
  categories = signal<Category[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  
  currentPage = signal(1);
  pageSize = signal(12);
  totalPages = signal(0);
  totalCount = signal(0);

  searchParams = signal<EventSearchParams>({
    pageIndex: 1,
    pageSize: 12
  });

  ngOnInit(): void {
    this.loadCategories();
    this.loadEvents();
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.categories.set(response.data);
        }
      }
    });
  }

  loadEvents(): void {
    this.loading.set(true);
    this.error.set(null);

    this.eventService.searchEvents(this.searchParams()).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.events.set(response.data.items);
          this.totalPages.set(response.data.totalPages);
          this.totalCount.set(response.data.totalItems);
          this.currentPage.set(response.data.currentPage);
        } else {
          this.error.set(response.message || 'Failed to load events');
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'An error occurred while loading events');
        this.loading.set(false);
      }
    });
  }

  onSearch(params: EventSearchParams): void {
    this.searchParams.set({
      ...params,
      pageIndex: 1,
      pageSize: this.pageSize()
    });
    this.loadEvents();
  }

  onPageChange(page: number): void {
    this.searchParams.update(p => ({ ...p, pageIndex: page }));
    this.currentPage.set(page);
    this.loadEvents();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
