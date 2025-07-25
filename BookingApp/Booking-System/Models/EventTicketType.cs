using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking_System.Models
{
    public class EventTicketType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int EventId { get; set; }
        [Required]
        public int TicketTypeId { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        [Required]
        public int TotalSeats { get; set; }
        [Required]
        public int AvailableSeats { get; set; }

        // Navigation properties
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [ForeignKey("TicketTypeId")]
        public virtual TicketType TicketType { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
