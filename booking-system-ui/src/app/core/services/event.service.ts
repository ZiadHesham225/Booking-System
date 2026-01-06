import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Event, EventSearchParams, CreateEventRequest, UpdateEventRequest, PaginatedResponse, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private readonly apiUrl = `${environment.apiUrl}/events`;

  constructor(private http: HttpClient) {}

  getEvents(pageIndex: number = 1, pageSize: number = 12): Observable<ApiResponse<PaginatedResponse<Event>>> {
    const params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApiResponse<PaginatedResponse<Event>>>(this.apiUrl, { params });
  }

  getEventById(id: number): Observable<ApiResponse<Event>> {
    return this.http.get<ApiResponse<Event>>(`${this.apiUrl}/${id}`);
  }

  searchEvents(searchRequest: EventSearchParams): Observable<ApiResponse<PaginatedResponse<Event>>> {
    let params = new HttpParams()
      .set('pageIndex', (searchRequest.pageIndex ?? 1).toString())
      .set('pageSize', (searchRequest.pageSize ?? 12).toString());

    // Support multiple field names for keyword search
    const keyword = searchRequest.keyword || searchRequest.searchTerm || searchRequest.title;
    if (keyword) {
      params = params.set('keyword', keyword);
    }
    if (searchRequest.city) {
      params = params.set('city', searchRequest.city);
    }
    if (searchRequest.categoryId) {
      params = params.set('categoryId', searchRequest.categoryId.toString());
    }
    if (searchRequest.startDate) {
      params = params.set('startDate', new Date(searchRequest.startDate).toISOString());
    }
    if (searchRequest.endDate) {
      params = params.set('endDate', new Date(searchRequest.endDate).toISOString());
    }
    if (searchRequest.minPrice) {
      params = params.set('minPrice', searchRequest.minPrice.toString());
    }
    if (searchRequest.maxPrice) {
      params = params.set('maxPrice', searchRequest.maxPrice.toString());
    }
    if (searchRequest.sortBy) {
      params = params.set('sortBy', searchRequest.sortBy);
    }
    if (searchRequest.isDescending !== undefined) {
      params = params.set('isDescending', searchRequest.isDescending.toString());
    }

    return this.http.get<ApiResponse<PaginatedResponse<Event>>>(`${this.apiUrl}/search`, { params });
  }

  createEvent(request: CreateEventRequest): Observable<ApiResponse<Event>> {
    const formData = this.createFormData(request);
    return this.http.post<ApiResponse<Event>>(this.apiUrl, formData);
  }

  updateEvent(id: number, request: UpdateEventRequest): Observable<ApiResponse<Event>> {
    const formData = this.createFormData(request);
    return this.http.put<ApiResponse<Event>>(`${this.apiUrl}/${id}`, formData);
  }

  deleteEvent(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  private createFormData(request: CreateEventRequest | UpdateEventRequest): FormData {
    const formData = new FormData();
    
    formData.append('title', request.title);
    formData.append('description', request.description);
    formData.append('startDateTime', new Date(request.startDateTime).toISOString());
    formData.append('endDateTime', new Date(request.endDateTime).toISOString());
    formData.append('city', request.city);
    formData.append('address', request.address);
    formData.append('categoryId', request.categoryId.toString());
    
    if (request.image) {
      formData.append('EventPicture', request.image);
    }

    if ('eventTicketTypes' in request && request.eventTicketTypes) {
      request.eventTicketTypes.forEach((tt, index) => {
        formData.append(`ticketTypes[${index}].TicketTypeId`, tt.ticketTypeId.toString());
        formData.append(`ticketTypes[${index}].Price`, tt.price.toString());
        formData.append(`ticketTypes[${index}].TotalSeats`, tt.totalSeats.toString());
      });
    }

    return formData;
  }
}
