namespace Booking_System.DTOs
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int EventTicketTypeId { get; set; }
        public int EventId { get; set; }
        public string? TicketTypeName { get; set; }
        public string? EventName { get; set; }
        public DateTime BookingDate { get; set; }
        public int NumTickets { get; set; }
        public decimal TotalPrice { get; set; }
        public int? CouponId { get; set; }
        public string? CouponCode { get; set; }
        public decimal? CouponDiscountPercent { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
    }
}
