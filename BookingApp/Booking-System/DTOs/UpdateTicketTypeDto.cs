using System.ComponentModel.DataAnnotations;

namespace Booking_System.DTOs
{
    public class UpdateTicketTypeDto
    {
        [Required]
        public int TicketTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public bool IsActive { get; set; }
    }
}
