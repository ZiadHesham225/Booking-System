using System.ComponentModel.DataAnnotations;

namespace Booking_System.DTOs
{
    public class CreateTicketTypeDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
