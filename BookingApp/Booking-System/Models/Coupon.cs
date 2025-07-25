using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking_System.Models
{
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercent { get; set; } // e.g. 15.00 => 15%

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinOrderValue { get; set; } // Minimum booking amount required to apply

        public DateTime? ExpiryDate { get; set; }

        public int? UsageLimit { get; set; } // Global usage limit (across all users)

        public int TimesUsed { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<UserCoupon> UserCoupons { get; set; }
    }

}
