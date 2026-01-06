export interface Booking {
  bookingId: number;
  eventTicketTypeId: number;
  eventId: number;
  ticketTypeName: string;
  eventName: string;
  bookingDate: string;
  numTickets: number;
  totalPrice: number;
  couponId?: number;
  couponCode?: string;
  couponDiscountPercent?: number;
  userId?: string;
  userName?: string;
  userEmail?: string;
}

export interface CreateBookingRequest {
  eventId: number;
  ticketTypeId: number;
  numTickets: number;
  couponCode?: string;
}

export interface BookingDetails extends Booking {
  // Extended booking details if needed
}
