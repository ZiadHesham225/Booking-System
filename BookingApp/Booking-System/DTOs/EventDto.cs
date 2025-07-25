namespace Booking_System.DTOs
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ImageUrl { get; set; }
        public bool isBooked { get; set; } = false;
        public IEnumerable<EventTicketTypeDto> EventTicketTypes { get; set; } = new List<EventTicketTypeDto>();
    }
}
