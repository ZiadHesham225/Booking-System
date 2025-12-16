using Booking_System.Application.Common;
using Booking_System.Domain.Entities;
using Booking_System.Infrastructure.Data;
using Booking_System.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booking_System.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Unit tests for EventRepository using EF Core InMemory database.
    /// Tests event-specific operations including search, pagination, and ticket availability.
    /// </summary>
    public class EventRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly EventRepository _repository;

        public EventRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new EventRepository(_context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Create categories
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, Name = "Concerts" },
                new Category { CategoryId = 2, Name = "Sports" },
                new Category { CategoryId = 3, Name = "Theater" }
            };
            _context.Categories.AddRange(categories);

            // Create ticket types
            var ticketTypes = new List<TicketType>
            {
                new TicketType { TicketTypeId = 1, Name = "General Admission", IsActive = true },
                new TicketType { TicketTypeId = 2, Name = "VIP", IsActive = true }
            };
            _context.TicketTypes.AddRange(ticketTypes);

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

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region CreateEventAsync Tests

        /// <summary>
        /// Tests that CreateEventAsync successfully creates a new event.
        /// </summary>
        [Fact]
        public async Task CreateEventAsync_WithValidEvent_ShouldCreateEvent()
        {
            // Arrange
            var newEvent = new Event
            {
                Title = "New Concert",
                Description = "An amazing concert",
                City = "Los Angeles",
                Address = "Hollywood Bowl",
                StartDateTime = DateTime.UtcNow.AddDays(30),
                EndDateTime = DateTime.UtcNow.AddDays(30).AddHours(3),
                CategoryId = 1
            };

            // Act
            var result = await _repository.CreateEventAsync(newEvent);
            await _context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.EventId.Should().BeGreaterThan(0);
            _context.Events.Should().HaveCount(1);
        }

        #endregion

        #region GetEventByIdAsync Tests

        /// <summary>
        /// Tests that GetEventByIdAsync returns event with all related data.
        /// </summary>
        [Fact]
        public async Task GetEventByIdAsync_WithExistingEvent_ShouldReturnEventWithRelations()
        {
            // Arrange
            var testEvent = new Event
            {
                Title = "Test Concert",
                Description = "Test Description",
                City = "New York",
                Address = "MSG",
                StartDateTime = DateTime.UtcNow.AddDays(30),
                EndDateTime = DateTime.UtcNow.AddDays(30).AddHours(3),
                CategoryId = 1
            };
            await _context.Events.AddAsync(testEvent);
            await _context.SaveChangesAsync();

            var eventTicketType = new EventTicketType
            {
                EventId = testEvent.EventId,
                TicketTypeId = 1,
                Price = 100.00m,
                TotalSeats = 1000,
                AvailableSeats = 950
            };
            await _context.EventTicketTypes.AddAsync(eventTicketType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEventByIdAsync(testEvent.EventId);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Test Concert");
            result.Category.Should().NotBeNull();
            result.Category!.Name.Should().Be("Concerts");
            result.EventTicketTypes.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests that GetEventByIdAsync returns null for non-existing event.
        /// </summary>
        [Fact]
        public async Task GetEventByIdAsync_WithNonExistingEvent_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetEventByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAllEventsAsync Tests

        /// <summary>
        /// Tests that GetAllEventsAsync returns paginated events.
        /// </summary>
        [Fact]
        public async Task GetAllEventsAsync_WithMultipleEvents_ShouldReturnPaginatedResults()
        {
            // Arrange
            for (int i = 1; i <= 25; i++)
            {
                var testEvent = new Event
                {
                    Title = $"Event {i}",
                    Description = $"Description {i}",
                    City = "New York",
                    Address = "Venue",
                    StartDateTime = DateTime.UtcNow.AddDays(i),
                    EndDateTime = DateTime.UtcNow.AddDays(i).AddHours(3),
                    CategoryId = 1
                };
                await _context.Events.AddAsync(testEvent);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllEventsAsync(pageIndex: 1, pageSize: 10);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(10);
            result.TotalItems.Should().Be(25);
            result.TotalPages.Should().Be(3);
            result.HasNextPage.Should().BeTrue();
            result.HasPreviousPage.Should().BeFalse();
        }

        /// <summary>
        /// Tests pagination on second page.
        /// </summary>
        [Fact]
        public async Task GetAllEventsAsync_OnSecondPage_ShouldReturnCorrectItems()
        {
            // Arrange
            for (int i = 1; i <= 25; i++)
            {
                var testEvent = new Event
                {
                    Title = $"Event {i}",
                    Description = $"Description {i}",
                    City = "New York",
                    Address = "Venue",
                    StartDateTime = DateTime.UtcNow.AddDays(i),
                    EndDateTime = DateTime.UtcNow.AddDays(i).AddHours(3),
                    CategoryId = 1
                };
                await _context.Events.AddAsync(testEvent);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllEventsAsync(pageIndex: 2, pageSize: 10);

            // Assert
            result.Items.Should().HaveCount(10);
            result.CurrentPage.Should().Be(2);
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeTrue();
        }

        #endregion

        #region GetUpcomingEventsAsync Tests

        /// <summary>
        /// Tests that GetUpcomingEventsAsync only returns future events.
        /// </summary>
        [Fact]
        public async Task GetUpcomingEventsAsync_ShouldOnlyReturnFutureEvents()
        {
            // Arrange
            var pastEvent = new Event
            {
                Title = "Past Event",
                Description = "Past event description",
                City = "New York",
                Address = "Venue",
                StartDateTime = DateTime.UtcNow.AddDays(-10),
                EndDateTime = DateTime.UtcNow.AddDays(-10).AddHours(3),
                CategoryId = 1
            };
            var futureEvent = new Event
            {
                Title = "Future Event",
                Description = "Future event description",
                City = "New York",
                Address = "Venue",
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3),
                CategoryId = 1
            };
            await _context.Events.AddRangeAsync(pastEvent, futureEvent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUpcomingEventsAsync();

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("Future Event");
        }

        #endregion

        #region SearchEventsAsync Tests

        /// <summary>
        /// Tests search by keyword in title.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_ByKeyword_ShouldReturnMatchingEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Title = "Rock Concert", Description = "Rock music", City = "NYC", Address = "MSG", StartDateTime = DateTime.UtcNow.AddDays(10), EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3), CategoryId = 1 },
                new Event { Title = "Jazz Festival", Description = "Jazz music", City = "NYC", Address = "Central Park", StartDateTime = DateTime.UtcNow.AddDays(15), EndDateTime = DateTime.UtcNow.AddDays(15).AddHours(5), CategoryId = 1 },
                new Event { Title = "Football Game", Description = "Sports event", City = "LA", Address = "Stadium", StartDateTime = DateTime.UtcNow.AddDays(20), EndDateTime = DateTime.UtcNow.AddDays(20).AddHours(4), CategoryId = 2 }
            };
            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            var searchHandler = new EventSearchHandler { Keyword = "Concert" };

            // Act
            var result = await _repository.SearchEventsAsync(searchHandler);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("Rock Concert");
        }

        /// <summary>
        /// Tests search by keyword in description.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_ByKeywordInDescription_ShouldReturnMatchingEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Title = "Event 1", Description = "Amazing rock show", City = "NYC", Address = "MSG", StartDateTime = DateTime.UtcNow.AddDays(10), EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3), CategoryId = 1 },
                new Event { Title = "Event 2", Description = "Classical music", City = "NYC", Address = "Hall", StartDateTime = DateTime.UtcNow.AddDays(15), EndDateTime = DateTime.UtcNow.AddDays(15).AddHours(3), CategoryId = 1 }
            };
            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            var searchHandler = new EventSearchHandler { Keyword = "rock" };

            // Act
            var result = await _repository.SearchEventsAsync(searchHandler);

            // Assert
            result.Items.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests search by category.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_ByCategoryId_ShouldReturnMatchingEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Title = "Concert", Description = "Music", City = "NYC", Address = "MSG", StartDateTime = DateTime.UtcNow.AddDays(10), EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3), CategoryId = 1 },
                new Event { Title = "Game", Description = "Sports", City = "NYC", Address = "Stadium", StartDateTime = DateTime.UtcNow.AddDays(15), EndDateTime = DateTime.UtcNow.AddDays(15).AddHours(3), CategoryId = 2 }
            };
            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            var searchHandler = new EventSearchHandler { CategoryId = 2 };

            // Act
            var result = await _repository.SearchEventsAsync(searchHandler);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("Game");
        }

        /// <summary>
        /// Tests search by city.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_ByCity_ShouldReturnMatchingEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Title = "NYC Event", Description = "Event in NYC", City = "New York", Address = "MSG", StartDateTime = DateTime.UtcNow.AddDays(10), EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3), CategoryId = 1 },
                new Event { Title = "LA Event", Description = "Event in LA", City = "Los Angeles", Address = "Staples", StartDateTime = DateTime.UtcNow.AddDays(15), EndDateTime = DateTime.UtcNow.AddDays(15).AddHours(3), CategoryId = 1 }
            };
            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            var searchHandler = new EventSearchHandler { City = "Los Angeles" };

            // Act
            var result = await _repository.SearchEventsAsync(searchHandler);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("LA Event");
        }

        /// <summary>
        /// Tests search by start date.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_ByStartDate_ShouldReturnEventsAfterDate()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Title = "Early Event", Description = "Early", City = "NYC", Address = "Venue", StartDateTime = DateTime.UtcNow.AddDays(5), EndDateTime = DateTime.UtcNow.AddDays(5).AddHours(3), CategoryId = 1 },
                new Event { Title = "Late Event", Description = "Late", City = "NYC", Address = "Venue", StartDateTime = DateTime.UtcNow.AddDays(20), EndDateTime = DateTime.UtcNow.AddDays(20).AddHours(3), CategoryId = 1 }
            };
            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            var searchHandler = new EventSearchHandler { StartDate = DateTime.UtcNow.AddDays(10) };

            // Act
            var result = await _repository.SearchEventsAsync(searchHandler);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("Late Event");
        }

        /// <summary>
        /// Tests search with multiple filters.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_WithMultipleFilters_ShouldReturnFilteredResults()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Title = "NYC Concert", Description = "Music", City = "New York", Address = "MSG", StartDateTime = DateTime.UtcNow.AddDays(10), EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3), CategoryId = 1 },
                new Event { Title = "NYC Game", Description = "Sports", City = "New York", Address = "Stadium", StartDateTime = DateTime.UtcNow.AddDays(15), EndDateTime = DateTime.UtcNow.AddDays(15).AddHours(3), CategoryId = 2 },
                new Event { Title = "LA Concert", Description = "Music", City = "Los Angeles", Address = "Bowl", StartDateTime = DateTime.UtcNow.AddDays(20), EndDateTime = DateTime.UtcNow.AddDays(20).AddHours(3), CategoryId = 1 }
            };
            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            var searchHandler = new EventSearchHandler { City = "New York", CategoryId = 1 };

            // Act
            var result = await _repository.SearchEventsAsync(searchHandler);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("NYC Concert");
        }

        #endregion

        #region IsAlreadyBooked Tests

        /// <summary>
        /// Tests that IsAlreadyBooked returns true when user has booked the event.
        /// </summary>
        [Fact]
        public async Task IsAlreadyBooked_WhenUserHasBooked_ShouldReturnTrue()
        {
            // Arrange
            var testEvent = new Event
            {
                Title = "Test Event",
                Description = "Test",
                City = "NYC",
                Address = "Venue",
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3),
                CategoryId = 1
            };
            await _context.Events.AddAsync(testEvent);
            await _context.SaveChangesAsync();

            var eventTicketType = new EventTicketType
            {
                EventId = testEvent.EventId,
                TicketTypeId = 1,
                Price = 50.00m,
                TotalSeats = 100,
                AvailableSeats = 100
            };
            await _context.EventTicketTypes.AddAsync(eventTicketType);
            await _context.SaveChangesAsync();

            var booking = new Booking
            {
                UserId = "user-1",
                EventId = testEvent.EventId,
                EventTicketTypeId = eventTicketType.Id,
                NumTickets = 2,
                TotalPrice = 100.00m,
                BookingDate = DateTime.UtcNow
            };
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsAlreadyBooked("user-1", testEvent.EventId);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that IsAlreadyBooked returns false when user has not booked the event.
        /// </summary>
        [Fact]
        public async Task IsAlreadyBooked_WhenUserHasNotBooked_ShouldReturnFalse()
        {
            // Arrange
            var testEvent = new Event
            {
                Title = "Test Event",
                Description = "Test",
                City = "NYC",
                Address = "Venue",
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3),
                CategoryId = 1
            };
            await _context.Events.AddAsync(testEvent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsAlreadyBooked("user-1", testEvent.EventId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region HasAvailableTickets Tests

        /// <summary>
        /// Tests that HasAvailableTickets returns true when tickets are available.
        /// </summary>
        [Fact]
        public async Task HasAvailableTickets_WithSufficientSeats_ShouldReturnTrue()
        {
            // Arrange
            var testEvent = new Event
            {
                Title = "Test Event",
                Description = "Test",
                City = "NYC",
                Address = "Venue",
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3),
                CategoryId = 1
            };
            await _context.Events.AddAsync(testEvent);
            await _context.SaveChangesAsync();

            var eventTicketType = new EventTicketType
            {
                EventId = testEvent.EventId,
                TicketTypeId = 1,
                Price = 50.00m,
                TotalSeats = 100,
                AvailableSeats = 50
            };
            await _context.EventTicketTypes.AddAsync(eventTicketType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasAvailableTickets(testEvent.EventId, 1, 10);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that HasAvailableTickets returns false when not enough seats.
        /// </summary>
        [Fact]
        public async Task HasAvailableTickets_WithInsufficientSeats_ShouldReturnFalse()
        {
            // Arrange
            var testEvent = new Event
            {
                Title = "Test Event",
                Description = "Test",
                City = "NYC",
                Address = "Venue",
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3),
                CategoryId = 1
            };
            await _context.Events.AddAsync(testEvent);
            await _context.SaveChangesAsync();

            var eventTicketType = new EventTicketType
            {
                EventId = testEvent.EventId,
                TicketTypeId = 1,
                Price = 50.00m,
                TotalSeats = 100,
                AvailableSeats = 5
            };
            await _context.EventTicketTypes.AddAsync(eventTicketType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasAvailableTickets(testEvent.EventId, 1, 10);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that HasAvailableTickets returns false for non-existing ticket type.
        /// </summary>
        [Fact]
        public async Task HasAvailableTickets_WithNonExistingTicketType_ShouldReturnFalse()
        {
            // Arrange
            var testEvent = new Event
            {
                Title = "Test Event",
                Description = "Test",
                City = "NYC",
                Address = "Venue",
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3),
                CategoryId = 1
            };
            await _context.Events.AddAsync(testEvent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasAvailableTickets(testEvent.EventId, 999, 10);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetUserBookedEventIdsAsync Tests

        /// <summary>
        /// Tests that GetUserBookedEventIdsAsync returns set of booked event IDs.
        /// </summary>
        [Fact]
        public async Task GetUserBookedEventIdsAsync_WithBookings_ShouldReturnEventIds()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Title = "Event 1", Description = "Desc", City = "NYC", Address = "Venue", StartDateTime = DateTime.UtcNow.AddDays(10), EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3), CategoryId = 1 },
                new Event { Title = "Event 2", Description = "Desc", City = "NYC", Address = "Venue", StartDateTime = DateTime.UtcNow.AddDays(15), EndDateTime = DateTime.UtcNow.AddDays(15).AddHours(3), CategoryId = 1 }
            };
            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            foreach (var evt in events)
            {
                var ett = new EventTicketType
                {
                    EventId = evt.EventId,
                    TicketTypeId = 1,
                    Price = 50.00m,
                    TotalSeats = 100,
                    AvailableSeats = 100
                };
                await _context.EventTicketTypes.AddAsync(ett);
            }
            await _context.SaveChangesAsync();

            var bookings = new List<Booking>
            {
                new Booking { UserId = "user-1", EventId = events[0].EventId, EventTicketTypeId = 1, NumTickets = 1, TotalPrice = 50.00m, BookingDate = DateTime.UtcNow },
                new Booking { UserId = "user-1", EventId = events[1].EventId, EventTicketTypeId = 2, NumTickets = 2, TotalPrice = 100.00m, BookingDate = DateTime.UtcNow }
            };
            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserBookedEventIdsAsync("user-1");

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(events[0].EventId);
            result.Should().Contain(events[1].EventId);
        }

        #endregion

        #region CountAsync Tests

        /// <summary>
        /// Tests that CountAsync returns correct event count.
        /// </summary>
        [Fact]
        public async Task CountAsync_WithEvents_ShouldReturnCorrectCount()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Title = "Event 1", Description = "Desc", City = "NYC", Address = "Venue", StartDateTime = DateTime.UtcNow.AddDays(10), EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3), CategoryId = 1 },
                new Event { Title = "Event 2", Description = "Desc", City = "NYC", Address = "Venue", StartDateTime = DateTime.UtcNow.AddDays(15), EndDateTime = DateTime.UtcNow.AddDays(15).AddHours(3), CategoryId = 1 },
                new Event { Title = "Event 3", Description = "Desc", City = "NYC", Address = "Venue", StartDateTime = DateTime.UtcNow.AddDays(20), EndDateTime = DateTime.UtcNow.AddDays(20).AddHours(3), CategoryId = 1 }
            };
            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CountAsync();

            // Assert
            result.Should().Be(3);
        }

        #endregion

        #region DeleteEventAsync Tests

        /// <summary>
        /// Tests that DeleteEventAsync removes an event from the database.
        /// </summary>
        [Fact]
        public async Task DeleteEventAsync_WithExistingEvent_ShouldRemoveEvent()
        {
            // Arrange
            var testEvent = new Event
            {
                Title = "Test Event",
                Description = "Test",
                City = "NYC",
                Address = "Venue",
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3),
                CategoryId = 1
            };
            await _context.Events.AddAsync(testEvent);
            await _context.SaveChangesAsync();
            var eventId = testEvent.EventId;

            // Act
            await _repository.DeleteEventAsync(eventId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedEvent = await _context.Events.FindAsync(eventId);
            deletedEvent.Should().BeNull();
        }

        #endregion

        #region UpdateEvent Tests

        /// <summary>
        /// Tests that UpdateEvent modifies an existing event.
        /// </summary>
        [Fact]
        public async Task UpdateEvent_WithExistingEvent_ShouldModifyEvent()
        {
            // Arrange
            var testEvent = new Event
            {
                Title = "Original Title",
                Description = "Original Description",
                City = "NYC",
                Address = "Venue",
                StartDateTime = DateTime.UtcNow.AddDays(10),
                EndDateTime = DateTime.UtcNow.AddDays(10).AddHours(3),
                CategoryId = 1
            };
            await _context.Events.AddAsync(testEvent);
            await _context.SaveChangesAsync();
            _context.Entry(testEvent).State = EntityState.Detached;

            // Act
            testEvent.Title = "Updated Title";
            _repository.UpdateEvent(testEvent);
            await _context.SaveChangesAsync();

            // Assert
            var updatedEvent = await _context.Events.FindAsync(testEvent.EventId);
            updatedEvent!.Title.Should().Be("Updated Title");
        }

        #endregion
    }
}
