using Booking_System.Application.Interfaces;
using Booking_System.Application.Common;
using Booking_System.Application.DTOs.TicketType;
using Booking_System.Application.DTOs.EventTicketType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booking_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketTypeController : ControllerBase
    {
        private readonly ITicketTypeService _ticketTypeService;

        public TicketTypeController(ITicketTypeService ticketTypeService)
        {
            _ticketTypeService = ticketTypeService;
        }

        /// <summary>
        /// Get all ticket types
        /// </summary>
        /// <returns>List of all ticket types</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TicketTypeDto>>>> GetAll()
        {
            try
            {
                var ticketTypes = await _ticketTypeService.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<TicketTypeDto>>.Success(ticketTypes));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<TicketTypeDto>>.Failure("An error occurred while retrieving ticket types."));
            }
        }

        /// <summary>
        /// Get all active ticket types
        /// </summary>
        /// <returns>List of active ticket types</returns>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TicketTypeDto>>>> GetActiveTicketTypes()
        {
            try
            {
                var ticketTypes = await _ticketTypeService.GetActiveTicketTypesAsync();
                return Ok(ApiResponse<IEnumerable<TicketTypeDto>>.Success(ticketTypes));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<TicketTypeDto>>.Failure("An error occurred while retrieving active ticket types."));
            }
        }

        /// <summary>
        /// Get ticket type by ID
        /// </summary>
        /// <param name="id">Ticket type ID</param>
        /// <returns>Ticket type details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TicketTypeDto>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse<TicketTypeDto>.Failure("Invalid ticket type ID."));

                var ticketType = await _ticketTypeService.GetByIdAsync(id);
                if (ticketType == null)
                    return NotFound(ApiResponse<TicketTypeDto>.Failure("Ticket type not found."));

                return Ok(ApiResponse<TicketTypeDto>.Success(ticketType));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TicketTypeDto>.Failure("An error occurred while retrieving the ticket type."));
            }
        }

        /// <summary>
        /// Get ticket type by name
        /// </summary>
        /// <param name="name">Ticket type name</param>
        /// <returns>Ticket type details</returns>
        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<ApiResponse<TicketTypeDto>>> GetByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(ApiResponse<TicketTypeDto>.Failure("Ticket type name cannot be empty."));

                var ticketType = await _ticketTypeService.GetByNameAsync(name);
                if (ticketType == null)
                    return NotFound(ApiResponse<TicketTypeDto>.Failure("Ticket type not found."));

                return Ok(ApiResponse<TicketTypeDto>.Success(ticketType));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TicketTypeDto>.Failure("An error occurred while retrieving the ticket type."));
            }
        }

        /// <summary>
        /// Create a new ticket type
        /// </summary>
        /// <param name="dto">Create ticket type DTO</param>
        /// <returns>Created ticket type</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TicketTypeDto>>> Create([FromBody] CreateTicketTypeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<TicketTypeDto>.Failure("Invalid data"));

                var ticketType = await _ticketTypeService.CreateAsync(dto);
                return Ok(ApiResponse<TicketTypeDto>.Success(ticketType, "Ticket type created successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<TicketTypeDto>.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TicketTypeDto>.Failure("An error occurred while creating the ticket type."));
            }
        }

        /// <summary>
        /// Update an existing ticket type
        /// </summary>
        /// <param name="id">Ticket type ID</param>
        /// <param name="dto">Update ticket type DTO</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateTicketTypeDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse.Failure("Invalid ticket type ID."));

                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Failure("Invalid data"));

                if (dto.TicketTypeId != id)
                    return BadRequest(ApiResponse.Failure("ID in route does not match ID in request body."));

                await _ticketTypeService.UpdateAsync(dto);
                return Ok(ApiResponse.Success("Ticket type updated successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("An error occurred while updating the ticket type."));
            }
        }

        /// <summary>
        /// Delete a ticket type
        /// </summary>
        /// <param name="id">Ticket type ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse.Failure("Invalid ticket type ID."));

                await _ticketTypeService.DeleteAsync(id);
                return Ok(ApiResponse.Success("Ticket type deleted successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("An error occurred while deleting the ticket type."));
            }
        }

        /// <summary>
        /// Toggle the active status of a ticket type
        /// </summary>
        /// <param name="id">Ticket type ID</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<ApiResponse>> ToggleActiveStatus(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse.Failure("Invalid ticket type ID."));

                await _ticketTypeService.ToggleActiveStatusAsync(id);
                return Ok(ApiResponse.Success("Ticket type status toggled successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("An error occurred while toggling the ticket type status."));
            }
        }
    }
}


