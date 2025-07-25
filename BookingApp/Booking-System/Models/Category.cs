using System.ComponentModel.DataAnnotations;

namespace Booking_System.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
