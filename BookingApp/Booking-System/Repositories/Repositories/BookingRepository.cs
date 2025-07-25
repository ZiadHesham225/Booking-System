using Booking_System.Data;
using Booking_System.Data_Access.Repositories;
using Booking_System.DTOs;
using Booking_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Booking_System.Data_Access.Interfaces
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
        .Include(b => b.Event)
        .Include(b => b.EventTicketType)
            .ThenInclude(ett => ett.TicketType)
        .Include(b => b.Coupon)
        .Where(b => b.UserId == userId)
        .OrderByDescending(b => b.BookingDate)
        .AsNoTracking()
        .Select(b => new BookingDto
        {
            BookingId = b.BookingId,
            BookingDate = b.BookingDate,
            NumTickets = b.NumTickets,
            TotalPrice = b.TotalPrice,
            UserId = b.User.Id,
            UserName = b.User.UserName,
            UserEmail = b.User.Email,
            CouponId = b.Coupon != null ? b.Coupon.CouponId : null,
            CouponCode = b.Coupon != null ? b.Coupon.Code : null,
            CouponDiscountPercent = b.Coupon != null ? b.Coupon.DiscountPercent : null,
            EventId = b.Event.EventId,
            EventName = b.Event.Title,
            EventTicketTypeId = b.EventTicketType.Id,
            TicketTypeName = b.EventTicketType.TicketType.Name
        })
        .ToListAsync();
        }

        public async Task<BookingDto> GetBookingDetailsByIdAsync(int bookingId, string userId)
        {
            return await _context.Bookings
        .Include(b => b.Event)
        .Include(b => b.EventTicketType)
            .ThenInclude(ett => ett.TicketType)
        .Include(b => b.Coupon)
        .AsNoTracking()
        .Select(b => new BookingDto
        {
            BookingId = b.BookingId,
            BookingDate = b.BookingDate,
            NumTickets = b.NumTickets,
            TotalPrice = b.TotalPrice,
            UserId = b.User.Id,
            UserName = b.User.UserName,
            UserEmail = b.User.Email,
            CouponId = b.Coupon != null ? b.Coupon.CouponId : null,
            CouponCode = b.Coupon != null ? b.Coupon.Code : null,
            CouponDiscountPercent = b.Coupon != null ? b.Coupon.DiscountPercent : null,
            EventId = b.Event.EventId,
            EventName = b.Event.Title,
            EventTicketTypeId = b.EventTicketType.Id,
            TicketTypeName = b.EventTicketType.TicketType.Name
        })
        .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);
        }
        public async Task<BookingDto> GetBookingWithDetailsAsync(int bookingId)
        {
            return await _context.Bookings
        .Include(b => b.Event)
        .Include(b => b.EventTicketType)
            .ThenInclude(ett => ett.TicketType)
        .Include(b => b.User)
        .Include(b => b.Coupon)
        .AsNoTracking()
        .Select(b => new BookingDto
        {
            BookingId = b.BookingId,
            BookingDate = b.BookingDate,
            NumTickets = b.NumTickets,
            TotalPrice = b.TotalPrice,
            UserId = b.User.Id,
            UserName = b.User.UserName,
            UserEmail = b.User.Email,
            CouponId = b.Coupon != null ? b.Coupon.CouponId : null,
            CouponCode = b.Coupon != null ? b.Coupon.Code : null,
            CouponDiscountPercent = b.Coupon != null ? b.Coupon.DiscountPercent : null,
            EventId = b.Event.EventId,
            EventName = b.Event.Title,
            EventTicketTypeId = b.EventTicketType.Id,
            TicketTypeName = b.EventTicketType.TicketType.Name
        })
        .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }
        public async Task CreateBookingAsync(Booking booking)
        {
            await CreateAsync(booking);
        }
        public async Task DeleteBookingAsync(Booking booking)
        {
            await DeleteAsync(booking.BookingId);
        }
        public async Task<bool> HasUserBookedEventAsync(string userId, int eventId)
        {
            return await _context.Bookings
                .AnyAsync(b => b.UserId == userId && b.EventId == eventId);
        }
        public async Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId)
        {
            return await _context.Bookings.Where(b => b.EventId == eventId).ToListAsync();
        }
        public Task<int> CountAsync(Expression<Func<Booking, bool>>? predicate = null)
        {
            return predicate == null
                ? _context.Bookings.CountAsync()
                : _context.Bookings.CountAsync(predicate);
        }

        public async Task<decimal> SumAsync(Expression<Func<Booking, decimal?>> selector)
        {
            return await _context.Bookings.SumAsync(selector) ?? 0;
        }
    }
}
