using Booking_System.Application.Common;
using Booking_System.Application.DTOs.Event;
using Booking_System.Application.DTOs.EventTicketType;
using Booking_System.Application.Interfaces;
using Booking_System.Application.Services;
using Booking_System.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Booking_System.Application.Tests.Services
{
    /// <summary>
    /// Unit tests for EventService covering event creation, retrieval, update, deletion, and search functionality.
    /// </summary>
    public class EventServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IImageService> _mockImageService;
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<IEventTicketTypeRepository> _mockEventTicketTypeRepository;
        private readonly EventService _sut;

        public EventServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockImageService = new Mock<IImageService>();
            _mockEventRepository = new Mock<IEventRepository>();
            _mockEventTicketTypeRepository = new Mock<IEventTicketTypeRepository>();

            _mockUnitOfWork.Setup(x => x.Events).Returns(_mockEventRepository.Object);
            _mockUnitOfWork.Setup(x => x.EventTicketTypes).Returns(_mockEventTicketTypeRepository.Object);

            _sut = new EventService(_mockUnitOfWork.Object, _mockImageService.Object);
        }

        #region CreateEvent Tests

        /// <summary>
        /// Verifies that CreateEventAsync successfully creates an event with ticket types.
        /// </summary>
        [Fact]
        public async Task CreateEventAsync_ValidData_ReturnsCreatedEvent()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("event.jpg");
            mockFile.Setup(f => f.Length).Returns(1024);

            var eventDto = new CreateEventDto
            {
                Title = "Test Concert",
                Description = "A great concert",
                StartDateTime = DateTime.UtcNow.AddDays(7),
                EndDateTime = DateTime.UtcNow.AddDays(7).AddHours(3),
                City = "New York",
                Address = "123 Main St",
                CategoryId = 1,
                EventPicture = mockFile.Object
            };

            var ticketTypes = new List<CreateEventTicketTypeDto>
            {
                new CreateEventTicketTypeDto { TicketTypeId = 1, Price = 50m, TotalSeats = 100 },
                new CreateEventTicketTypeDto { TicketTypeId = 2, Price = 100m, TotalSeats = 50 }
            };

            var createdEvent = new Event
            {
                EventId = 1,
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartDateTime = eventDto.StartDateTime,
                EndDateTime = eventDto.EndDateTime,
                City = eventDto.City,
                Address = eventDto.Address,
                CategoryId = eventDto.CategoryId,
                ImageUrl = "images/Events/event.jpg"
            };

            _mockImageService.Setup(x => x.SaveImageAsync(eventDto.EventPicture, "Events"))
                .ReturnsAsync("images/Events/event.jpg");
            _mockEventRepository.Setup(x => x.CreateEventAsync(It.IsAny<Event>()))
                .ReturnsAsync(createdEvent);
            _mockEventTicketTypeRepository.Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<EventTicketType>>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateEventAsync(eventDto, ticketTypes);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(eventDto.Title);
            _mockEventRepository.Verify(x => x.CreateEventAsync(It.IsAny<Event>()), Times.Once);
            _mockEventTicketTypeRepository.Verify(x => x.CreateRangeAsync(It.IsAny<IEnumerable<EventTicketType>>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that CreateEventAsync throws exception when an error occurs.
        /// </summary>
        [Fact]
        public async Task CreateEventAsync_DatabaseError_ThrowsApplicationException()
        {
            // Arrange
            var eventDto = new CreateEventDto
            {
                Title = "Test Event",
                Description = "Description"
            };
            var ticketTypes = new List<CreateEventTicketTypeDto>
            {
                new CreateEventTicketTypeDto { TicketTypeId = 1, Price = 50m, TotalSeats = 100 }
            };

            _mockEventRepository.Setup(x => x.CreateEventAsync(It.IsAny<Event>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            Func<Task> act = async () => await _sut.CreateEventAsync(eventDto, ticketTypes);

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("Error creating event with ticket types");
        }

        #endregion

        #region GetAllEvents Tests

        /// <summary>
        /// Verifies that GetAllEventsAsync returns paginated events.
        /// </summary>
        [Fact]
        public async Task GetAllEventsAsync_ValidRequest_ReturnsPaginatedEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                CreateTestEvent(1, "Event 1"),
                CreateTestEvent(2, "Event 2")
            };

            var paginatedResponse = new PaginatedResponse<Event>
            {
                Items = events,
                CurrentPage = 1,
                TotalPages = 1,
                TotalItems = 2,
                HasPreviousPage = false,
                HasNextPage = false
            };

            _mockEventRepository.Setup(x => x.GetAllEventsAsync(1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.GetAllEventsAsync(null, 1, 20);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.CurrentPage.Should().Be(1);
            result.TotalItems.Should().Be(2);
        }

        /// <summary>
        /// Verifies that GetAllEventsAsync marks booked events for authenticated user.
        /// </summary>
        [Fact]
        public async Task GetAllEventsAsync_WithUserId_MarksBookedEvents()
        {
            // Arrange
            var userId = "user-123";
            var events = new List<Event>
            {
                CreateTestEvent(1, "Event 1"),
                CreateTestEvent(2, "Event 2")
            };

            var paginatedResponse = new PaginatedResponse<Event>
            {
                Items = events,
                CurrentPage = 1,
                TotalPages = 1,
                TotalItems = 2
            };

            var bookedEventIds = new HashSet<int> { 1 }; // User has booked event 1

            _mockEventRepository.Setup(x => x.GetAllEventsAsync(1, 20))
                .ReturnsAsync(paginatedResponse);
            _mockEventRepository.Setup(x => x.GetUserBookedEventIdsAsync(userId))
                .ReturnsAsync(bookedEventIds);

            // Act
            var result = await _sut.GetAllEventsAsync(userId, 1, 20);

            // Assert
            result.Should().NotBeNull();
            var eventsList = result.Items.ToList();
            eventsList.First(e => e.EventId == 1).isBooked.Should().BeTrue();
            eventsList.First(e => e.EventId == 2).isBooked.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that GetAllEventsAsync throws exception when database error occurs.
        /// </summary>
        [Fact]
        public async Task GetAllEventsAsync_DatabaseError_ThrowsApplicationException()
        {
            // Arrange
            _mockEventRepository.Setup(x => x.GetAllEventsAsync(1, 20))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            Func<Task> act = async () => await _sut.GetAllEventsAsync(null, 1, 20);

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("Error retrieving events");
        }

        #endregion

        #region GetEventById Tests

        /// <summary>
        /// Verifies that GetEventByIdAsync returns event details for valid ID.
        /// </summary>
        [Fact]
        public async Task GetEventByIdAsync_ValidId_ReturnsEventDto()
        {
            // Arrange
            var eventId = 1;
            var eventEntity = CreateTestEvent(eventId, "Test Event");

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(eventEntity);

            // Act
            var result = await _sut.GetEventByIdAsync(eventId);

            // Assert
            result.Should().NotBeNull();
            result.EventId.Should().Be(eventId);
            result.Title.Should().Be("Test Event");
        }

        /// <summary>
        /// Verifies that GetEventByIdAsync throws exception when event is not found.
        /// </summary>
        [Fact]
        public async Task GetEventByIdAsync_EventNotFound_ThrowsApplicationException()
        {
            // Arrange
            var eventId = 999;

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync((Event?)null);

            // Act
            Func<Task> act = async () => await _sut.GetEventByIdAsync(eventId);

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage($"Error retrieving event with ID {eventId}");
        }

        /// <summary>
        /// Verifies that GetEventByIdAsync marks event as booked for authenticated user.
        /// </summary>
        [Fact]
        public async Task GetEventByIdAsync_WithUserId_MarksBookedEvent()
        {
            // Arrange
            var eventId = 1;
            var userId = "user-123";
            var eventEntity = CreateTestEvent(eventId, "Test Event");
            var bookedEventIds = new HashSet<int> { eventId };

            _mockEventRepository.Setup(x => x.GetEventByIdAsync(eventId))
                .ReturnsAsync(eventEntity);
            _mockEventRepository.Setup(x => x.GetUserBookedEventIdsAsync(userId))
                .ReturnsAsync(bookedEventIds);

            // Act
            var result = await _sut.GetEventByIdAsync(eventId, userId);

            // Assert
            result.Should().NotBeNull();
            result.isBooked.Should().BeTrue();
        }

        #endregion

        #region SearchEvents Tests

        /// <summary>
        /// Verifies that SearchEventsAsync returns filtered events by keyword.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_WithKeyword_ReturnsFilteredEvents()
        {
            // Arrange
            var searchHandler = new EventSearchHandler { Keyword = "Concert" };
            var events = new List<Event>
            {
                CreateTestEvent(1, "Rock Concert")
            };

            var paginatedResponse = new PaginatedResponse<Event>
            {
                Items = events,
                CurrentPage = 1,
                TotalPages = 1,
                TotalItems = 1
            };

            _mockEventRepository.Setup(x => x.SearchEventsAsync(searchHandler, 1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.SearchEventsAsync(searchHandler, null, 1, 20);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Contain("Concert");
        }

        /// <summary>
        /// Verifies that SearchEventsAsync returns filtered events by city.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_WithCity_ReturnsFilteredEvents()
        {
            // Arrange
            var searchHandler = new EventSearchHandler { City = "New York" };
            var events = new List<Event>
            {
                CreateTestEvent(1, "NYC Event", city: "New York")
            };

            var paginatedResponse = new PaginatedResponse<Event>
            {
                Items = events,
                CurrentPage = 1,
                TotalPages = 1,
                TotalItems = 1
            };

            _mockEventRepository.Setup(x => x.SearchEventsAsync(searchHandler, 1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.SearchEventsAsync(searchHandler, null, 1, 20);

            // Assert
            result.Should().NotBeNull();
            result.Items.First().City.Should().Be("New York");
        }

        /// <summary>
        /// Verifies that SearchEventsAsync returns filtered events by category.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_WithCategoryId_ReturnsFilteredEvents()
        {
            // Arrange
            var searchHandler = new EventSearchHandler { CategoryId = 1 };
            var events = new List<Event>
            {
                CreateTestEvent(1, "Music Event", categoryId: 1)
            };

            var paginatedResponse = new PaginatedResponse<Event>
            {
                Items = events,
                CurrentPage = 1,
                TotalPages = 1,
                TotalItems = 1
            };

            _mockEventRepository.Setup(x => x.SearchEventsAsync(searchHandler, 1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.SearchEventsAsync(searchHandler, null, 1, 20);

            // Assert
            result.Should().NotBeNull();
            result.Items.First().CategoryId.Should().Be(1);
        }

        /// <summary>
        /// Verifies that SearchEventsAsync returns empty result when no events match criteria.
        /// </summary>
        [Fact]
        public async Task SearchEventsAsync_NoMatchingEvents_ReturnsEmptyResult()
        {
            // Arrange
            var searchHandler = new EventSearchHandler { Keyword = "NonExistent" };
            var paginatedResponse = new PaginatedResponse<Event>
            {
                Items = new List<Event>(),
                CurrentPage = 1,
                TotalPages = 0,
                TotalItems = 0
            };

            _mockEventRepository.Setup(x => x.SearchEventsAsync(searchHandler, 1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.SearchEventsAsync(searchHandler, null, 1, 20);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalItems.Should().Be(0);
        }

        #endregion

        #region Pagination Tests

        /// <summary>
        /// Verifies that pagination works correctly with various page sizes.
        /// </summary>
        [Theory]
        [InlineData(1, 10, 50, 5)]
        [InlineData(2, 20, 50, 3)]
        [InlineData(1, 100, 50, 1)]
        public async Task GetAllEventsAsync_Pagination_ReturnsCorrectTotalPages(
            int pageIndex, int pageSize, int totalItems, int expectedTotalPages)
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<Event>
            {
                Items = new List<Event>(),
                CurrentPage = pageIndex,
                TotalPages = expectedTotalPages,
                TotalItems = totalItems,
                HasPreviousPage = pageIndex > 1,
                HasNextPage = pageIndex < expectedTotalPages
            };

            _mockEventRepository.Setup(x => x.GetAllEventsAsync(pageIndex, pageSize))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.GetAllEventsAsync(null, pageIndex, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.TotalPages.Should().Be(expectedTotalPages);
            result.TotalItems.Should().Be(totalItems);
        }

        /// <summary>
        /// Verifies that HasPreviousPage and HasNextPage are set correctly.
        /// </summary>
        [Fact]
        public async Task GetAllEventsAsync_MiddlePage_HasBothPreviousAndNextPage()
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<Event>
            {
                Items = new List<Event>(),
                CurrentPage = 2,
                TotalPages = 3,
                TotalItems = 60,
                HasPreviousPage = true,
                HasNextPage = true
            };

            _mockEventRepository.Setup(x => x.GetAllEventsAsync(2, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.GetAllEventsAsync(null, 2, 20);

            // Assert
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeTrue();
        }

        #endregion

        #region Helper Methods

        private Event CreateTestEvent(int id, string title, string city = "Test City", int categoryId = 1)
        {
            return new Event
            {
                EventId = id,
                Title = title,
                Description = "Test Description",
                StartDateTime = DateTime.UtcNow.AddDays(7),
                EndDateTime = DateTime.UtcNow.AddDays(7).AddHours(3),
                City = city,
                Address = "123 Test St",
                CategoryId = categoryId,
                Category = new Category { CategoryId = categoryId, Name = "Test Category" },
                EventTicketTypes = new List<EventTicketType>
                {
                    new EventTicketType
                    {
                        Id = id,
                        EventId = id,
                        TicketTypeId = 1,
                        Price = 50m,
                        TotalSeats = 100,
                        AvailableSeats = 100,
                        TicketType = new TicketType { TicketTypeId = 1, Name = "General" }
                    }
                }
            };
        }

        #endregion
    }
}
