import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Booking, BookingDetails, CreateBookingRequest, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private readonly apiUrl = `${environment.apiUrl}/booking`;

  constructor(private http: HttpClient) {}

  getUserBookings(pageIndex: number = 1, pageSize: number = 10): Observable<ApiResponse<Booking[]>> {
    return this.http.get<ApiResponse<Booking[]>>(`${this.apiUrl}/user-bookings`, {
      params: {
        pageIndex: pageIndex.toString(),
        pageSize: pageSize.toString()
      }
    });
  }

  getBookingById(id: number): Observable<ApiResponse<BookingDetails>> {
    return this.http.get<ApiResponse<BookingDetails>>(`${this.apiUrl}/${id}`);
  }

  createBooking(request: CreateBookingRequest): Observable<ApiResponse<BookingDetails>> {
    return this.http.post<ApiResponse<BookingDetails>>(this.apiUrl, request);
  }

  deleteBooking(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  checkBooking(eventId: number): Observable<ApiResponse<{ hasBooked: boolean }>> {
    return this.http.get<ApiResponse<{ hasBooked: boolean }>>(`${this.apiUrl}/check-booking/${eventId}`);
  }
}
