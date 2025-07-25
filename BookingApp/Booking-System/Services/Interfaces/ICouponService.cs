using Booking_System.DTOs;

namespace Booking_System.Business_Logic.Interfaces
{
    public interface ICouponService
    {
        Task<IEnumerable<CouponDto>> GetAllAsync();
        Task<IEnumerable<CouponDto>> GetActiveCouponsAsync();
        Task<CouponDto> GetByIdAsync(int id);
        Task<CouponDto> GetByCodeAsync(string code);
        Task<CouponDto> CreateAsync(CreateCouponDto dto);
        Task UpdateAsync(UpdateCouponDto dto);
        Task DeleteAsync(int id);
        Task ToggleActiveStatusAsync(int id);
        Task IncrementUsageAsync(int couponId);
        Task<decimal> CalculateDiscountAsync(string code, decimal orderValue);
        Task<CouponValidationResult> ValidateCouponCodeAsync(string couponCode, string userId, decimal orderValue);
        Task ApplyCouponAsync(string couponCode, string userId);
        Task<bool> HasUserUsedCouponAsync(string userId, int couponId);
        Task<IEnumerable<UserCouponDto>> GetUserCouponsAsync(string userId);
    }
}
