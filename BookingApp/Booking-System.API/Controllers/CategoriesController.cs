using Booking_System.Application.Interfaces;
using Booking_System.Application.Common;
using Booking_System.Application.DTOs.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booking_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
        {
            try
            {
                var categories = await _service.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<CategoryDto>>.Success(categories));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CategoryDto>>.Failure("Error retrieving categories"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Get(int id)
        {
            try
            {
                var category = await _service.GetByIdAsync(id);
                if (category == null) 
                    return NotFound(ApiResponse<CategoryDto>.Failure("Category not found"));
                return Ok(ApiResponse<CategoryDto>.Success(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Failure("Error retrieving category"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ApiResponse<CategoryDto>.Failure("Invalid data"));
            try
            {
                var created = await _service.CreateAsync(dto);
                return Ok(ApiResponse<CategoryDto>.Success(created, "Category created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Failure("Error creating category"));
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> Update([FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ApiResponse.Failure("Invalid data"));
            try
            {
                await _service.UpdateAsync(dto);
                return Ok(ApiResponse.Success("Category updated successfully"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("Error updating category"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(ApiResponse.Success("Category deleted successfully"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("Error deleting category"));
            }
        }
    }
}


