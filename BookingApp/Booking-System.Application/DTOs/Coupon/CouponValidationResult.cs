namespace Booking_System.Application.DTOs.Coupon
{
    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
    }
}
