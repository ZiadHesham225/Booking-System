namespace Booking_System.Application.DTOs.Booking
{
    public class BookingPriceCalculationDto
    {
        public decimal BasePrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string? CouponCode { get; set; }
    }
}


