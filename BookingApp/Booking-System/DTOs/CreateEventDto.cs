using Booking_System.Validations;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Booking_System.DTOs
{
    public class CreateEventDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int CategoryId { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        [AllowedFileExtensions(new[] { ".png", ".jpg", ".jpeg" })]
        [MaxFileSize(10 * 1024 * 1024)]
        [Required]
        public IFormFile EventPicture { get; set; }
    }
}
