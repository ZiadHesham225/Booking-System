using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Booking_System.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public int CategoryId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }
        public string? ImageUrl { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual ICollection<EventTicketType> EventTicketTypes { get; set; } = new List<EventTicketType>();

        [NotMapped]
        public IEnumerable<TicketType> TicketTypes => EventTicketTypes?.Select(ett => ett.TicketType) ?? new List<TicketType>();
    }
}
