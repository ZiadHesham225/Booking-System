using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking_System.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int EventTicketTypeId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int NumTickets { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        public int? CouponId { get; set; }

        // Navigation properties
        [ForeignKey("CouponId")]
        public virtual Coupon? Coupon { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [ForeignKey("EventTicketTypeId")]
        public virtual EventTicketType EventTicketType { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

    }

}
