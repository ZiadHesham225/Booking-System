using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Booking_System.Business_Logic.Interfaces;
using Booking_System.DTOs;

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
        public async Task<IActionResult> GetUserBookings()
        {
            try
            {
                var userId = GetCurrentUserId();
                var bookings = await _bookingService.GetUserBookingsAsync(userId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving bookings.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get booking details by ID for the current user
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Booking details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.GetBookingDetailsByIdAsync(id, userId);
                return Ok(booking);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving booking details.", details = ex.Message });
            }
        }
        /// <summary>
        /// Create a new booking
        /// </summary>
        /// <param name="bookingDto">Booking creation data</param>
        /// <returns>Created booking</returns>
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto bookingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                var booking = await _bookingService.CreateBookingAsync(bookingDto, userId);
                return CreatedAtAction(nameof(GetBookingDetails), new { id = booking.BookingId }, booking);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the booking.", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _bookingService.DeleteBookingAsync(id, userId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the booking.", details = ex.Message });
            }
        }

        /// <summary>
        /// Check if user has already booked an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Boolean indicating if user has booked</returns>
        [HttpGet("check-booking/{eventId}")]
        public async Task<IActionResult> HasUserBookedEventTicketType(int eventId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var hasBooked = await _bookingService.HasUserBookedEventAsync(userId, eventId);
                return Ok(new { hasBooked });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking booking status.", details = ex.Message });
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