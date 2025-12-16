using Booking_System.Application.DTOs.Booking;
using Booking_System.Application.Interfaces;
using Booking_System.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Booking_System.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for BookingController covering booking-related endpoints.
    /// </summary>
    public class BookingControllerTests
    {
        private readonly Mock<IBookingService> _mockBookingService;
        private readonly BookingController _sut;

        public BookingControllerTests()
        {
            _mockBookingService = new Mock<IBookingService>();
            _sut = new BookingController(_mockBookingService.Object);
            SetupControllerWithUser("user-123");
        }

        #region GetUserBookings Tests

        /// <summary>
        /// Verifies that GetUserBookings returns Ok with bookings list.
        /// </summary>
        [Fact]
        public async Task GetUserBookings_ValidUser_ReturnsOkWithBookings()
        {
            // Arrange
            var userId = "user-123";
            var bookings = new List<BookingDto>
            {
                new BookingDto { BookingId = 1, EventName = "Concert", NumTickets = 2 },
                new BookingDto { BookingId = 2, EventName = "Sports Event", NumTickets = 4 }
            };

            _mockBookingService.Setup(x => x.GetUserBookingsAsync(userId))
                .ReturnsAsync(bookings);

            // Act
            var result = await _sut.GetUserBookings();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedBookings = okResult.Value as IEnumerable<BookingDto>;
            returnedBookings.Should().HaveCount(2);
        }

        /// <summary>
        /// Verifies that GetUserBookings returns Ok with empty list when user has no bookings.
        /// </summary>
        [Fact]
        public async Task GetUserBookings_NoBookings_ReturnsOkWithEmptyList()
        {
            // Arrange
            var userId = "user-123";

            _mockBookingService.Setup(x => x.GetUserBookingsAsync(userId))
                .ReturnsAsync(new List<BookingDto>());

            // Act
            var result = await _sut.GetUserBookings();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedBookings = okResult.Value as IEnumerable<BookingDto>;
            returnedBookings.Should().BeEmpty();
        }

        /// <summary>
        /// Verifies that GetUserBookings returns 500 when service throws exception.
        /// </summary>
        [Fact]
        public async Task GetUserBookings_ServiceError_ReturnsInternalServerError()
        {
            // Arrange
            var userId = "user-123";

            _mockBookingService.Setup(x => x.GetUserBookingsAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetUserBookings();

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        #endregion

        #region GetBookingDetails Tests

        /// <summary>
        /// Verifies that GetBookingDetails returns Ok with booking details.
        /// </summary>
        [Fact]
        public async Task GetBookingDetails_ValidBookingId_ReturnsOkWithBooking()
        {
            // Arrange
            var bookingId = 1;
            var userId = "user-123";
            var booking = new BookingDto
            {
                BookingId = bookingId,
                EventName = "Concert",
                NumTickets = 2,
                TotalPrice = 100m
            };

            _mockBookingService.Setup(x => x.GetBookingDetailsByIdAsync(bookingId, userId))
                .ReturnsAsync(booking);

            // Act
            var result = await _sut.GetBookingDetails(bookingId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedBooking = okResult.Value as BookingDto;
            returnedBooking.Should().NotBeNull();
            returnedBooking!.BookingId.Should().Be(bookingId);
        }

        /// <summary>
        /// Verifies that GetBookingDetails returns NotFound when booking doesn't exist.
        /// </summary>
        [Fact]
        public async Task GetBookingDetails_BookingNotFound_ReturnsNotFound()
        {
            // Arrange
            var bookingId = 999;
            var userId = "user-123";

            _mockBookingService.Setup(x => x.GetBookingDetailsByIdAsync(bookingId, userId))
                .ThrowsAsync(new ArgumentException("Booking not found."));

            // Act
            var result = await _sut.GetBookingDetails(bookingId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region CreateBooking Tests

        /// <summary>
        /// Verifies that CreateBooking returns CreatedAtAction when booking is created successfully.
        /// </summary>
        [Fact]
        public async Task CreateBooking_ValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var createBookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = 2,
                CouponCode = null
            };
            var userId = "user-123";
            var createdBooking = new BookingDto
            {
                BookingId = 1,
                EventId = 1,
                NumTickets = 2,
                TotalPrice = 100m
            };

            _mockBookingService.Setup(x => x.CreateBookingAsync(createBookingDto, userId))
                .ReturnsAsync(createdBooking);

            // Act
            var result = await _sut.CreateBooking(createBookingDto);

            // Assert
            var createdAtResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtResult.ActionName.Should().Be(nameof(BookingController.GetBookingDetails));
            createdAtResult.Value.Should().BeEquivalentTo(createdBooking);
        }

        /// <summary>
        /// Verifies that CreateBooking returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task CreateBooking_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var createBookingDto = new CreateBookingDto();
            _sut.ModelState.AddModelError("EventId", "EventId is required");

            // Act
            var result = await _sut.CreateBooking(createBookingDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Verifies that CreateBooking returns BadRequest when event is not found.
        /// </summary>
        [Fact]
        public async Task CreateBooking_EventNotFound_ReturnsBadRequest()
        {
            // Arrange
            var createBookingDto = new CreateBookingDto
            {
                EventId = 999,
                TicketTypeId = 1,
                NumTickets = 2
            };
            var userId = "user-123";

            _mockBookingService.Setup(x => x.CreateBookingAsync(createBookingDto, userId))
                .ThrowsAsync(new ArgumentException("Event not found."));

            // Act
            var result = await _sut.CreateBooking(createBookingDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Verifies that CreateBooking returns Conflict when user has already booked the event.
        /// </summary>
        [Fact]
        public async Task CreateBooking_DuplicateBooking_ReturnsConflict()
        {
            // Arrange
            var createBookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = 2
            };
            var userId = "user-123";

            _mockBookingService.Setup(x => x.CreateBookingAsync(createBookingDto, userId))
                .ThrowsAsync(new InvalidOperationException("You have already booked this event."));

            // Act
            var result = await _sut.CreateBooking(createBookingDto);

            // Assert
            var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().BeEquivalentTo(new { message = "You have already booked this event." });
        }

        /// <summary>
        /// Verifies that CreateBooking returns Conflict when not enough seats available.
        /// </summary>
        [Fact]
        public async Task CreateBooking_NotEnoughSeats_ReturnsConflict()
        {
            // Arrange
            var createBookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = 100
            };
            var userId = "user-123";

            _mockBookingService.Setup(x => x.CreateBookingAsync(createBookingDto, userId))
                .ThrowsAsync(new InvalidOperationException("Not enough seats available."));

            // Act
            var result = await _sut.CreateBooking(createBookingDto);

            // Assert
            result.Should().BeOfType<ConflictObjectResult>();
        }

        /// <summary>
        /// Verifies that CreateBooking returns BadRequest when coupon is invalid.
        /// </summary>
        [Fact]
        public async Task CreateBooking_InvalidCoupon_ReturnsBadRequest()
        {
            // Arrange
            var createBookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = 2,
                CouponCode = "INVALID"
            };
            var userId = "user-123";

            _mockBookingService.Setup(x => x.CreateBookingAsync(createBookingDto, userId))
                .ThrowsAsync(new ArgumentException("Invalid coupon code"));

            // Act
            var result = await _sut.CreateBooking(createBookingDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region DeleteBooking Tests

        /// <summary>
        /// Verifies that DeleteBooking returns NoContent when booking is deleted successfully.
        /// </summary>
        [Fact]
        public async Task DeleteBooking_ValidBookingId_ReturnsNoContent()
        {
            // Arrange
            var bookingId = 1;
            var userId = "user-123";

            _mockBookingService.Setup(x => x.DeleteBookingAsync(bookingId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteBooking(bookingId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        /// <summary>
        /// Verifies that DeleteBooking returns NotFound when booking doesn't exist.
        /// </summary>
        [Fact]
        public async Task DeleteBooking_BookingNotFound_ReturnsNotFound()
        {
            // Arrange
            var bookingId = 999;
            var userId = "user-123";

            _mockBookingService.Setup(x => x.DeleteBookingAsync(bookingId, userId))
                .ThrowsAsync(new ArgumentException("Booking not found."));

            // Act
            var result = await _sut.DeleteBooking(bookingId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region HasUserBookedEventTicketType Tests

        /// <summary>
        /// Verifies that HasUserBookedEventTicketType returns Ok with true when user has booked.
        /// </summary>
        [Fact]
        public async Task HasUserBookedEventTicketType_UserHasBooked_ReturnsOkWithTrue()
        {
            // Arrange
            var eventId = 1;
            var userId = "user-123";

            _mockBookingService.Setup(x => x.HasUserBookedEventAsync(userId, eventId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.HasUserBookedEventTicketType(eventId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { hasBooked = true });
        }

        /// <summary>
        /// Verifies that HasUserBookedEventTicketType returns Ok with false when user hasn't booked.
        /// </summary>
        [Fact]
        public async Task HasUserBookedEventTicketType_UserHasNotBooked_ReturnsOkWithFalse()
        {
            // Arrange
            var eventId = 1;
            var userId = "user-123";

            _mockBookingService.Setup(x => x.HasUserBookedEventAsync(userId, eventId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.HasUserBookedEventTicketType(eventId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { hasBooked = false });
        }

        #endregion

        #region Helper Methods

        private void SetupControllerWithUser(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        #endregion
    }
}
