using Booking_System.Domain.Entities;

namespace Booking_System.Application.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByUserIdAsync(string userId);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task DeleteByUserIdAsync(string userId);
        Task DeleteByTokenAsync(string token);
    }
}


