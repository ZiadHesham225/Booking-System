using Booking_System.Infrastructure.Data;
using Booking_System.Application.Interfaces;
using Booking_System.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<RefreshToken?> GetByUserIdAsync(string userId)
        {
            return await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.UserId == userId);
        }
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }
        public async Task DeleteByUserIdAsync(string userId)
        {
            var refreshToken = await GetByUserIdAsync(userId);
            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
            }
        }
        public async Task DeleteByTokenAsync(string token)
        {
            var refreshToken = await GetByTokenAsync(token);
            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
            }
        }
    }
    
}




