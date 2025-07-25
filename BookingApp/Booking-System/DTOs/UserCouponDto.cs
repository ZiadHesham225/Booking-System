namespace Booking_System.DTOs
{
    public class UserCouponDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CouponId { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedDate { get; set; }
        public string CouponCode { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal? MinOrderValue { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}
