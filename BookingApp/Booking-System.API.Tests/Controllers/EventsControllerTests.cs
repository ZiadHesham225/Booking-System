using Booking_System.Application.Common;
using Booking_System.Application.DTOs.Event;
using Booking_System.Application.DTOs.EventTicketType;
using Booking_System.Application.Interfaces;
using Booking_System.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace Booking_System.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for EventsController covering event management endpoints.
    /// </summary>
    public class EventsControllerTests
    {
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<ILogger<EventsController>> _mockLogger;
        private readonly EventsController _sut;

        public EventsControllerTests()
        {
            _mockEventService = new Mock<IEventService>();
            _mockLogger = new Mock<ILogger<EventsController>>();
            _sut = new EventsController(_mockEventService.Object, _mockLogger.Object);
            
            // Set up default HttpContext to prevent NullReferenceException when accessing User
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        #region GetAllEvents Tests

        /// <summary>
        /// Verifies that GetAllEvents returns Ok with paginated events.
        /// </summary>
        [Fact]
        public async Task GetAllEvents_ValidRequest_ReturnsOkWithPaginatedEvents()
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<EventDto>
            {
                Items = new List<EventDto>
                {
                    new EventDto { EventId = 1, Title = "Concert" },
                    new EventDto { EventId = 2, Title = "Sports Event" }
                },
                CurrentPage = 1,
                TotalPages = 1,
                TotalItems = 2,
                HasPreviousPage = false,
                HasNextPage = false
            };

            _mockEventService.Setup(x => x.GetAllEventsAsync(It.IsAny<string?>(), 1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.GetAllEvents(1, 20);

            // Assert
            result.Result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = objectResult.Value as PaginatedResponse<EventDto>;
            response.Should().NotBeNull();
            response!.Items.Should().HaveCount(2);
        }

        /// <summary>
        /// Verifies that GetAllEvents returns Ok with empty list when no events exist.
        /// </summary>
        [Fact]
        public async Task GetAllEvents_NoEvents_ReturnsOkWithEmptyList()
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<EventDto>
            {
                Items = new List<EventDto>(),
                CurrentPage = 1,
                TotalPages = 0,
                TotalItems = 0
            };

            _mockEventService.Setup(x => x.GetAllEventsAsync(It.IsAny<string?>(), 1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.GetAllEvents(1, 20);

            // Assert
            result.Result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = objectResult.Value as PaginatedResponse<EventDto>;
            response!.Items.Should().BeEmpty();
        }

        /// <summary>
        /// Verifies that GetAllEvents marks booked events for authenticated user.
        /// </summary>
        [Fact]
        public async Task GetAllEvents_AuthenticatedUser_ReturnsEventsWithBookedStatus()
        {
            // Arrange
            var userId = "user-123";
            SetupControllerWithUser(userId);

            var paginatedResponse = new PaginatedResponse<EventDto>
            {
                Items = new List<EventDto>
                {
                    new EventDto { EventId = 1, Title = "Concert", isBooked = true },
                    new EventDto { EventId = 2, Title = "Sports Event", isBooked = false }
                },
                CurrentPage = 1,
                TotalPages = 1,
                TotalItems = 2
            };

            _mockEventService.Setup(x => x.GetAllEventsAsync(userId, 1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.GetAllEvents(1, 20);

            // Assert
            result.Result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = objectResult.Value as PaginatedResponse<EventDto>;
            response!.Items.First().isBooked.Should().BeTrue();
            response.Items.Last().isBooked.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that GetAllEvents returns 500 when service throws exception.
        /// </summary>
        [Fact]
        public async Task GetAllEvents_ServiceError_ReturnsInternalServerError()
        {
            // Arrange
            _mockEventService.Setup(x => x.GetAllEventsAsync(It.IsAny<string?>(), 1, 20))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetAllEvents(1, 20);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        #endregion

        #region SearchEvents Tests

        /// <summary>
        /// Verifies that SearchEvents returns Ok with filtered events.
        /// </summary>
        [Fact]
        public async Task SearchEvents_ValidSearchCriteria_ReturnsOkWithFilteredEvents()
        {
            // Arrange
            var searchHandler = new EventSearchHandler { Keyword = "Concert" };
            var paginatedResponse = new PaginatedResponse<EventDto>
            {
                Items = new List<EventDto>
                {
                    new EventDto { EventId = 1, Title = "Rock Concert" }
                },
                CurrentPage = 1,
                TotalPages = 1,
                TotalItems = 1
            };

            _mockEventService.Setup(x => x.SearchEventsAsync(It.IsAny<EventSearchHandler>(), It.IsAny<string?>(), 1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.SearchEvents(searchHandler, 1, 20);

            // Assert
            result.Result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = objectResult.Value as PaginatedResponse<EventDto>;
            response!.Items.Should().HaveCount(1);
        }

        /// <summary>
        /// Verifies that SearchEvents with null handler creates default handler.
        /// </summary>
        [Fact]
        public async Task SearchEvents_NullSearchHandler_UsesDefaultHandler()
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<EventDto>
            {
                Items = new List<EventDto>(),
                CurrentPage = 1,
                TotalPages = 0,
                TotalItems = 0
            };

            _mockEventService.Setup(x => x.SearchEventsAsync(It.IsAny<EventSearchHandler>(), It.IsAny<string?>(), 1, 20))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.SearchEvents(null, 1, 20);

            // Assert
            result.Result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            _mockEventService.Verify(x => x.SearchEventsAsync(It.IsNotNull<EventSearchHandler>(), It.IsAny<string?>(), 1, 20), Times.Once);
        }

        #endregion

        #region GetEventById Tests

        /// <summary>
        /// Verifies that GetEventById returns Ok with event details.
        /// </summary>
        [Fact]
        public async Task GetEventById_ValidId_ReturnsOkWithEvent()
        {
            // Arrange
            var eventId = 1;
            var eventDto = new EventDto
            {
                EventId = eventId,
                Title = "Concert",
                Description = "A great concert"
            };

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId, It.IsAny<string?>()))
                .ReturnsAsync(eventDto);

            // Act
            var result = await _sut.GetEventById(eventId);

            // Assert
            result.Result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            var returnedEvent = objectResult.Value as EventDto;
            returnedEvent.Should().NotBeNull();
            returnedEvent!.EventId.Should().Be(eventId);
        }

        /// <summary>
        /// Verifies that GetEventById returns NotFound when event doesn't exist.
        /// </summary>
        [Fact]
        public async Task GetEventById_EventNotFound_ReturnsNotFound()
        {
            // Arrange
            var eventId = 999;

            _mockEventService.Setup(x => x.GetEventByIdAsync(eventId, It.IsAny<string?>()))
                .ReturnsAsync((EventDto?)null);

            // Act
            var result = await _sut.GetEventById(eventId);

            // Assert
            result.Result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        #endregion

        #region CreateEvent Tests

        /// <summary>
        /// Verifies that CreateEvent returns Created when event is created successfully (Admin only).
        /// </summary>
        [Fact]
        public async Task CreateEvent_ValidData_ReturnsCreated()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("event.jpg");

            var createEventDto = new CreateEventDto
            {
                Title = "New Concert",
                Description = "Amazing concert",
                StartDateTime = DateTime.UtcNow.AddDays(7),
                EndDateTime = DateTime.UtcNow.AddDays(7).AddHours(3),
                City = "New York",
                Address = "123 Main St",
                CategoryId = 1,
                EventPicture = mockFile.Object
            };

            var ticketTypes = new List<CreateEventTicketTypeDto>
            {
                new CreateEventTicketTypeDto { TicketTypeId = 1, Price = 50m, TotalSeats = 100 }
            };

            _mockEventService.Setup(x => x.CreateEventAsync(createEventDto, ticketTypes))
                .ReturnsAsync(createEventDto);

            // Act
            var result = await _sut.CreateEvent(createEventDto, ticketTypes);

            // Assert
            result.Result.Should().BeOfType<CreatedResult>();
        }

        /// <summary>
        /// Verifies that CreateEvent returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task CreateEvent_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var createEventDto = new CreateEventDto();
            var ticketTypes = new List<CreateEventTicketTypeDto>();
            _sut.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _sut.CreateEvent(createEventDto, ticketTypes);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Verifies that CreateEvent returns BadRequest when no ticket types provided.
        /// </summary>
        [Fact]
        public async Task CreateEvent_NoTicketTypes_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var createEventDto = new CreateEventDto
            {
                Title = "New Concert",
                EventPicture = mockFile.Object
            };
            var ticketTypes = new List<CreateEventTicketTypeDto>(); // Empty list

            // Act
            var result = await _sut.CreateEvent(createEventDto, ticketTypes);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("At least one ticket type is required");
        }

        /// <summary>
        /// Verifies that CreateEvent returns BadRequest when null ticket types provided.
        /// </summary>
        [Fact]
        public async Task CreateEvent_NullTicketTypes_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var createEventDto = new CreateEventDto
            {
                Title = "New Concert",
                EventPicture = mockFile.Object
            };

            // Act
            var result = await _sut.CreateEvent(createEventDto, null!);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region UpdateEvent Tests

        /// <summary>
        /// Verifies that UpdateEvent returns NoContent when event is updated successfully.
        /// </summary>
        [Fact]
        public async Task UpdateEvent_ValidData_ReturnsNoContent()
        {
            // Arrange
            var updateEventDto = new UpdateEventDto
            {
                EventId = 1,
                Title = "Updated Concert",
                Description = "Updated description"
            };

            _mockEventService.Setup(x => x.UpdateEventAsync(updateEventDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateEvent(updateEventDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        /// <summary>
        /// Verifies that UpdateEvent returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task UpdateEvent_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var updateEventDto = new UpdateEventDto();
            _sut.ModelState.AddModelError("EventId", "EventId is required");

            // Act
            var result = await _sut.UpdateEvent(updateEventDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Verifies that UpdateEvent returns NotFound when event doesn't exist.
        /// </summary>
        [Fact]
        public async Task UpdateEvent_EventNotFound_ReturnsNotFound()
        {
            // Arrange
            var updateEventDto = new UpdateEventDto { EventId = 999, Title = "Non-existent" };

            _mockEventService.Setup(x => x.UpdateEventAsync(updateEventDto))
                .ThrowsAsync(new ArgumentException("Event not found."));

            // Act
            var result = await _sut.UpdateEvent(updateEventDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region DeleteEvent Tests

        /// <summary>
        /// Verifies that DeleteEvent returns NoContent when event is deleted successfully.
        /// </summary>
        [Fact]
        public async Task DeleteEvent_ValidId_ReturnsNoContent()
        {
            // Arrange
            var eventId = 1;

            _mockEventService.Setup(x => x.DeleteEventAsync(eventId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteEvent(eventId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        /// <summary>
        /// Verifies that DeleteEvent returns NotFound when event doesn't exist.
        /// </summary>
        [Fact]
        public async Task DeleteEvent_EventNotFound_ReturnsNotFound()
        {
            // Arrange
            var eventId = 999;

            _mockEventService.Setup(x => x.DeleteEventAsync(eventId))
                .ThrowsAsync(new ArgumentException("Event not found."));

            // Act
            var result = await _sut.DeleteEvent(eventId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        /// <summary>
        /// Verifies that DeleteEvent returns 500 when service throws unexpected exception.
        /// </summary>
        [Fact]
        public async Task DeleteEvent_ServiceError_ReturnsInternalServerError()
        {
            // Arrange
            var eventId = 1;

            _mockEventService.Setup(x => x.DeleteEventAsync(eventId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.DeleteEvent(eventId);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        #endregion

        #region Pagination Tests

        /// <summary>
        /// Verifies that pagination parameters are passed correctly.
        /// </summary>
        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 20)]
        [InlineData(5, 50)]
        public async Task GetAllEvents_PaginationParams_PassedToService(int pageIndex, int pageSize)
        {
            // Arrange
            var paginatedResponse = new PaginatedResponse<EventDto>
            {
                Items = new List<EventDto>(),
                CurrentPage = pageIndex,
                TotalPages = 10,
                TotalItems = 100
            };

            _mockEventService.Setup(x => x.GetAllEventsAsync(It.IsAny<string?>(), pageIndex, pageSize))
                .ReturnsAsync(paginatedResponse);

            // Act
            var result = await _sut.GetAllEvents(pageIndex, pageSize);

            // Assert
            _mockEventService.Verify(x => x.GetAllEventsAsync(It.IsAny<string?>(), pageIndex, pageSize), Times.Once);
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
