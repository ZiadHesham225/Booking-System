using System.ComponentModel.DataAnnotations;

namespace Booking_System.DTOs
{
    public class CreateCouponDto
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [Range(0.01, 100.00)]
        public decimal DiscountPercent { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? MinOrderValue { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Range(1, int.MaxValue)]
        public int? UsageLimit { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
