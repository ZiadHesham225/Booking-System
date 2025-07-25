using Booking_System.Business_Logic.Interfaces;
using Booking_System.Common;
using Booking_System.DTOs;
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
        public async Task<ActionResult<PaginatedResponse<EventDto>>> GetAllEvents(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _eventService.GetAllEventsAsync(userId, pageIndex, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving events");
            }
        }

        /// <summary>
        /// Search events with various filters
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<PaginatedResponse<EventDto>>> SearchEvents(
            [FromBody] EventSearchHandler? searchHandler = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20
            )
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                searchHandler ??= new EventSearchHandler();
                var result = await _eventService.SearchEventsAsync(searchHandler, userId, pageIndex, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching events");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error searching events");
            }
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEventById(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _eventService.GetEventByIdAsync(id, userId);

                if (result == null)
                    return NotFound($"Event with ID {id} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event with ID {EventId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving event with ID {id}");
            }
        }

        /// <summary>
        /// Create a new event with ticket types
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateEventDto>> CreateEvent([FromForm] CreateEventDto eventDto, [FromForm] List<CreateEventTicketTypeDto> ticketTypes)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (ticketTypes == null || !ticketTypes.Any())
                return BadRequest("At least one ticket type is required");

            try
            {
                var createdEvent = await _eventService.CreateEventAsync(eventDto, ticketTypes);
                return Created();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating event");
            }
        }

        /// <summary>
        /// Update an existing event
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateEvent([FromForm] UpdateEventDto eventDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _eventService.UpdateEventAsync(eventDto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating event with ID {EventId}", eventDto.EventId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating event with ID {eventDto.EventId}");
            }
        }

        /// <summary>
        /// Delete an event by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            try
            {
                await _eventService.DeleteEventAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting event with ID {EventId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting event with ID {id}");
            }
        }
    }
}
