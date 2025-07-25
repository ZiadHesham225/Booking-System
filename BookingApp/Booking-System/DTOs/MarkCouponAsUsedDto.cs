using System.ComponentModel.DataAnnotations;

namespace Booking_System.DTOs
{
    public class MarkCouponAsUsedDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public int CouponId { get; set; }
    }
}
