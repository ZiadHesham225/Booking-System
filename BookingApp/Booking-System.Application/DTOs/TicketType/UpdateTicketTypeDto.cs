using System.ComponentModel.DataAnnotations;

namespace Booking_System.Application.DTOs.TicketType
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


