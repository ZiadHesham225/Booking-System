using System.ComponentModel.DataAnnotations;

namespace Booking_System.Application.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}


