export interface AdminDashboard {
  totalEvents: number;
  totalBookings: number;
  totalUsers: number;
  totalRevenue: number;
  pendingBookings: number;
  recentBookings: RecentBooking[];
  upcomingEvents: UpcomingEvent[];
}

export interface RecentBooking {
  id: number;
  bookingId: number;
  userName: string;
  eventTitle: string;
  totalPrice: number;
  bookingDate: Date;
  status: string;
  event?: { title: string };
  user?: { email: string };
}

export interface UpcomingEvent {
  eventId: number;
  title: string;
  startDateTime: Date;
  totalBookings: number;
  availableSeats: number;
}
