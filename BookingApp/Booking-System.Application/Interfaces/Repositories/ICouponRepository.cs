using Booking_System.Domain.Entities;

namespace Booking_System.Application.Interfaces
{
    public interface ICouponRepository : IGenericRepository<Coupon>
    {
        Task<Coupon?> GetByCodeAsync(string code);
        Task<IEnumerable<Coupon>> GetActiveCouponsAsync();
        Task<bool> IsValidCouponAsync(string code, decimal orderValue);
        Task IncrementUsageAsync(int couponId);
    }
}


