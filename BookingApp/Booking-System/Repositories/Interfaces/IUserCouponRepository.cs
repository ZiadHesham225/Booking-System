using Booking_System.Models;

namespace Booking_System.Data_Access.Interfaces
{
    public interface IUserCouponRepository : IGenericRepository<UserCoupon>
    {
        Task<IEnumerable<UserCoupon>> GetUserCouponsAsync(string userId);
        Task<bool> HasUserUsedCouponAsync(string userId, int couponId);
    }
}
