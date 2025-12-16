using Booking_System.Application.DTOs.Booking;
using Booking_System.Domain.Entities;
using Booking_System.Application.Interfaces;
using System.Linq.Expressions;

namespace Booking_System.Application.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId);
        Task<BookingDto?> GetBookingDetailsByIdAsync(int bookingId, string userId);
        Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId);
        Task CreateBookingAsync(Booking booking);
        Task DeleteBookingAsync(Booking booking);
        Task<BookingDto?> GetBookingWithDetailsAsync(int bookingId);
        Task<bool> HasUserBookedEventAsync(string userId, int eventTicketTypeId);
        Task<int> CountAsync(Expression<Func<Booking, bool>>? predicate = null);
        Task<decimal> SumAsync(Expression<Func<Booking, decimal?>> selector);
    }
}



