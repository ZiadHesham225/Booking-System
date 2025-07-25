using Booking_System.Data;
using Booking_System.Data_Access.Interfaces;
using Booking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Data_Access.Repositories
{
    public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
    {
        public CouponRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Coupon> GetByCodeAsync(string code)
        {
            return await dbSet.FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            return await dbSet
                .Where(c => c.IsActive &&
                           (c.ExpiryDate == null || c.ExpiryDate > DateTime.UtcNow) &&
                           (c.UsageLimit == null || c.TimesUsed < c.UsageLimit))
                .ToListAsync();
        }

        public async Task<bool> IsValidCouponAsync(string code, decimal orderValue)
        {
            var coupon = await GetByCodeAsync(code);
            if (coupon == null || !coupon.IsActive)
                return false;

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.UtcNow)
                return false;

            if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit)
                return false;

            if (coupon.MinOrderValue.HasValue && orderValue < coupon.MinOrderValue)
                return false;

            return true;
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
