namespace Booking_System.DTOs
{
    public class EventTicketTypeDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int TicketTypeId { get; set; }
        public string? TicketTypeName { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalSeats { get; set; }
    }
}
