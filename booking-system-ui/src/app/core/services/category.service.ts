import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Category, CreateCategoryRequest, UpdateCategoryRequest, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly apiUrl = `${environment.apiUrl}/categories`;

  constructor(private http: HttpClient) {}

  getCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(this.apiUrl);
  }

  createCategory(request: CreateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.apiUrl, request);
  }

  updateCategory(id: number, request: UpdateCategoryRequest): Observable<ApiResponse<void>> {
    return this.http.put<ApiResponse<void>>(this.apiUrl, { id, ...request });
  }

  deleteCategory(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
