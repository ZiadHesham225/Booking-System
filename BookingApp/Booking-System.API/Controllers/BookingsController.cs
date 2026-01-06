using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Booking_System.Application.Interfaces;
using Booking_System.Application.Common;
using Booking_System.Application.DTOs.Booking;
using Booking_System.Application.DTOs.Coupon;

namespace Booking_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Get all bookings for the current user
        /// </summary>
        /// <returns>List of user bookings</returns>
        [HttpGet("user-bookings")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetUserBookings()
        {
            try
            {
                var userId = GetCurrentUserId();
                var bookings = await _bookingService.GetUserBookingsAsync(userId);
                return Ok(ApiResponse<IEnumerable<BookingDto>>.Success(bookings));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<BookingDto>>.Failure("An error occurred while retrieving bookings."));
            }
        }

        /// <summary>
        /// Get booking details by ID for the current user
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Booking details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BookingDto>>> GetBookingDetails(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.GetBookingDetailsByIdAsync(id, userId);
                return Ok(ApiResponse<BookingDto>.Success(booking));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<BookingDto>.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BookingDto>.Failure("An error occurred while retrieving booking details."));
            }
        }
        /// <summary>
        /// Create a new booking
        /// </summary>
        /// <param name="bookingDto">Booking creation data</param>
        /// <returns>Created booking</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BookingDto>>> CreateBooking([FromBody] CreateBookingDto bookingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<BookingDto>.Failure("Invalid data"));
            }

            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.CreateBookingAsync(bookingDto, userId);
                return Ok(ApiResponse<BookingDto>.Success(booking, "Booking created successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<BookingDto>.Failure(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<BookingDto>.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<BookingDto>.Failure("An error occurred while creating the booking."));
            }
        }

        /// <summary>
        /// Delete a booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteBooking(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _bookingService.DeleteBookingAsync(id, userId);
                return Ok(ApiResponse.Success("Booking deleted successfully"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("An error occurred while deleting the booking."));
            }
        }

        /// <summary>
        /// Check if user has already booked an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Boolean indicating if user has booked</returns>
        [HttpGet("check-booking/{eventId}")]
        public async Task<ActionResult<ApiResponse<object>>> HasUserBookedEventTicketType(int eventId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var hasBooked = await _bookingService.HasUserBookedEventAsync(userId, eventId);
                return Ok(ApiResponse<object>.Success(new { hasBooked }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred while checking booking status."));
            }
        }
        private string GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }
    }
}

