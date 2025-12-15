using Booking_System.Domain.Entities;

namespace Booking_System.Application.Interfaces
{
    public interface IUserCouponRepository : IGenericRepository<UserCoupon>
    {
        Task<IEnumerable<UserCoupon>> GetUserCouponsAsync(string userId);
        Task<bool> HasUserUsedCouponAsync(string userId, int couponId);
    }
}


