using Booking_System.Application.Interfaces;
using Booking_System.Application.Common;
using Booking_System.Application.DTOs.Auth;
using Booking_System.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Booking_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Failure("Invalid data"));

            try
            {
                await _authService.RegisterUserAsync(registerDto);
                return Ok(ApiResponse.Success("User registered successfully"));
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Authentication error during registration");
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return StatusCode(500, ApiResponse.Failure("An error occurred during registration"));
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AuthResponseDto>.Failure("Invalid data"));

            try
            {
                var authResponse = await _authService.LoginAsync(loginDto);
                return Ok(ApiResponse<AuthResponseDto>.Success(authResponse, "Login successful"));
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Authentication error during login");
                return BadRequest(ApiResponse<AuthResponseDto>.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user");
                return StatusCode(500, ApiResponse<AuthResponseDto>.Failure("An error occurred during login"));
            }
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<TokenResponseDto>.Failure("Invalid data"));

            try
            {
                var authResponse = await _authService.RefreshTokenAsync(
                    refreshTokenDto.AccessToken,
                    refreshTokenDto.RefreshToken);

                return Ok(ApiResponse<TokenResponseDto>.Success(authResponse));
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Invalid token during refresh");
                return Unauthorized(ApiResponse<TokenResponseDto>.Failure("Invalid or expired token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, ApiResponse<TokenResponseDto>.Failure("An error occurred while refreshing token"));
            }
        }
        [HttpPost("revoke")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> RevokeToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse.Failure("User not authenticated"));

                await _authService.RevokeTokenAsync(userId);
                return Ok(ApiResponse.Success("Token revoked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.Failure("An error occurred while revoking token"));
            }
        }
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            try
            {
                await _authService.ForgotPasswordAsync(model);
                return Ok(ApiResponse.Success("We've sent you a password reset email!"));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("An unexpected error occurred."));
            }
        }
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse>> ResetPassword(ResetPasswordRequestDto model)
        {
            try
            {
                await _authService.ResetPasswordAsync(model);
                return Ok(ApiResponse.Success("Password reset successfully!"));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure("An unexpected error occurred."));
            }
        }
    }
}


