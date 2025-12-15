namespace Booking_System.Application.DTOs.Coupon
{
    public class CalculateDiscountDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public decimal OrderValue { get; set; }
    }
}


