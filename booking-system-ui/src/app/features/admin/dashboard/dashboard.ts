import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../../core/services';
import { AdminDashboard } from '../../../core/models';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink, CurrencyPipe, LoadingSpinner],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private adminService = inject(AdminService);

  stats = signal<AdminDashboard | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.loading.set(true);
    this.adminService.getDashboard().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.stats.set(response.data);
        } else {
          this.error.set(response.message || 'Failed to load dashboard');
        }
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err?.message || 'An error occurred');
        this.loading.set(false);
      }
    });
  }
}
