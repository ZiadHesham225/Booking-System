import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryService } from '../../../core/services';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-category-management',
  imports: [CommonModule, ReactiveFormsModule, LoadingSpinner],
  templateUrl: './category-management.html',
  styleUrl: './category-management.scss',
})
export class CategoryManagement implements OnInit {
  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);

  categories = signal<Category[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  
  categoryForm!: FormGroup;
  showModal = signal(false);
  editingCategory = signal<Category | null>(null);
  submitting = signal(false);
  
  deleteModal = signal<{ show: boolean; category: Category | null }>({ show: false, category: null });
  deleting = signal(false);

  ngOnInit(): void {
    this.initForm();
    this.loadCategories();
  }

  initForm(): void {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]]
    });
  }

  loadCategories(): void {
    this.loading.set(true);
    this.categoryService.getCategories().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.categories.set(response.data);
        } else {
          this.error.set(response.message || 'Failed to load categories');
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
    this.editingCategory.set(null);
    this.categoryForm.reset();
    this.showModal.set(true);
  }

  openEditModal(category: Category): void {
    this.editingCategory.set(category);
    this.categoryForm.patchValue({
      name: category.name
    });
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingCategory.set(null);
    this.categoryForm.reset();
  }

  onSubmit(): void {
    if (this.categoryForm.invalid || this.submitting()) return;

    this.submitting.set(true);
    const formValue = this.categoryForm.value;
    const editing = this.editingCategory();

    if (editing) {
      const request: UpdateCategoryRequest = {
        name: formValue.name
      };

      this.categoryService.updateCategory(editing.id, request).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.loadCategories();
            this.closeModal();
          } else {
            this.error.set(response.message || 'Failed to update category');
          }
          this.submitting.set(false);
        },
        error: (err: any) => {
          this.error.set(err?.message || 'An error occurred');
          this.submitting.set(false);
        }
      });
    } else {
      const request: CreateCategoryRequest = {
        name: formValue.name
      };

      this.categoryService.createCategory(request).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.loadCategories();
            this.closeModal();
          } else {
            this.error.set(response.message || 'Failed to create category');
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

  openDeleteModal(category: Category): void {
    this.deleteModal.set({ show: true, category });
  }

  closeDeleteModal(): void {
    this.deleteModal.set({ show: false, category: null });
  }

  confirmDelete(): void {
    const category = this.deleteModal().category;
    if (!category || this.deleting()) return;

    this.deleting.set(true);
    this.categoryService.deleteCategory(category.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadCategories();
          this.closeDeleteModal();
        } else {
          this.error.set(response.message || 'Failed to delete category');
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
