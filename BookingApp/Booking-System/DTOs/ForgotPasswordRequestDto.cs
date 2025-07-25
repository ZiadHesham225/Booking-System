using System.ComponentModel.DataAnnotations;

namespace Booking_System.DTOs
{
    public class ForgotPasswordRequestDto
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}
