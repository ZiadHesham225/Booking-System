using Booking_System.Data;
using Booking_System.Data_Access.Interfaces;
using Booking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Data_Access.Repositories
{
    public class UserCouponRepository : GenericRepository<UserCoupon>, IUserCouponRepository
    {
        public UserCouponRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserCoupon>> GetUserCouponsAsync(string userId)
        {
            return await dbSet
                .Include(uc => uc.Coupon)
                .Where(uc => uc.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> HasUserUsedCouponAsync(string userId, int couponId)
        {
            return await dbSet
                .AnyAsync(uc => uc.UserId == userId && uc.CouponId == couponId);
        }
    }
}
