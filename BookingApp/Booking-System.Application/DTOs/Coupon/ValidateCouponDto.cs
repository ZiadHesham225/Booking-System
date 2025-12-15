namespace Booking_System.Application.DTOs.Coupon
{
    public class ValidateCouponDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public decimal OrderValue { get; set; }
    }
}


