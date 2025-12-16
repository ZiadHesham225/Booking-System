using Booking_System.Application.DTOs.Booking;
using Booking_System.Application.DTOs.Coupon;
using Booking_System.Application.Interfaces;
using Booking_System.Application.Services;
using Booking_System.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Booking_System.Application.Tests.Services
{
    /// <summary>
    /// Unit tests for BookingService covering booking creation, retrieval, cancellation, and validation.
    /// </summary>
    public class BookingServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICouponService> _mockCouponService;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<IEventTicketTypeRepository> _mockEventTicketTypeRepository;
        private readonly Mock<ICouponRepository> _mockCouponRepository;
        private readonly BookingService _sut;

        public BookingServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCouponService = new Mock<ICouponService>();
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockEventRepository = new Mock<IEventRepository>();
            _mockEventTicketTypeRepository = new Mock<IEventTicketTypeRepository>();
            _mockCouponRepository = new Mock<ICouponRepository>();

            _mockUnitOfWork.Setup(x => x.Bookings).Returns(_mockBookingRepository.Object);
            _mockUnitOfWork.Setup(x => x.Events).Returns(_mockEventRepository.Object);
            _mockUnitOfWork.Setup(x => x.EventTicketTypes).Returns(_mockEventTicketTypeRepository.Object);
            _mockUnitOfWork.Setup(x => x.Coupons).Returns(_mockCouponRepository.Object);

            _sut = new BookingService(_mockUnitOfWork.Object, _mockCouponService.Object);
        }

        #region CreateBooking Tests

        /// <summary>
        /// Verifies that CreateBookingAsync successfully creates a booking with valid data.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_ValidData_ReturnsBookingDto()
        {
            // Arrange
            var userId = "user-123";
            var bookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = 2,
                CouponCode = null
            };

            var eventEntity = new Event { EventId = 1, Title = "Test Event" };
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                EventId = 1,
                TicketTypeId = 1,
                Price = 50m,
                TotalSeats = 100,
                AvailableSeats = 50
            };
            var expectedBooking = new BookingDto
            {
                BookingId = 1,
                EventId = 1,
                NumTickets = 2,
                TotalPrice = 100m
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(bookingDto.EventId))
                .ReturnsAsync(eventEntity);
            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(
                bookingDto.EventId, bookingDto.TicketTypeId))
                .ReturnsAsync(eventTicketType);
            _mockBookingRepository.Setup(x => x.HasUserBookedEventAsync(userId, bookingDto.EventId))
                .ReturnsAsync(false);
            _mockBookingRepository.Setup(x => x.CreateBookingAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);
            _mockBookingRepository.Setup(x => x.GetBookingWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(expectedBooking);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateBookingAsync(bookingDto, userId);

            // Assert
            result.Should().NotBeNull();
            _mockBookingRepository.Verify(x => x.CreateBookingAsync(It.IsAny<Booking>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that CreateBookingAsync throws exception when event is not found.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_EventNotFound_ThrowsArgumentException()
        {
            // Arrange
            var userId = "user-123";
            var bookingDto = new CreateBookingDto
            {
                EventId = 999,
                TicketTypeId = 1,
                NumTickets = 2
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(bookingDto.EventId))
                .ReturnsAsync((Event?)null);

            // Act
            Func<Task> act = async () => await _sut.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Event not found.");
        }

        /// <summary>
        /// Verifies that CreateBookingAsync throws exception when ticket type is not found for event.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_EventTicketTypeNotFound_ThrowsArgumentException()
        {
            // Arrange
            var userId = "user-123";
            var bookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 999,
                NumTickets = 2
            };

            var eventEntity = new Event { EventId = 1, Title = "Test Event" };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(bookingDto.EventId))
                .ReturnsAsync(eventEntity);
            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(
                bookingDto.EventId, bookingDto.TicketTypeId))
                .ReturnsAsync((EventTicketType?)null);

            // Act
            Func<Task> act = async () => await _sut.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Event ticket type not found.");
        }

        /// <summary>
        /// Verifies that CreateBookingAsync throws exception when number of tickets is zero or negative.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task CreateBookingAsync_InvalidNumTickets_ThrowsArgumentException(int numTickets)
        {
            // Arrange
            var userId = "user-123";
            var bookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = numTickets
            };

            var eventEntity = new Event { EventId = 1, Title = "Test Event" };
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                EventId = 1,
                TicketTypeId = 1,
                Price = 50m,
                AvailableSeats = 50
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(bookingDto.EventId))
                .ReturnsAsync(eventEntity);
            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(
                bookingDto.EventId, bookingDto.TicketTypeId))
                .ReturnsAsync(eventTicketType);

            // Act
            Func<Task> act = async () => await _sut.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Number of tickets must be greater than zero.");
        }

        /// <summary>
        /// Verifies that CreateBookingAsync throws exception when not enough seats are available.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_NotEnoughSeats_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "user-123";
            var bookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = 100 // Requesting more than available
            };

            var eventEntity = new Event { EventId = 1, Title = "Test Event" };
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                EventId = 1,
                TicketTypeId = 1,
                Price = 50m,
                AvailableSeats = 10 // Only 10 seats available
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(bookingDto.EventId))
                .ReturnsAsync(eventEntity);
            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(
                bookingDto.EventId, bookingDto.TicketTypeId))
                .ReturnsAsync(eventTicketType);

            // Act
            Func<Task> act = async () => await _sut.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Not enough seats available.");
        }

        /// <summary>
        /// Verifies that user cannot book the same event twice (duplicate booking prevention).
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_UserAlreadyBookedEvent_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "user-123";
            var bookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = 2
            };

            var eventEntity = new Event { EventId = 1, Title = "Test Event" };
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                EventId = 1,
                TicketTypeId = 1,
                Price = 50m,
                AvailableSeats = 50
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(bookingDto.EventId))
                .ReturnsAsync(eventEntity);
            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(
                bookingDto.EventId, bookingDto.TicketTypeId))
                .ReturnsAsync(eventTicketType);
            _mockBookingRepository.Setup(x => x.HasUserBookedEventAsync(userId, bookingDto.EventId))
                .ReturnsAsync(true); // User has already booked

            // Act
            Func<Task> act = async () => await _sut.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("You have already booked this event.");
        }

        /// <summary>
        /// Verifies that CreateBookingAsync applies coupon discount when valid coupon is provided.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_WithValidCoupon_AppliesDiscount()
        {
            // Arrange
            var userId = "user-123";
            var bookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = 2,
                CouponCode = "DISCOUNT20"
            };

            var eventEntity = new Event { EventId = 1, Title = "Test Event" };
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                EventId = 1,
                TicketTypeId = 1,
                Price = 50m,
                AvailableSeats = 50
            };
            var coupon = new Coupon { CouponId = 1, Code = "DISCOUNT20", DiscountPercent = 20 };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(bookingDto.EventId))
                .ReturnsAsync(eventEntity);
            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(
                bookingDto.EventId, bookingDto.TicketTypeId))
                .ReturnsAsync(eventTicketType);
            _mockBookingRepository.Setup(x => x.HasUserBookedEventAsync(userId, bookingDto.EventId))
                .ReturnsAsync(false);
            _mockCouponService.Setup(x => x.ValidateCouponCodeAsync(bookingDto.CouponCode, userId, 100m))
                .ReturnsAsync(new CouponValidationResult
                {
                    IsValid = true,
                    DiscountAmount = 20m,
                    DiscountPercent = 20
                });
            _mockCouponRepository.Setup(x => x.GetByCodeAsync(bookingDto.CouponCode))
                .ReturnsAsync(coupon);
            _mockCouponService.Setup(x => x.ApplyCouponAsync(bookingDto.CouponCode, userId))
                .Returns(Task.CompletedTask);

            _mockBookingRepository.Setup(x => x.CreateBookingAsync(It.Is<Booking>(b => b.TotalPrice == 80m)))
                .Returns(Task.CompletedTask);
            _mockBookingRepository.Setup(x => x.GetBookingWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(new BookingDto { TotalPrice = 80m });
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateBookingAsync(bookingDto, userId);

            // Assert
            _mockCouponService.Verify(x => x.ApplyCouponAsync(bookingDto.CouponCode, userId), Times.Once);
        }

        /// <summary>
        /// Verifies that CreateBookingAsync throws exception when coupon is invalid.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_WithInvalidCoupon_ThrowsArgumentException()
        {
            // Arrange
            var userId = "user-123";
            var bookingDto = new CreateBookingDto
            {
                EventId = 1,
                TicketTypeId = 1,
                NumTickets = 2,
                CouponCode = "INVALIDCODE"
            };

            var eventEntity = new Event { EventId = 1, Title = "Test Event" };
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                EventId = 1,
                TicketTypeId = 1,
                Price = 50m,
                AvailableSeats = 50
            };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(bookingDto.EventId))
                .ReturnsAsync(eventEntity);
            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(
                bookingDto.EventId, bookingDto.TicketTypeId))
                .ReturnsAsync(eventTicketType);
            _mockBookingRepository.Setup(x => x.HasUserBookedEventAsync(userId, bookingDto.EventId))
                .ReturnsAsync(false);
            _mockCouponService.Setup(x => x.ValidateCouponCodeAsync(bookingDto.CouponCode, userId, 100m))
                .ReturnsAsync(new CouponValidationResult
                {
                    IsValid = false,
                    Message = "Invalid coupon code"
                });

            // Act
            Func<Task> act = async () => await _sut.CreateBookingAsync(bookingDto, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid coupon code");
        }

        #endregion

        #region GetUserBookings Tests

        /// <summary>
        /// Verifies that GetUserBookingsAsync returns all bookings for a user.
        /// </summary>
        [Fact]
        public async Task GetUserBookingsAsync_ValidUserId_ReturnsUserBookings()
        {
            // Arrange
            var userId = "user-123";
            var expectedBookings = new List<BookingDto>
            {
                new BookingDto { BookingId = 1, EventName = "Event 1" },
                new BookingDto { BookingId = 2, EventName = "Event 2" }
            };

            _mockBookingRepository.Setup(x => x.GetUserBookingsAsync(userId))
                .ReturnsAsync(expectedBookings);

            // Act
            var result = await _sut.GetUserBookingsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Verifies that GetUserBookingsAsync returns empty collection when user has no bookings.
        /// </summary>
        [Fact]
        public async Task GetUserBookingsAsync_NoBookings_ReturnsEmptyCollection()
        {
            // Arrange
            var userId = "user-123";

            _mockBookingRepository.Setup(x => x.GetUserBookingsAsync(userId))
                .ReturnsAsync(new List<BookingDto>());

            // Act
            var result = await _sut.GetUserBookingsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetBookingDetails Tests

        /// <summary>
        /// Verifies that GetBookingDetailsByIdAsync returns booking details for valid booking ID.
        /// </summary>
        [Fact]
        public async Task GetBookingDetailsByIdAsync_ValidBookingId_ReturnsBookingDto()
        {
            // Arrange
            var bookingId = 1;
            var userId = "user-123";
            var expectedBooking = new BookingDto
            {
                BookingId = bookingId,
                EventName = "Test Event",
                NumTickets = 2
            };

            _mockBookingRepository.Setup(x => x.GetBookingDetailsByIdAsync(bookingId, userId))
                .ReturnsAsync(expectedBooking);

            // Act
            var result = await _sut.GetBookingDetailsByIdAsync(bookingId, userId);

            // Assert
            result.Should().NotBeNull();
            result.BookingId.Should().Be(bookingId);
        }

        /// <summary>
        /// Verifies that GetBookingDetailsByIdAsync throws exception when booking is not found.
        /// </summary>
        [Fact]
        public async Task GetBookingDetailsByIdAsync_BookingNotFound_ThrowsArgumentException()
        {
            // Arrange
            var bookingId = 999;
            var userId = "user-123";

            _mockBookingRepository.Setup(x => x.GetBookingDetailsByIdAsync(bookingId, userId))
                .ReturnsAsync((BookingDto?)null);

            // Act
            Func<Task> act = async () => await _sut.GetBookingDetailsByIdAsync(bookingId, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Booking not found.");
        }

        #endregion

        #region DeleteBooking Tests

        /// <summary>
        /// Verifies that DeleteBookingAsync successfully deletes a booking and restores available seats.
        /// </summary>
        [Fact]
        public async Task DeleteBookingAsync_ValidBooking_DeletesAndRestoresSeats()
        {
            // Arrange
            var bookingId = 1;
            var userId = "user-123";
            var booking = new Booking
            {
                BookingId = bookingId,
                EventTicketTypeId = 1,
                NumTickets = 2
            };
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                AvailableSeats = 48
            };

            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync(booking);
            _mockEventTicketTypeRepository.Setup(x => x.GetByIdAsync(booking.EventTicketTypeId))
                .ReturnsAsync(eventTicketType);
            _mockBookingRepository.Setup(x => x.DeleteBookingAsync(booking))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.DeleteBookingAsync(bookingId, userId);

            // Assert
            _mockBookingRepository.Verify(x => x.DeleteBookingAsync(booking), Times.Once);
            _mockEventTicketTypeRepository.Verify(x => x.Update(It.Is<EventTicketType>(e => e.AvailableSeats == 50)), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that DeleteBookingAsync throws exception when booking is not found.
        /// </summary>
        [Fact]
        public async Task DeleteBookingAsync_BookingNotFound_ThrowsArgumentException()
        {
            // Arrange
            var bookingId = 999;
            var userId = "user-123";

            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync((Booking?)null);

            // Act
            Func<Task> act = async () => await _sut.DeleteBookingAsync(bookingId, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Booking not found.");
        }

        #endregion

        #region HasUserBookedEvent Tests

        /// <summary>
        /// Verifies that HasUserBookedEventAsync returns true when user has booked the event.
        /// </summary>
        [Fact]
        public async Task HasUserBookedEventAsync_UserHasBooked_ReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var eventId = 1;

            _mockBookingRepository.Setup(x => x.HasUserBookedEventAsync(userId, eventId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.HasUserBookedEventAsync(userId, eventId);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that HasUserBookedEventAsync returns false when user has not booked the event.
        /// </summary>
        [Fact]
        public async Task HasUserBookedEventAsync_UserHasNotBooked_ReturnsFalse()
        {
            // Arrange
            var userId = "user-123";
            var eventId = 1;

            _mockBookingRepository.Setup(x => x.HasUserBookedEventAsync(userId, eventId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.HasUserBookedEventAsync(userId, eventId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region CalculateBookingPrice Tests

        /// <summary>
        /// Verifies that CalculateBookingPriceAsync returns correct price without coupon.
        /// </summary>
        [Fact]
        public async Task CalculateBookingPriceAsync_WithoutCoupon_ReturnsBasePrice()
        {
            // Arrange
            var eventId = 1;
            var ticketTypeId = 1;
            var numTickets = 3;
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                EventId = eventId,
                TicketTypeId = ticketTypeId,
                Price = 50m
            };

            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(eventId, ticketTypeId))
                .ReturnsAsync(eventTicketType);

            // Act
            var result = await _sut.CalculateBookingPriceAsync(eventId, ticketTypeId, numTickets);

            // Assert
            result.Should().NotBeNull();
            result.BasePrice.Should().Be(150m);
            result.DiscountAmount.Should().Be(0m);
            result.FinalPrice.Should().Be(150m);
        }

        /// <summary>
        /// Verifies that CalculateBookingPriceAsync returns discounted price with valid coupon.
        /// </summary>
        [Fact]
        public async Task CalculateBookingPriceAsync_WithValidCoupon_ReturnsDiscountedPrice()
        {
            // Arrange
            var eventId = 1;
            var ticketTypeId = 1;
            var numTickets = 2;
            var couponCode = "SAVE10";
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                EventId = eventId,
                TicketTypeId = ticketTypeId,
                Price = 50m
            };

            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(eventId, ticketTypeId))
                .ReturnsAsync(eventTicketType);
            _mockCouponService.Setup(x => x.CalculateDiscountAsync(couponCode, 100m))
                .ReturnsAsync(10m);

            // Act
            var result = await _sut.CalculateBookingPriceAsync(eventId, ticketTypeId, numTickets, couponCode);

            // Assert
            result.Should().NotBeNull();
            result.BasePrice.Should().Be(100m);
            result.DiscountAmount.Should().Be(10m);
            result.FinalPrice.Should().Be(90m);
            result.CouponCode.Should().Be(couponCode);
        }

        /// <summary>
        /// Verifies that CalculateBookingPriceAsync throws exception when event ticket type is not found.
        /// </summary>
        [Fact]
        public async Task CalculateBookingPriceAsync_EventTicketTypeNotFound_ThrowsArgumentException()
        {
            // Arrange
            var eventId = 999;
            var ticketTypeId = 1;
            var numTickets = 2;

            _mockEventTicketTypeRepository.Setup(x => x.GetByEventAndTicketTypeAsync(eventId, ticketTypeId))
                .ReturnsAsync((EventTicketType?)null);

            // Act
            Func<Task> act = async () => await _sut.CalculateBookingPriceAsync(eventId, ticketTypeId, numTickets);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Event ticket type not found.");
        }

        #endregion
    }
}
