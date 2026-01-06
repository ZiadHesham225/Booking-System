export interface Event {
  eventId: number;
  title: string;
  description: string;
  startDateTime: string;
  endDateTime: string;
  city: string;
  address: string;
  imageUrl: string;
  categoryId: number;
  categoryName: string;
  isBooked: boolean;
  eventTicketTypes: EventTicketType[];
}

export interface EventTicketType {
  id: number;
  eventId: number;
  ticketTypeId: number;
  ticketTypeName: string;
  price: number;
  totalSeats: number;
  availableSeats: number;
}

export interface CreateEventRequest {
  title: string;
  description: string;
  startDateTime: string;
  endDateTime: string;
  city: string;
  address: string;
  categoryId: number;
  image?: File;
  eventTicketTypes: CreateEventTicketTypeRequest[];
}

export interface CreateEventTicketTypeRequest {
  ticketTypeId: number;
  price: number;
  totalSeats: number;
}

export interface UpdateEventRequest {
  title: string;
  description: string;
  startDateTime: string;
  endDateTime: string;
  city: string;
  address: string;
  categoryId: number;
  image?: File;
}

export interface EventSearchParams {
  keyword?: string;
  searchTerm?: string;
  title?: string;
  city?: string;
  categoryId?: number;
  startDate?: string;
  endDate?: string;
  minPrice?: number;
  maxPrice?: number;
  pageIndex?: number;
  pageSize?: number;
  pageNumber?: number;
  sortBy?: string;
  isDescending?: boolean;
}
