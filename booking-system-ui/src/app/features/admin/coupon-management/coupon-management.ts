import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CouponService } from '../../../core/services';
import { Coupon, CreateCouponRequest, UpdateCouponRequest } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-coupon-management',
  imports: [CommonModule, ReactiveFormsModule, DatePipe, LoadingSpinner],
  templateUrl: './coupon-management.html',
  styleUrl: './coupon-management.scss',
})
export class CouponManagement implements OnInit {
  private fb = inject(FormBuilder);
  private couponService = inject(CouponService);

  coupons = signal<Coupon[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  
  couponForm!: FormGroup;
  showModal = signal(false);
  editingCoupon = signal<Coupon | null>(null);
  submitting = signal(false);
  
  deleteModal = signal<{ show: boolean; coupon: Coupon | null }>({ show: false, coupon: null });
  deleting = signal(false);

  ngOnInit(): void {
    this.initForm();
    this.loadCoupons();
  }

  initForm(): void {
    this.couponForm = this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(50)]],
      discountPercent: [10, [Validators.required, Validators.min(0.01), Validators.max(100)]],
      minOrderValue: [null],
      usageLimit: [100, [Validators.min(1)]],
      expiryDate: [''],
      isActive: [true]
    });
  }

  loadCoupons(): void {
    this.loading.set(true);
    this.couponService.getCoupons().subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.coupons.set(result.data || []);
        } else {
          this.error.set(result.message || 'Failed to load coupons');
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
    this.editingCoupon.set(null);
    this.couponForm.reset({
      code: '',
      discountPercent: 10,
      minOrderValue: null,
      usageLimit: 100,
      expiryDate: '',
      isActive: true
    });
    this.showModal.set(true);
  }

  openEditModal(coupon: Coupon): void {
    this.editingCoupon.set(coupon);
    const expDate = coupon.expiryDate ? new Date(coupon.expiryDate).toISOString().slice(0, 10) : '';
    this.couponForm.patchValue({
      code: coupon.code,
      discountPercent: coupon.discountPercent,
      minOrderValue: coupon.minOrderValue,
      usageLimit: coupon.usageLimit,
      expiryDate: expDate,
      isActive: coupon.isActive
    });
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingCoupon.set(null);
  }

  onSubmit(): void {
    if (this.couponForm.invalid || this.submitting()) return;

    this.submitting.set(true);
    const formValue = this.couponForm.value;
    const editing = this.editingCoupon();

    if (editing) {
      const request: UpdateCouponRequest = {
        code: formValue.code,
        discountPercent: formValue.discountPercent,
        minOrderValue: formValue.minOrderValue || undefined,
        usageLimit: formValue.usageLimit || undefined,
        expiryDate: formValue.expiryDate || undefined,
        isActive: formValue.isActive
      };

      this.couponService.updateCoupon(editing.couponId, request).subscribe({
        next: (result) => {
          if (result.isSuccess) {
            this.loadCoupons();
            this.closeModal();
          } else {
            this.error.set(result.message || 'Failed to update coupon');
          }
          this.submitting.set(false);
        },
        error: (err: any) => {
          this.error.set(err?.message || 'An error occurred');
          this.submitting.set(false);
        }
      });
    } else {
      const request: CreateCouponRequest = {
        code: formValue.code,
        discountPercent: formValue.discountPercent,
        minOrderValue: formValue.minOrderValue || undefined,
        usageLimit: formValue.usageLimit || undefined,
        expiryDate: formValue.expiryDate || undefined,
        isActive: formValue.isActive
      };

      this.couponService.createCoupon(request).subscribe({
        next: (result) => {
          if (result.isSuccess) {
            this.loadCoupons();
            this.closeModal();
          } else {
            this.error.set(result.message || 'Failed to create coupon');
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

  openDeleteModal(coupon: Coupon): void {
    this.deleteModal.set({ show: true, coupon });
  }

  closeDeleteModal(): void {
    this.deleteModal.set({ show: false, coupon: null });
  }

  confirmDelete(): void {
    const coupon = this.deleteModal().coupon;
    if (!coupon || this.deleting()) return;

    this.deleting.set(true);
    this.couponService.deleteCoupon(coupon.couponId).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.loadCoupons();
          this.closeDeleteModal();
        } else {
          this.error.set(result.message || 'Failed to delete coupon');
        }
        this.deleting.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'An error occurred');
        this.deleting.set(false);
      }
    });
  }

  isExpired(date?: string): boolean {
    if (!date) return false;
    return new Date(date) < new Date();
  }
}
