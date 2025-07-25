using Booking_System.Models;

namespace Booking_System.Data_Access.Interfaces
{
    public interface ICouponRepository : IGenericRepository<Coupon>
    {
        Task<Coupon> GetByCodeAsync(string code);
        Task<IEnumerable<Coupon>> GetActiveCouponsAsync();
        Task<bool> IsValidCouponAsync(string code, decimal orderValue);
        Task IncrementUsageAsync(int couponId);
    }
}
