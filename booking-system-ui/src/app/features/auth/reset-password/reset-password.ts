import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss',
})
export class ResetPassword implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  resetForm: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';
  token = '';
  email = '';

  constructor() {
    this.resetForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParams['token'] || '';
    this.email = this.route.snapshot.queryParams['email'] || '';
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
    }
    return null;
  }

  onSubmit(): void {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const request = {
      email: this.email,
      token: this.token,
      newPassword: this.resetForm.value.newPassword
    };

    this.authService.resetPassword(request).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.isSuccess) {
          this.successMessage = response.message || 'Password has been reset successfully.';
          setTimeout(() => this.router.navigate(['/auth/login']), 2000);
        } else {
          this.errorMessage = response.message || 'Failed to reset password. Please try again.';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'Failed to reset password. Please try again.';
      }
    });
  }

  get f() {
    return this.resetForm.controls;
  }
}
