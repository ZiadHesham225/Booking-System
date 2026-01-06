import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TicketTypeService } from '../../../core/services';
import { TicketType, CreateTicketTypeRequest, UpdateTicketTypeRequest } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-ticket-type-management',
  imports: [CommonModule, ReactiveFormsModule, LoadingSpinner],
  templateUrl: './ticket-type-management.html',
  styleUrl: './ticket-type-management.scss',
})
export class TicketTypeManagement implements OnInit {
  private fb = inject(FormBuilder);
  private ticketTypeService = inject(TicketTypeService);

  ticketTypes = signal<TicketType[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  
  ticketTypeForm!: FormGroup;
  showModal = signal(false);
  editingTicketType = signal<TicketType | null>(null);
  submitting = signal(false);
  
  deleteModal = signal<{ show: boolean; ticketType: TicketType | null }>({ show: false, ticketType: null });
  deleting = signal(false);

  ngOnInit(): void {
    this.initForm();
    this.loadTicketTypes();
  }

  initForm(): void {
    this.ticketTypeForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      isActive: [true]
    });
  }

  loadTicketTypes(): void {
    this.loading.set(true);
    this.ticketTypeService.getTicketTypes().subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.ticketTypes.set(result.data || []);
        } else {
          this.error.set(result.message || 'Failed to load ticket types');
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'An error occurred');
        this.loading.set(false);
      }
    });
  }

  openCreateModal(): void {
    this.editingTicketType.set(null);
    this.ticketTypeForm.reset({
      name: '',
      isActive: true
    });
    this.showModal.set(true);
  }

  openEditModal(ticketType: TicketType): void {
    this.editingTicketType.set(ticketType);
    this.ticketTypeForm.patchValue({
      name: ticketType.name,
      isActive: ticketType.isActive
    });
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingTicketType.set(null);
  }

  onSubmit(): void {
    if (this.ticketTypeForm.invalid || this.submitting()) return;

    this.submitting.set(true);
    const formValue = this.ticketTypeForm.value;
    const editing = this.editingTicketType();

    if (editing) {
      const request: UpdateTicketTypeRequest = {
        name: formValue.name,
        isActive: formValue.isActive
      };

      this.ticketTypeService.updateTicketType(editing.ticketTypeId, request).subscribe({
        next: (result) => {
          if (result.isSuccess) {
            this.loadTicketTypes();
            this.closeModal();
          } else {
            this.error.set(result.message || 'Failed to update ticket type');
          }
          this.submitting.set(false);
        },
        error: (err: any) => {
          this.error.set(err?.message || 'An error occurred');
          this.submitting.set(false);
        }
      });
    } else {
      const request: CreateTicketTypeRequest = {
        name: formValue.name,
        isActive: formValue.isActive
      };

      this.ticketTypeService.createTicketType(request).subscribe({
        next: (result) => {
          if (result.isSuccess) {
            this.loadTicketTypes();
            this.closeModal();
          } else {
            this.error.set(result.message || 'Failed to create ticket type');
          }
          this.submitting.set(false);
        },
        error: (err: any) => {
          this.error.set(err?.message || 'An error occurred');
          this.submitting.set(false);
        }
      });
    }
  }

  openDeleteModal(ticketType: TicketType): void {
    this.deleteModal.set({ show: true, ticketType });
  }

  closeDeleteModal(): void {
    this.deleteModal.set({ show: false, ticketType: null });
  }

  confirmDelete(): void {
    const ticketType = this.deleteModal().ticketType;
    if (!ticketType || this.deleting()) return;

    this.deleting.set(true);
    this.ticketTypeService.deleteTicketType(ticketType.ticketTypeId).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.loadTicketTypes();
          this.closeDeleteModal();
        } else {
          this.error.set(result.message || 'Failed to delete ticket type');
        }
        this.deleting.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'An error occurred');
        this.deleting.set(false);
      }
    });
  }
}
