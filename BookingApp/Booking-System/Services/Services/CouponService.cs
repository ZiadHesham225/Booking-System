using Booking_System.Data;
using Booking_System.DTOs;
using Booking_System.Models;

namespace Booking_System.Business_Logic.Interfaces
{
    public class CouponService : ICouponService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CouponService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Coupon Management
        public async Task<IEnumerable<CouponDto>> GetAllAsync()
        {
            var coupons = await _unitOfWork.Coupons.GetAllAsync();
            return coupons.Select(MapToDto);
        }

        public async Task<IEnumerable<CouponDto>> GetActiveCouponsAsync()
        {
            var coupons = await _unitOfWork.Coupons.GetActiveCouponsAsync();
            return coupons.Select(MapToDto);
        }

        public async Task<CouponDto> GetByIdAsync(int id)
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (coupon == null) return null;

            return MapToDto(coupon);
        }

        public async Task<CouponDto> GetByCodeAsync(string code)
        {
            var coupon = await _unitOfWork.Coupons.GetByCodeAsync(code);
            if (coupon == null) return null;

            return MapToDto(coupon);
        }

        public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
        {
            var existingCoupon = await _unitOfWork.Coupons.GetByCodeAsync(dto.Code);
            if (existingCoupon != null)
                throw new ArgumentException("Coupon with this code already exists.");

            if (dto.DiscountPercent <= 0 || dto.DiscountPercent > 100)
                throw new ArgumentException("Discount percent must be between 1 and 100.");

            var coupon = new Coupon
            {
                Code = dto.Code.ToUpper(),
                DiscountPercent = dto.DiscountPercent,
                MinOrderValue = dto.MinOrderValue,
                ExpiryDate = dto.ExpiryDate,
                UsageLimit = dto.UsageLimit,
                IsActive = dto.IsActive,
                TimesUsed = 0
            };

            await _unitOfWork.Coupons.CreateAsync(coupon);
            await _unitOfWork.CommitAsync();

            return MapToDto(coupon);
        }

        public async Task UpdateAsync(UpdateCouponDto dto)
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(dto.CouponId);
            if (coupon == null)
                throw new ArgumentException("Coupon not found.");

            var existingCoupon = await _unitOfWork.Coupons.GetByCodeAsync(dto.Code);
            if (existingCoupon != null && existingCoupon.CouponId != dto.CouponId)
                throw new ArgumentException("Another coupon with this code already exists.");

            if (dto.DiscountPercent <= 0 || dto.DiscountPercent > 100)
                throw new ArgumentException("Discount percent must be between 1 and 100.");

            coupon.Code = dto.Code.ToUpper();
            coupon.DiscountPercent = dto.DiscountPercent;
            coupon.MinOrderValue = dto.MinOrderValue;
            coupon.ExpiryDate = dto.ExpiryDate;
            coupon.UsageLimit = dto.UsageLimit;
            coupon.IsActive = dto.IsActive;

            _unitOfWork.Coupons.Update(coupon);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (coupon == null)
                throw new ArgumentException("Coupon not found.");

            await _unitOfWork.Coupons.DeleteAsync(id);
            await _unitOfWork.CommitAsync();
        }

        public async Task ToggleActiveStatusAsync(int id)
        {
            var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
            if (coupon == null)
                throw new ArgumentException("Coupon not found.");

            coupon.IsActive = !coupon.IsActive;
            _unitOfWork.Coupons.Update(coupon);
            await _unitOfWork.CommitAsync();
        }
        #endregion

        #region Coupon Usage & Validation
        public async Task<CouponValidationResult> ValidateCouponCodeAsync(string couponCode, string userId, decimal orderValue)
        {
            var coupon = await _unitOfWork.Coupons.GetByCodeAsync(couponCode);

            if (coupon == null)
                return new CouponValidationResult { IsValid = false, Message = "Invalid coupon code" };

            if (!coupon.IsActive)
                return new CouponValidationResult { IsValid = false, Message = "Coupon is not active" };

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.UtcNow)
                return new CouponValidationResult { IsValid = false, Message = "Coupon has expired" };

            if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit)
                return new CouponValidationResult { IsValid = false, Message = "Coupon usage limit reached" };

            if (coupon.MinOrderValue.HasValue && orderValue < coupon.MinOrderValue)
                return new CouponValidationResult { IsValid = false, Message = $"Minimum order value is {coupon.MinOrderValue}" };

            var hasUsed = await _unitOfWork.UserCoupons.HasUserUsedCouponAsync(userId, coupon.CouponId);
            if (hasUsed)
                return new CouponValidationResult { IsValid = false, Message = "You have already used this coupon" };
            var discountAmount = orderValue * (coupon.DiscountPercent / 100);

            return new CouponValidationResult
            {
                IsValid = true,
                DiscountAmount = discountAmount,
                DiscountPercent = coupon.DiscountPercent
            };
        }

        public async Task ApplyCouponAsync(string couponCode, string userId)
        {
            var coupon = await _unitOfWork.Coupons.GetByCodeAsync(couponCode);
            if (coupon == null)
                throw new ArgumentException("Coupon not found.");

            var userCoupon = new UserCoupon
            {
                UserId = userId,
                CouponId = coupon.CouponId,
                UsedDate = DateTime.UtcNow
            };

            await _unitOfWork.UserCoupons.CreateAsync(userCoupon);

            await IncrementUsageAsync(coupon.CouponId);
        }

        public async Task<decimal> CalculateDiscountAsync(string code, decimal orderValue)
        {
            var coupon = await _unitOfWork.Coupons.GetByCodeAsync(code);
            if (coupon == null || !await _unitOfWork.Coupons.IsValidCouponAsync(code, orderValue))
                return 0;

            return orderValue * (coupon.DiscountPercent / 100);
        }

        public async Task IncrementUsageAsync(int couponId)
        {
            await _unitOfWork.Coupons.IncrementUsageAsync(couponId);
            await _unitOfWork.CommitAsync();
        }

        public async Task<bool> HasUserUsedCouponAsync(string userId, int couponId)
        {
            return await _unitOfWork.UserCoupons.HasUserUsedCouponAsync(userId, couponId);
        }
        #endregion

        #region User Coupon History
        public async Task<IEnumerable<UserCouponDto>> GetUserCouponsAsync(string userId)
        {
            var userCoupons = await _unitOfWork.UserCoupons.GetUserCouponsAsync(userId);
            return userCoupons.Select(MapUserCouponToDto);
        }
        #endregion

        #region Private Methods
        private CouponDto MapToDto(Coupon coupon)
        {
            return new CouponDto
            {
                CouponId = coupon.CouponId,
                Code = coupon.Code,
                DiscountPercent = coupon.DiscountPercent,
                MinOrderValue = coupon.MinOrderValue,
                ExpiryDate = coupon.ExpiryDate,
                UsageLimit = coupon.UsageLimit,
                TimesUsed = coupon.TimesUsed,
                IsActive = coupon.IsActive
            };
        }

        private UserCouponDto MapUserCouponToDto(UserCoupon userCoupon)
        {
            return new UserCouponDto
            {
                Id = userCoupon.Id,
                UserId = userCoupon.UserId,
                CouponId = userCoupon.CouponId,
                UsedDate = userCoupon.UsedDate,
                CouponCode = userCoupon.Coupon?.Code,
                DiscountPercent = userCoupon.Coupon?.DiscountPercent ?? 0,
                MinOrderValue = userCoupon.Coupon?.MinOrderValue,
                ExpiryDate = userCoupon.Coupon?.ExpiryDate,
                IsActive = userCoupon.Coupon?.IsActive ?? false
            };
        }
        #endregion
    }

    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
    }
}
