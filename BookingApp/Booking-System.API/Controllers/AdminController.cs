using Booking_System.Application.Interfaces;
using Booking_System.Application.Common;
using Booking_System.Application.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booking_System.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public AdminController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetDashboard()
        {
            try
            {
                var result = await _dashboardService.GetDashboardDataAsync();
                return Ok(ApiResponse<AdminDashboardDto>.Success(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AdminDashboardDto>.Failure("Error retrieving dashboard data"));
            }
        }
    }
}


