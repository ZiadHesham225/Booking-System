import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Coupon, CreateCouponRequest, ValidateCouponRequest, CouponValidationResponse, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CouponService {
  private readonly apiUrl = `${environment.apiUrl}/coupon`;

  constructor(private http: HttpClient) {}

  // Admin endpoints
  getAllCoupons(): Observable<ApiResponse<Coupon[]>> {
    return this.http.get<ApiResponse<Coupon[]>>(this.apiUrl);
  }

  getCoupons(): Observable<ApiResponse<Coupon[]>> {
    return this.getAllCoupons();
  }

  createCoupon(request: CreateCouponRequest): Observable<ApiResponse<Coupon>> {
    return this.http.post<ApiResponse<Coupon>>(this.apiUrl, request);
  }

  updateCoupon(id: number, request: any): Observable<ApiResponse<Coupon>> {
    return this.http.put<ApiResponse<Coupon>>(`${this.apiUrl}/${id}`, request);
  }

  deleteCoupon(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  toggleCouponStatus(id: number): Observable<ApiResponse<void>> {
    return this.http.patch<ApiResponse<void>>(`${this.apiUrl}/${id}/toggle-status`, {});
  }

  // User endpoints
  getMyCoupons(): Observable<ApiResponse<Coupon[]>> {
    return this.http.get<ApiResponse<Coupon[]>>(`${this.apiUrl}/my-coupons`);
  }

  validateCoupon(request: ValidateCouponRequest): Observable<ApiResponse<CouponValidationResponse>> {
    return this.http.post<ApiResponse<CouponValidationResponse>>(`${this.apiUrl}/validate`, request);
  }
}
