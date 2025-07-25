using Booking_System.Models;

namespace Booking_System.Data_Access.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetByUserIdAsync(string userId);
        Task<RefreshToken> GetByTokenAsync(string token);
        Task DeleteByUserIdAsync(string userId);
        Task DeleteByTokenAsync(string token);
    }
}
