using Booking_System.Application.DTOs.Auth;
using System.Security.Claims;

namespace Booking_System.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<TokenResponseDto> RefreshAccessTokenAsync(string accessToken, string refreshToken);
        Task SaveRefreshTokenAsync(string userId, string token, DateTime expiryTime);
        Task RevokeRefreshTokenAsync(string userId);
    }
}



