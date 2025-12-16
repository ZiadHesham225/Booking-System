using Booking_System.Domain.Entities;
using Booking_System.Infrastructure.Data;
using Booking_System.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booking_System.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Unit tests for BookingRepository using EF Core InMemory database.
    /// Tests booking-specific operations including user bookings, duplicate detection, and aggregation.
    /// </summary>
    public class BookingRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly BookingRepository _repository;

        public BookingRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new BookingRepository(_context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Create test user
            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            _context.Users.Add(user);

            // Create category
            var category = new Category
            {
                CategoryId = 1,
                Name = "Concerts"
            };
            _context.Categories.Add(category);

            // Create ticket type
            var ticketType = new TicketType
            {
                TicketTypeId = 1,
                Name = "General Admission",
                IsActive = true
            };
            _context.TicketTypes.Add(ticketType);

            // Create event
            var testEvent = new Event
            {
                EventId = 1,
                Title = "Test Concert",
                Description = "Test concert description",
                City = "New York",
                Address = "Madison Square Garden",
                StartDateTime = DateTime.UtcNow.AddDays(30),
                EndDateTime = DateTime.UtcNow.AddDays(30).AddHours(3),
                CategoryId = 1
            };
            _context.Events.Add(testEvent);

            // Create EventTicketType
            var eventTicketType = new EventTicketType
            {
                Id = 1,
                EventId = 1,
                TicketTypeId = 1,
                Price = 50.00m,
                TotalSeats = 100,
                AvailableSeats = 95
            };
            _context.EventTicketTypes.Add(eventTicketType);

            // Create coupon
            var coupon = new Coupon
            {
                CouponId = 1,
                Code = "DISCOUNT10",
                DiscountPercent = 10,
                IsActive = true,
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };
            _context.Coupons.Add(coupon);

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetUserBookingsAsync Tests

        /// <summary>
        /// Tests that GetUserBookingsAsync returns all bookings for a specific user.
        /// </summary>
        [Fact]
        public async Task GetUserBookingsAsync_WithExistingBookings_ShouldReturnUserBookings()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 2,
                TotalPrice = 100.00m,
                BookingDate = DateTime.UtcNow
            };
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserBookingsAsync("user-1");

            // Assert
            result.Should().HaveCount(1);
            var bookingDto = result.First();
            bookingDto.NumTickets.Should().Be(2);
            bookingDto.TotalPrice.Should().Be(100.00m);
            bookingDto.EventName.Should().Be("Test Concert");
        }

        /// <summary>
        /// Tests that GetUserBookingsAsync returns empty when user has no bookings.
        /// </summary>
        [Fact]
        public async Task GetUserBookingsAsync_WithNoBookings_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetUserBookingsAsync("user-1");

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that GetUserBookingsAsync only returns bookings for the specified user.
        /// </summary>
        [Fact]
        public async Task GetUserBookingsAsync_ShouldOnlyReturnSpecifiedUserBookings()
        {
            // Arrange
            var user2 = new ApplicationUser
            {
                Id = "user-2",
                UserName = "testuser2",
                Email = "test2@example.com",
                FirstName = "Test2",
                LastName = "User"
            };
            await _context.Users.AddAsync(user2);

            var booking1 = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 2,
                TotalPrice = 100.00m,
                BookingDate = DateTime.UtcNow
            };
            var booking2 = new Booking
            {
                UserId = "user-2",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 1,
                TotalPrice = 50.00m,
                BookingDate = DateTime.UtcNow
            };
            await _context.Bookings.AddRangeAsync(booking1, booking2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserBookingsAsync("user-1");

            // Assert
            result.Should().HaveCount(1);
            result.First().NumTickets.Should().Be(2);
        }

        /// <summary>
        /// Tests that GetUserBookingsAsync returns bookings ordered by date descending.
        /// </summary>
        [Fact]
        public async Task GetUserBookingsAsync_ShouldReturnBookingsOrderedByDateDescending()
        {
            // Arrange
            var booking1 = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 1,
                TotalPrice = 50.00m,
                BookingDate = DateTime.UtcNow.AddDays(-2)
            };
            var booking2 = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 2,
                TotalPrice = 100.00m,
                BookingDate = DateTime.UtcNow
            };
            await _context.Bookings.AddRangeAsync(booking1, booking2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserBookingsAsync("user-1");

            // Assert
            result.Should().HaveCount(2);
            result.First().NumTickets.Should().Be(2); // Most recent booking first
        }

        #endregion

        #region GetBookingDetailsByIdAsync Tests

        /// <summary>
        /// Tests that GetBookingDetailsByIdAsync returns booking details for valid booking and user.
        /// </summary>
        [Fact]
        public async Task GetBookingDetailsByIdAsync_WithValidBookingAndUser_ShouldReturnBookingDetails()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 2,
                TotalPrice = 100.00m,
                BookingDate = DateTime.UtcNow
            };
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBookingDetailsByIdAsync(booking.BookingId, "user-1");

            // Assert
            result.Should().NotBeNull();
            result!.BookingId.Should().Be(booking.BookingId);
            result.NumTickets.Should().Be(2);
            result.EventName.Should().Be("Test Concert");
            result.TicketTypeName.Should().Be("General Admission");
        }

        /// <summary>
        /// Tests that GetBookingDetailsByIdAsync returns null for non-existing booking.
        /// </summary>
        [Fact]
        public async Task GetBookingDetailsByIdAsync_WithNonExistingBooking_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetBookingDetailsByIdAsync(999, "user-1");

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that GetBookingDetailsByIdAsync returns null when booking belongs to different user.
        /// </summary>
        [Fact]
        public async Task GetBookingDetailsByIdAsync_WithDifferentUser_ShouldReturnNull()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 2,
                TotalPrice = 100.00m,
                BookingDate = DateTime.UtcNow
            };
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBookingDetailsByIdAsync(booking.BookingId, "user-2");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetBookingWithDetailsAsync Tests

        /// <summary>
        /// Tests that GetBookingWithDetailsAsync returns full booking details.
        /// </summary>
        [Fact]
        public async Task GetBookingWithDetailsAsync_WithValidBookingId_ShouldReturnFullDetails()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 2,
                TotalPrice = 100.00m,
                BookingDate = DateTime.UtcNow,
                CouponId = 1
            };
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBookingWithDetailsAsync(booking.BookingId);

            // Assert
            result.Should().NotBeNull();
            result!.CouponCode.Should().Be("DISCOUNT10");
            result.CouponDiscountPercent.Should().Be(10);
            result.UserName.Should().Be("testuser");
        }

        /// <summary>
        /// Tests that GetBookingWithDetailsAsync returns null for non-existing booking.
        /// </summary>
        [Fact]
        public async Task GetBookingWithDetailsAsync_WithNonExistingBooking_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetBookingWithDetailsAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region HasUserBookedEventAsync Tests

        /// <summary>
        /// Tests that HasUserBookedEventAsync returns true when user has booked the event.
        /// </summary>
        [Fact]
        public async Task HasUserBookedEventAsync_WhenUserHasBookedEvent_ShouldReturnTrue()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 1,
                TotalPrice = 50.00m,
                BookingDate = DateTime.UtcNow
            };
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasUserBookedEventAsync("user-1", 1);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that HasUserBookedEventAsync returns false when user hasn't booked the event.
        /// </summary>
        [Fact]
        public async Task HasUserBookedEventAsync_WhenUserHasNotBookedEvent_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.HasUserBookedEventAsync("user-1", 1);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests duplicate booking prevention by checking user-event combination.
        /// </summary>
        [Fact]
        public async Task HasUserBookedEventAsync_PreventsDuplicateBookings()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 1,
                TotalPrice = 50.00m,
                BookingDate = DateTime.UtcNow
            };
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Act - Check if user can book the same event again
            var canBookAgain = !await _repository.HasUserBookedEventAsync("user-1", 1);

            // Assert
            canBookAgain.Should().BeFalse(); // User should not be able to book same event twice
        }

        #endregion

        #region GetByEventIdAsync Tests

        /// <summary>
        /// Tests that GetByEventIdAsync returns all bookings for an event.
        /// </summary>
        [Fact]
        public async Task GetByEventIdAsync_WithBookings_ShouldReturnAllEventBookings()
        {
            // Arrange
            var user2 = new ApplicationUser
            {
                Id = "user-2",
                UserName = "testuser2",
                Email = "test2@example.com",
                FirstName = "Test2",
                LastName = "User"
            };
            await _context.Users.AddAsync(user2);

            var bookings = new List<Booking>
            {
                new Booking { UserId = "user-1", EventId = 1, EventTicketTypeId = 1, NumTickets = 2, TotalPrice = 100.00m, BookingDate = DateTime.UtcNow },
                new Booking { UserId = "user-2", EventId = 1, EventTicketTypeId = 1, NumTickets = 3, TotalPrice = 150.00m, BookingDate = DateTime.UtcNow }
            };
            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByEventIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Tests that GetByEventIdAsync returns empty when no bookings exist for event.
        /// </summary>
        [Fact]
        public async Task GetByEventIdAsync_WithNoBookings_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetByEventIdAsync(1);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region CountAsync Tests

        /// <summary>
        /// Tests that CountAsync returns correct count without predicate.
        /// </summary>
        [Fact]
        public async Task CountAsync_WithoutPredicate_ShouldReturnTotalCount()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking { UserId = "user-1", EventId = 1, EventTicketTypeId = 1, NumTickets = 1, TotalPrice = 50.00m, BookingDate = DateTime.UtcNow },
                new Booking { UserId = "user-1", EventId = 1, EventTicketTypeId = 1, NumTickets = 2, TotalPrice = 100.00m, BookingDate = DateTime.UtcNow },
                new Booking { UserId = "user-1", EventId = 1, EventTicketTypeId = 1, NumTickets = 3, TotalPrice = 150.00m, BookingDate = DateTime.UtcNow }
            };
            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CountAsync();

            // Assert
            result.Should().Be(3);
        }

        /// <summary>
        /// Tests that CountAsync returns correct count with predicate.
        /// </summary>
        [Fact]
        public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
        {
            // Arrange
            var user2 = new ApplicationUser
            {
                Id = "user-2",
                UserName = "testuser2",
                Email = "test2@example.com",
                FirstName = "Test2",
                LastName = "User"
            };
            await _context.Users.AddAsync(user2);

            var bookings = new List<Booking>
            {
                new Booking { UserId = "user-1", EventId = 1, EventTicketTypeId = 1, NumTickets = 1, TotalPrice = 50.00m, BookingDate = DateTime.UtcNow },
                new Booking { UserId = "user-1", EventId = 1, EventTicketTypeId = 1, NumTickets = 2, TotalPrice = 100.00m, BookingDate = DateTime.UtcNow },
                new Booking { UserId = "user-2", EventId = 1, EventTicketTypeId = 1, NumTickets = 1, TotalPrice = 50.00m, BookingDate = DateTime.UtcNow }
            };
            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CountAsync(b => b.UserId == "user-1");

            // Assert
            result.Should().Be(2);
        }

        #endregion

        #region SumAsync Tests

        /// <summary>
        /// Tests that SumAsync returns correct sum of total prices.
        /// </summary>
        [Fact]
        public async Task SumAsync_WithBookings_ShouldReturnCorrectSum()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking { UserId = "user-1", EventId = 1, EventTicketTypeId = 1, NumTickets = 1, TotalPrice = 50.00m, BookingDate = DateTime.UtcNow },
                new Booking { UserId = "user-1", EventId = 1, EventTicketTypeId = 1, NumTickets = 2, TotalPrice = 100.00m, BookingDate = DateTime.UtcNow },
                new Booking { UserId = "user-1", EventId = 1, EventTicketTypeId = 1, NumTickets = 3, TotalPrice = 150.00m, BookingDate = DateTime.UtcNow }
            };
            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SumAsync(b => b.TotalPrice);

            // Assert
            result.Should().Be(300.00m);
        }

        /// <summary>
        /// Tests that SumAsync returns zero when no bookings exist.
        /// </summary>
        [Fact]
        public async Task SumAsync_WithNoBookings_ShouldReturnZero()
        {
            // Act
            var result = await _repository.SumAsync(b => b.TotalPrice);

            // Assert
            result.Should().Be(0);
        }

        #endregion

        #region CreateBookingAsync Tests

        /// <summary>
        /// Tests that CreateBookingAsync successfully creates a booking.
        /// </summary>
        [Fact]
        public async Task CreateBookingAsync_WithValidBooking_ShouldCreateBooking()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 2,
                TotalPrice = 100.00m,
                BookingDate = DateTime.UtcNow
            };

            // Act
            await _repository.CreateBookingAsync(booking);
            await _context.SaveChangesAsync();

            // Assert
            var savedBooking = await _context.Bookings.FirstOrDefaultAsync();
            savedBooking.Should().NotBeNull();
            savedBooking!.NumTickets.Should().Be(2);
        }

        #endregion

        #region DeleteBookingAsync Tests

        /// <summary>
        /// Tests that DeleteBookingAsync removes a booking from the database.
        /// </summary>
        [Fact]
        public async Task DeleteBookingAsync_WithExistingBooking_ShouldRemoveBooking()
        {
            // Arrange
            var booking = new Booking
            {
                UserId = "user-1",
                EventId = 1,
                EventTicketTypeId = 1,
                NumTickets = 2,
                TotalPrice = 100.00m,
                BookingDate = DateTime.UtcNow
            };
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            var bookingId = booking.BookingId;

            // Act
            await _repository.DeleteBookingAsync(booking);
            await _context.SaveChangesAsync();

            // Assert
            var deletedBooking = await _context.Bookings.FindAsync(bookingId);
            deletedBooking.Should().BeNull();
        }

        #endregion
    }
}
