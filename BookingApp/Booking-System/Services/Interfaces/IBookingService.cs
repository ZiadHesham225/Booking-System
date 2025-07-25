using Booking_System.DTOs;

namespace Booking_System.Business_Logic.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId);
        Task<BookingDto> GetBookingDetailsByIdAsync(int bookingId, string userId);
        Task<BookingDto> GetBookingWithDetailsAsync(int bookingId);
        Task<BookingDto> CreateBookingAsync(CreateBookingDto bookingDto, string userId);
        Task DeleteBookingAsync(int bookingId, string userId);
        Task<bool> HasUserBookedEventAsync(string userId, int eventTicketTypeId);
        Task<BookingPriceCalculationDto> CalculateBookingPriceAsync(int eventId, int ticketTypeId, int numTickets, string? couponCode = null);
    }
}
