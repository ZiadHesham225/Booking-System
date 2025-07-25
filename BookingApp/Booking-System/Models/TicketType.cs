using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking_System.Models
{
    public class TicketType
    {
        [Key]
        public int TicketTypeId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } // e.g. "VIP", "Regular", "Student", "Early Bird"
        public bool IsActive { get; set; } = true;

        public virtual ICollection<EventTicketType> EventTicketTypes { get; set; } = new List<EventTicketType>();
    }
}
