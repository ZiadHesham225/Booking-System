namespace Booking_System.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalBookings { get; set; }
        public int TodayBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalEvents { get; set; }
        public List<TopEventDto> TopEvents { get; set; }
    }
}
