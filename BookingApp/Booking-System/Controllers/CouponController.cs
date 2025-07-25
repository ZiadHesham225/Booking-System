using Booking_System.Business_Logic.Interfaces;
using Booking_System.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booking_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        #region Coupon Management

        /// <summary>
        /// Get all coupons (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCoupons()
        {
            try
            {
                var coupons = await _couponService.GetAllAsync();
                return Ok(new { success = true, data = coupons });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active coupons
        /// </summary>
        [HttpGet("active")]
        [Authorize]
        public async Task<IActionResult> GetActiveCoupons()
        {
            try
            {
                var coupons = await _couponService.GetActiveCouponsAsync();
                return Ok(new { success = true, data = coupons });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get coupon by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetCouponById(int id)
        {
            try
            {
                var coupon = await _couponService.GetByIdAsync(id);
                if (coupon == null)
                    return NotFound(new { success = false, message = "Coupon not found" });

                return Ok(new { success = true, data = coupon });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get coupon by code
        /// </summary>
        [HttpGet("code/{code}")]
        [Authorize]
        public async Task<IActionResult> GetCouponByCode(string code)
        {
            try
            {
                var coupon = await _couponService.GetByCodeAsync(code);
                if (coupon == null)
                    return NotFound(new { success = false, message = "Coupon not found" });

                return Ok(new { success = true, data = coupon });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new coupon (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var coupon = await _couponService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetCouponById), new { id = coupon.CouponId },
                    new { success = true, data = coupon });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing coupon (Admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] UpdateCouponDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                if (id != dto.CouponId)
                    return BadRequest(new { success = false, message = "ID mismatch" });

                await _couponService.UpdateAsync(dto);
                return Ok(new { success = true, message = "Coupon updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a coupon (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            try
            {
                await _couponService.DeleteAsync(id);
                return Ok(new { success = true, message = "Coupon deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Toggle coupon active status (Admin only)
        /// </summary>
        [HttpPatch("{id:int}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleCouponStatus(int id)
        {
            try
            {
                await _couponService.ToggleActiveStatusAsync(id);
                return Ok(new { success = true, message = "Coupon status toggled successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        #endregion

        #region Coupon Validation & Usage

        /// <summary>
        /// Validate a coupon code for a specific order
        /// </summary>
        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var result = await _couponService.ValidateCouponCodeAsync(request.CouponCode, userId, request.OrderValue);

                if (result.IsValid)
                    return Ok(new { success = true, data = result });
                else
                    return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }
        /// <summary>
        /// Check if user has used a specific coupon
        /// </summary>
        [HttpGet("usage-check/{couponId:int}")]
        [Authorize]
        public async Task<IActionResult> CheckCouponUsage(int couponId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var hasUsed = await _couponService.HasUserUsedCouponAsync(userId, couponId);
                return Ok(new { success = true, data = new { hasUsed = hasUsed } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        #endregion

        #region User Coupon History

        /// <summary>
        /// Get current user's coupon usage history
        /// </summary>
        [HttpGet("my-coupons")]
        [Authorize]
        public async Task<IActionResult> GetMyCoupons()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var userCoupons = await _couponService.GetUserCouponsAsync(userId);
                return Ok(new { success = true, data = userCoupons });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user's coupon usage history by user ID (Admin only)
        /// </summary>
        [HttpGet("user/{userId}/coupons")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserCoupons(string userId)
        {
            try
            {
                var userCoupons = await _couponService.GetUserCouponsAsync(userId);
                return Ok(new { success = true, data = userCoupons });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        #endregion
    }
}
