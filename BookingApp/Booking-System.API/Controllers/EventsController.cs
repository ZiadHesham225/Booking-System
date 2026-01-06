using Booking_System.Application.Interfaces;
using Booking_System.Application.Common;
using Booking_System.Application.DTOs.Event;
using Booking_System.Application.DTOs.EventTicketType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booking_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventService eventService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        /// <summary>
        /// Get all events with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<EventDto>>>> GetAllEvents(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _eventService.GetAllEventsAsync(userId, pageIndex, pageSize);
                return Ok(ApiResponse<PaginatedResponse<EventDto>>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse<PaginatedResponse<EventDto>>.Failure("Error retrieving events"));
            }
        }

        /// <summary>
        /// Search events with various filters
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<EventDto>>>> SearchEvents(
            [FromQuery] string? keyword = null,
            [FromQuery] string? city = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isDescending = false,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20
            )
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var searchHandler = new EventSearchHandler
                {
                    Keyword = keyword,
                    City = city,
                    CategoryId = categoryId,
                    StartDate = startDate,
                    EndDate = endDate,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    SortBy = sortBy,
                    IsDescending = isDescending
                };
                var result = await _eventService.SearchEventsAsync(searchHandler, userId, pageIndex, pageSize);
                return Ok(ApiResponse<PaginatedResponse<EventDto>>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching events");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse<PaginatedResponse<EventDto>>.Failure("Error searching events"));
            }
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EventDto>>> GetEventById(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _eventService.GetEventByIdAsync(id, userId);

                if (result == null)
                    return NotFound(ApiResponse<EventDto>.Failure($"Event with ID {id} not found"));

                return Ok(ApiResponse<EventDto>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event with ID {EventId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse<EventDto>.Failure($"Error retrieving event with ID {id}"));
            }
        }

        /// <summary>
        /// Create a new event with ticket types
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<EventDto>>> CreateEvent([FromForm] CreateEventDto eventDto, [FromForm] List<CreateEventTicketTypeDto> ticketTypes)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<EventDto>.Failure("Invalid data"));

            if (ticketTypes == null || !ticketTypes.Any())
                return BadRequest(ApiResponse<EventDto>.Failure("At least one ticket type is required"));

            try
            {
                var createdEvent = await _eventService.CreateEventAsync(eventDto, ticketTypes);
                return Ok(ApiResponse<EventDto>.Success(null!, "Event created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse<EventDto>.Failure("Error creating event"));
            }
        }

        /// <summary>
        /// Update an existing event
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateEvent([FromForm] UpdateEventDto eventDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Failure("Invalid data"));

            try
            {
                await _eventService.UpdateEventAsync(eventDto);
                return Ok(ApiResponse.Success("Event updated successfully"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating event with ID {EventId}", eventDto.EventId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse.Failure($"Error updating event with ID {eventDto.EventId}"));
            }
        }

        /// <summary>
        /// Delete an event by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> DeleteEvent(int id)
        {
            try
            {
                await _eventService.DeleteEventAsync(id);
                return Ok(ApiResponse.Success("Event deleted successfully"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting event with ID {EventId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse.Failure($"Error deleting event with ID {id}"));
            }
        }
    }
}


