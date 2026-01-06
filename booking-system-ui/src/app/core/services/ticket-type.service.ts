import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TicketType, CreateTicketTypeRequest, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class TicketTypeService {
  private readonly apiUrl = `${environment.apiUrl}/tickettype`;

  constructor(private http: HttpClient) {}

  getTicketTypes(): Observable<ApiResponse<TicketType[]>> {
    return this.http.get<ApiResponse<TicketType[]>>(this.apiUrl);
  }

  createTicketType(request: CreateTicketTypeRequest): Observable<ApiResponse<TicketType>> {
    return this.http.post<ApiResponse<TicketType>>(this.apiUrl, request);
  }

  updateTicketType(id: number, request: any): Observable<ApiResponse<TicketType>> {
    return this.http.put<ApiResponse<TicketType>>(`${this.apiUrl}/${id}`, request);
  }

  deleteTicketType(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
