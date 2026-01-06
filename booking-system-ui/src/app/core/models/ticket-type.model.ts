export interface TicketType {
  ticketTypeId: number;
  name: string;
  isActive: boolean;
}

export interface CreateTicketTypeRequest {
  name: string;
  isActive: boolean;
}

export interface UpdateTicketTypeRequest {
  name: string;
  isActive: boolean;
}
