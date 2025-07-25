using Booking_System.DTOs;

namespace Booking_System.Business_Logic.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task RegisterUserAsync(RegisterDto registerDto);
        Task<TokenResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);
        Task RevokeTokenAsync(string userId);
        Task ForgotPasswordAsync(ForgotPasswordRequestDto model);
        string GeneratePasswordResetLink(string frontendResetPasswordUrlBase, string token, string userEmail);
        Task ResetPasswordAsync(ResetPasswordRequestDto model);
    }
}
