namespace Booking_System.DTOs
{
    public class CalculateDiscountDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public decimal OrderValue { get; set; }
    }
}
