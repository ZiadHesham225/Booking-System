import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EventService, CategoryService, TicketTypeService } from '../../../core/services';
import { Event, Category, TicketType, CreateEventRequest, UpdateEventRequest, CreateEventTicketTypeRequest } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-event-form',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LoadingSpinner],
  templateUrl: './event-form.html',
  styleUrl: './event-form.scss',
})
export class EventForm implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private eventService = inject(EventService);
  private categoryService = inject(CategoryService);
  private ticketTypeService = inject(TicketTypeService);

  eventForm!: FormGroup;
  categories = signal<Category[]>([]);
  ticketTypes = signal<TicketType[]>([]);
  loading = signal(true);
  submitting = signal(false);
  error = signal<string | null>(null);
  selectedImage: File | null = null;
  
  isEditMode = false;
  eventId: number | null = null;

  ngOnInit(): void {
    this.initForm();
    this.loadCategories();
    this.loadTicketTypes();
    
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.eventId = +id;
      this.loadEvent(this.eventId);
    } else {
      this.loading.set(false);
    }
  }

  initForm(): void {
    this.eventForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.required]],
      startDateTime: ['', Validators.required],
      endDateTime: ['', Validators.required],
      city: ['', [Validators.required, Validators.maxLength(100)]],
      address: ['', [Validators.required, Validators.maxLength(200)]],
      categoryId: ['', Validators.required],
      eventTicketTypes: this.fb.array([])
    });
  }

  get eventTicketTypes(): FormArray {
    return this.eventForm.get('eventTicketTypes') as FormArray;
  }

  addEventTicketType(): void {
    const ticketType = this.fb.group({
      ticketTypeId: ['', Validators.required],
      price: [0, [Validators.required, Validators.min(0)]],
      totalSeats: [100, [Validators.required, Validators.min(1)]]
    });
    this.eventTicketTypes.push(ticketType);
  }

  removeEventTicketType(index: number): void {
    this.eventTicketTypes.removeAt(index);
  }

  onImageSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedImage = file;
    }
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

  loadTicketTypes(): void {
    this.ticketTypeService.getTicketTypes().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.ticketTypes.set(response.data);
        }
      }
    });
  }

  loadEvent(id: number): void {
    this.eventService.getEventById(id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.patchForm(response.data);
        } else {
          this.error.set(response.message || 'Failed to load event');
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'Failed to load event');
        this.loading.set(false);
      }
    });
  }

  patchForm(event: Event): void {
    const startDate = new Date(event.startDateTime);
    const endDate = new Date(event.endDateTime);
    const formattedStartDate = startDate.toISOString().slice(0, 16);
    const formattedEndDate = endDate.toISOString().slice(0, 16);
    
    this.eventForm.patchValue({
      title: event.title,
      description: event.description,
      startDateTime: formattedStartDate,
      endDateTime: formattedEndDate,
      city: event.city,
      address: event.address,
      categoryId: event.categoryId
    });

    this.eventTicketTypes.clear();
    event.eventTicketTypes?.forEach(tt => {
      const ticketType = this.fb.group({
        ticketTypeId: [tt.ticketTypeId, Validators.required],
        price: [tt.price, [Validators.required, Validators.min(0)]],
        totalSeats: [tt.totalSeats, [Validators.required, Validators.min(1)]]
      });
      this.eventTicketTypes.push(ticketType);
    });
  }

  onSubmit(): void {
    if (this.eventForm.invalid || this.submitting()) return;

    this.submitting.set(true);
    this.error.set(null);

    const formValue = this.eventForm.value;
    
    if (this.isEditMode && this.eventId) {
      const request: UpdateEventRequest = {
        title: formValue.title,
        description: formValue.description,
        startDateTime: formValue.startDateTime,
        endDateTime: formValue.endDateTime,
        city: formValue.city,
        address: formValue.address,
        categoryId: +formValue.categoryId,
        image: this.selectedImage || undefined
      };

      this.eventService.updateEvent(this.eventId, request).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.router.navigate(['/admin/events']);
          } else {
            this.error.set(response.message || 'Failed to update event');
            this.submitting.set(false);
          }
        },
        error: (err: any) => {
          this.error.set(err?.message || 'An error occurred');
          this.submitting.set(false);
        }
      });
    } else {
      if (!this.selectedImage) {
        this.error.set('Please select an event image');
        this.submitting.set(false);
        return;
      }

      const request: CreateEventRequest = {
        title: formValue.title,
        description: formValue.description,
        startDateTime: formValue.startDateTime,
        endDateTime: formValue.endDateTime,
        city: formValue.city,
        address: formValue.address,
        categoryId: +formValue.categoryId,
        image: this.selectedImage,
        eventTicketTypes: formValue.eventTicketTypes.map((tt: any) => ({
          ticketTypeId: +tt.ticketTypeId,
          price: tt.price,
          totalSeats: tt.totalSeats
        }))
      };

      this.eventService.createEvent(request).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.router.navigate(['/admin/events']);
          } else {
            this.error.set(response.message || 'Failed to create event');
            this.submitting.set(false);
          }
        },
        error: (err: any) => {
          this.error.set(err?.message || 'An error occurred');
          this.submitting.set(false);
        }
      });
    }
  }
}
