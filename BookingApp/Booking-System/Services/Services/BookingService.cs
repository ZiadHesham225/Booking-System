using Booking_System.Data;
using Booking_System.Data_Access.Interfaces;
using Booking_System.DTOs;
using Booking_System.Models;

namespace Booking_System.Business_Logic.Interfaces
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICouponService _couponService;

        public BookingService(IUnitOfWork unitOfWork, ICouponService couponService)
        {
            _unitOfWork = unitOfWork;
            _couponService = couponService;
        }

        public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId)
        {
            var bookings = await _unitOfWork.Bookings.GetUserBookingsAsync(userId);
            return bookings;
        }

        public async Task<BookingDto> GetBookingDetailsByIdAsync(int bookingId, string userId)
        {
            var booking = await _unitOfWork.Bookings.GetBookingDetailsByIdAsync(bookingId, userId);
            if (booking == null)
                throw new ArgumentException("Booking not found.");

            return booking;
        }

        public async Task<BookingDto> GetBookingWithDetailsAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                throw new ArgumentException("Booking not found.");

            return booking;
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto bookingDto, string userId)
        {
            var eventEntity = await _unitOfWork.Events.GetEventByIdAsync(bookingDto.EventId);
            if (eventEntity == null)
                throw new ArgumentException("Event not found.");

            var eventTicketType = await _unitOfWork.EventTicketTypes.GetByEventAndTicketTypeAsync(bookingDto.EventId, bookingDto.TicketTypeId);
            if (eventTicketType == null)
                throw new ArgumentException("Event ticket type not found.");

            if (bookingDto.NumTickets <= 0)
                throw new ArgumentException("Number of tickets must be greater than zero.");

            if (bookingDto.NumTickets > eventTicketType.AvailableSeats)
                throw new InvalidOperationException("Not enough seats available.");

            var hasAlreadyBooked = await _unitOfWork.Bookings.HasUserBookedEventAsync(userId, bookingDto.EventId);
            if (hasAlreadyBooked)
                throw new InvalidOperationException("You have already booked this event.");

            var basePrice = bookingDto.NumTickets * eventTicketType.Price;
            var finalPrice = basePrice;
            var discountAmount = 0m;
            int? couponId = null;

            if (!string.IsNullOrEmpty(bookingDto.CouponCode))
            {
                var couponValidation = await _couponService.ValidateCouponCodeAsync(bookingDto.CouponCode, userId, basePrice);
                if (!couponValidation.IsValid)
                    throw new ArgumentException(couponValidation.Message);

                discountAmount = couponValidation.DiscountAmount;
                finalPrice = basePrice - discountAmount;

                var coupon = await _unitOfWork.Coupons.GetByCodeAsync(bookingDto.CouponCode);
                couponId = coupon?.CouponId;
            }

            var booking = new Booking
            {
                EventId = bookingDto.EventId,
                EventTicketTypeId = eventTicketType.Id,
                UserId = userId,
                NumTickets = bookingDto.NumTickets,
                TotalPrice = finalPrice,
                BookingDate = DateTime.UtcNow,
                CouponId = couponId
            };

            eventTicketType.AvailableSeats -= bookingDto.NumTickets;
            _unitOfWork.EventTicketTypes.Update(eventTicketType);

            await _unitOfWork.Bookings.CreateBookingAsync(booking);

            if (!string.IsNullOrEmpty(bookingDto.CouponCode))
            {
                await _couponService.ApplyCouponAsync(bookingDto.CouponCode, userId);
            }

            await _unitOfWork.CommitAsync();

            var createdBooking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(booking.BookingId);
            return createdBooking;
        }
        public async Task DeleteBookingAsync(int bookingId, string userId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null)
                throw new ArgumentException("Booking not found.");

            var eventTicketType = await _unitOfWork.EventTicketTypes.GetByIdAsync(booking.EventTicketTypeId);
            if (eventTicketType != null)
            {
                eventTicketType.AvailableSeats += booking.NumTickets;
                _unitOfWork.EventTicketTypes.Update(eventTicketType);
            }

            await _unitOfWork.Bookings.DeleteBookingAsync(booking);
            await _unitOfWork.CommitAsync();
        }

        public async Task<bool> HasUserBookedEventAsync(string userId, int eventId)
        {
            return await _unitOfWork.Bookings.HasUserBookedEventAsync(userId, eventId);
        }

        public async Task<BookingPriceCalculationDto> CalculateBookingPriceAsync(int eventId, int ticketTypeId, int numTickets, string? couponCode = null)
        {
            var eventTicketType = await _unitOfWork.EventTicketTypes.GetByEventAndTicketTypeAsync(eventId, ticketTypeId);
            if (eventTicketType == null)
                throw new ArgumentException("Event ticket type not found.");

            var basePrice = numTickets * eventTicketType.Price;
            var discountAmount = 0m;
            var finalPrice = basePrice;

            if (!string.IsNullOrEmpty(couponCode))
            {
                discountAmount = await _couponService.CalculateDiscountAsync(couponCode, basePrice);
                finalPrice = basePrice - discountAmount;
            }

            return new BookingPriceCalculationDto
            {
                BasePrice = basePrice,
                DiscountAmount = discountAmount,
                FinalPrice = finalPrice,
                CouponCode = couponCode
            };
        }
    }
}
