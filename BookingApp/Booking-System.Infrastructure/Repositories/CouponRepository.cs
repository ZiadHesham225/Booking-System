using Booking_System.Infrastructure.Data;
using Booking_System.Application.Interfaces;
using Booking_System.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Infrastructure.Repositories
{
    public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
    {
        public CouponRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            return await dbSet
                .AsNoTracking()
                .Where(c => c.IsActive &&
                           (c.ExpiryDate == null || c.ExpiryDate > DateTime.UtcNow) &&
                           (c.UsageLimit == null || c.TimesUsed < c.UsageLimit))
                .ToListAsync();
        }

        public async Task<bool> IsValidCouponAsync(string code, decimal orderValue)
        {
            return await dbSet
                .AsNoTracking()
                .AnyAsync(c => c.Code == code &&
                              c.IsActive &&
                              (!c.ExpiryDate.HasValue || c.ExpiryDate > DateTime.UtcNow) &&
                              (!c.UsageLimit.HasValue || c.TimesUsed < c.UsageLimit) &&
                              (!c.MinOrderValue.HasValue || orderValue >= c.MinOrderValue));
        }

        public async Task IncrementUsageAsync(int couponId)
        {
            var coupon = await dbSet.FindAsync(couponId);
            if (coupon != null)
            {
                coupon.TimesUsed++;
            }
        }
    }
}




