import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
})
export class ForgotPassword {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  forgotForm: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  constructor() {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.forgotPassword(this.forgotForm.value).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.isSuccess) {
          this.successMessage = response.message || 'Password reset link has been sent to your email.';
        } else {
          this.errorMessage = response.message || 'Failed to send reset link. Please try again.';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'Failed to send reset link. Please try again.';
      }
    });
  }

  get f() {
    return this.forgotForm.controls;
  }
}
