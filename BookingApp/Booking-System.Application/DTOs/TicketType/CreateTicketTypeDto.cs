using System.ComponentModel.DataAnnotations;

namespace Booking_System.Application.DTOs.TicketType
{
    public class CreateTicketTypeDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
    }
}


