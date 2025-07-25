using Booking_System.Validations;

namespace Booking_System.DTOs
{
    public class UpdateEventDto
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int CategoryId { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        [AllowedFileExtensions(new[] { ".png", ".jpg", ".jpeg" })]
        [MaxFileSize(10 * 1024 * 1024)]
        public IFormFile? EventPicture { get; set; }
    }
}
