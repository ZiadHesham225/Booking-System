import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Category, EventSearchParams } from '../../../core/models';

@Component({
  selector: 'app-event-search',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './event-search.html',
  styleUrl: './event-search.scss',
})
export class EventSearch implements OnInit {
  @Input() categories: Category[] = [];
  @Output() search = new EventEmitter<EventSearchParams>();

  private fb = new FormBuilder();
  searchForm!: FormGroup;
  showFilters = false;

  ngOnInit(): void {
    this.searchForm = this.fb.group({
      searchTerm: [''],
      categoryId: [''],
      startDate: [''],
      endDate: [''],
      minPrice: [''],
      maxPrice: [''],
      sortBy: ['date'],
      isDescending: [false]
    });
  }

  onSearch(): void {
    const formValue = this.searchForm.value;
    const params: EventSearchParams = {};

    if (formValue.searchTerm) params.searchTerm = formValue.searchTerm;
    if (formValue.categoryId) params.categoryId = +formValue.categoryId;
    if (formValue.startDate) params.startDate = formValue.startDate;
    if (formValue.endDate) params.endDate = formValue.endDate;
    if (formValue.minPrice) params.minPrice = +formValue.minPrice;
    if (formValue.maxPrice) params.maxPrice = +formValue.maxPrice;
    if (formValue.sortBy) params.sortBy = formValue.sortBy;
    params.isDescending = formValue.isDescending;

    this.search.emit(params);
  }

  clearFilters(): void {
    this.searchForm.reset({
      searchTerm: '',
      categoryId: '',
      startDate: '',
      endDate: '',
      minPrice: '',
      maxPrice: '',
      sortBy: 'date',
      isDescending: false
    });
    this.onSearch();
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }
}
