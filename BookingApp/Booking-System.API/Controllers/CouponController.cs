using Booking_System.Application.Interfaces;
using Booking_System.Application.Common;
using Booking_System.Application.DTOs.Coupon;
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
        public async Task<ActionResult<ApiResponse<IEnumerable<CouponDto>>>> GetAllCoupons()
        {
            try
            {
                var coupons = await _couponService.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<CouponDto>>.Success(coupons));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CouponDto>>.Failure("Internal server error"));
            }
        }

        /// <summary>
        /// Get all active coupons
        /// </summary>
        [HttpGet("active")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<CouponDto>>>> GetActiveCoupons()
        {
            try
            {
                var coupons = await _couponService.GetActiveCouponsAsync();
                return Ok(ApiResponse<IEnumerable<CouponDto>>.Success(coupons));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CouponDto>>.Failure("Internal server error"));
            }
        }

        /// <summary>
        /// Get coupon by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CouponDto>>> GetCouponById(int id)
        {
            try
            {
                var coupon = await _couponService.GetByIdAsync(id);
                if (coupon == null)
                    return NotFound(ApiResponse<CouponDto>.Failure("Coupon not found"));

                return Ok(ApiResponse<CouponDto>.Success(coupon));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CouponDto>.Failure("Internal server error"));
            }
        }

        /// <summary>
        /// Get coupon by code
        /// </summary>
        [HttpGet("code/{code}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CouponDto>>> GetCouponByCode(string code)
        {
            try
            {
                var coupon = await _couponService.GetByCodeAsync(code);
                if (coupon == null)
                    return NotFound(ApiResponse<CouponDto>.Failure("Coupon not found"));

                return Ok(ApiResponse<CouponDto>.Success(coupon));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CouponDto>.Failure("Internal server error"));
            }
        }

        /// <summary>
        /// Create a new coupon (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CouponDto>>> CreateCoupon([FromBody] CreateCouponDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<CouponDto>.Failure("Invalid data"));

                var coupon = await _couponService.CreateAsync(dto);
                return Ok(ApiResponse<CouponDto>.Success(coupon, "Coupon created successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<CouponDto>.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CouponDto>.Failure("Internal server error"));
            }
        }

        /// <summary>
        /// Update an existing coupon (Admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> UpdateCoupon(int id, [FromBody] UpdateCouponDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Failure("Invalid data"));

                if (id != dto.CouponId)
                    return BadRequest(ApiResponse.Failure("ID mismatch"));

                await _couponService.UpdateAsync(dto);
                return Ok(ApiResponse.Success("Coupon updated successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("Internal server error"));
            }
        }

        /// <summary>
        /// Delete a coupon (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> DeleteCoupon(int id)
        {
            try
            {
                await _couponService.DeleteAsync(id);
                return Ok(ApiResponse.Success("Coupon deleted successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("Internal server error"));
            }
        }

        /// <summary>
        /// Toggle coupon active status (Admin only)
        /// </summary>
        [HttpPatch("{id:int}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse>> ToggleCouponStatus(int id)
        {
            try
            {
                await _couponService.ToggleActiveStatusAsync(id);
                return Ok(ApiResponse.Success("Coupon status toggled successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("Internal server error"));
            }
        }

        #endregion

        #region Coupon Validation & Usage

        /// <summary>
        /// Validate a coupon code for a specific order
        /// </summary>
        [HttpPost("validate")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CouponValidationResult>>> ValidateCoupon([FromBody] ValidateCouponDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<CouponValidationResult>.Failure("Invalid data"));

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<CouponValidationResult>.Failure("User not authenticated"));

                var result = await _couponService.ValidateCouponCodeAsync(request.CouponCode, userId, request.OrderValue);

                if (result.IsValid)
                    return Ok(ApiResponse<CouponValidationResult>.Success(result));
                else
                    return BadRequest(ApiResponse<CouponValidationResult>.Failure(result.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CouponValidationResult>.Failure("Internal server error"));
            }
        }
        /// <summary>
        /// Check if user has used a specific coupon
        /// </summary>
        [HttpGet("usage-check/{couponId:int}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> CheckCouponUsage(int couponId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<object>.Failure("User not authenticated"));

                var hasUsed = await _couponService.HasUserUsedCouponAsync(userId, couponId);
                return Ok(ApiResponse<object>.Success(new { hasUsed = hasUsed }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure("Internal server error"));
            }
        }

        #endregion

        #region User Coupon History

        /// <summary>
        /// Get current user's coupon usage history
        /// </summary>
        [HttpGet("my-coupons")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserCouponDto>>>> GetMyCoupons()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<IEnumerable<UserCouponDto>>.Failure("User not authenticated"));

                var userCoupons = await _couponService.GetUserCouponsAsync(userId);
                return Ok(ApiResponse<IEnumerable<UserCouponDto>>.Success(userCoupons));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<UserCouponDto>>.Failure("Internal server error"));
            }
        }

        /// <summary>
        /// Get user's coupon usage history by user ID (Admin only)
        /// </summary>
        [HttpGet("user/{userId}/coupons")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserCouponDto>>>> GetUserCoupons(string userId)
        {
            try
            {
                var userCoupons = await _couponService.GetUserCouponsAsync(userId);
                return Ok(ApiResponse<IEnumerable<UserCouponDto>>.Success(userCoupons));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<UserCouponDto>>.Failure("Internal server error"));
            }
        }

        #endregion
    }
}


