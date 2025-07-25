namespace Booking_System.DTOs
{
    public class CreateBookingDto
    {
        public int EventId { get; set; }
        public int NumTickets { get; set; }
        public string? CouponCode { get; set; }
        public int TicketTypeId { get; set; }
    }
}
