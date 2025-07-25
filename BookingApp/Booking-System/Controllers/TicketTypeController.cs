using Booking_System.Business_Logic.Interfaces;
using Booking_System.DTOs;
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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var ticketTypes = await _ticketTypeService.GetAllAsync();
                return Ok(ticketTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving ticket types.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active ticket types
        /// </summary>
        /// <returns>List of active ticket types</returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTicketTypes()
        {
            try
            {
                var ticketTypes = await _ticketTypeService.GetActiveTicketTypesAsync();
                return Ok(ticketTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active ticket types.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get ticket type by ID
        /// </summary>
        /// <param name="id">Ticket type ID</param>
        /// <returns>Ticket type details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid ticket type ID." });

                var ticketType = await _ticketTypeService.GetByIdAsync(id);
                if (ticketType == null)
                    return NotFound(new { message = "Ticket type not found." });

                return Ok(ticketType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the ticket type.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get ticket type by name
        /// </summary>
        /// <param name="name">Ticket type name</param>
        /// <returns>Ticket type details</returns>
        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { message = "Ticket type name cannot be empty." });

                var ticketType = await _ticketTypeService.GetByNameAsync(name);
                if (ticketType == null)
                    return NotFound(new { message = "Ticket type not found." });

                return Ok(ticketType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the ticket type.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new ticket type
        /// </summary>
        /// <param name="dto">Create ticket type DTO</param>
        /// <returns>Created ticket type</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketTypeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ticketType = await _ticketTypeService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = ticketType.TicketTypeId }, ticketType);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the ticket type.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing ticket type
        /// </summary>
        /// <param name="id">Ticket type ID</param>
        /// <param name="dto">Update ticket type DTO</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketTypeDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid ticket type ID." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (dto.TicketTypeId != id)
                    return BadRequest(new { message = "ID in route does not match ID in request body." });

                await _ticketTypeService.UpdateAsync(dto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the ticket type.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a ticket type
        /// </summary>
        /// <param name="id">Ticket type ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid ticket type ID." });

                await _ticketTypeService.DeleteAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the ticket type.", error = ex.Message });
            }
        }

        /// <summary>
        /// Toggle the active status of a ticket type
        /// </summary>
        /// <param name="id">Ticket type ID</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid ticket type ID." });

                await _ticketTypeService.ToggleActiveStatusAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while toggling the ticket type status.", error = ex.Message });
            }
        }
    }
}
