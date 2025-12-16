using Booking_System.Application.DTOs.Coupon;
using Booking_System.Application.Interfaces;
using Booking_System.Application.Services;
using Booking_System.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Booking_System.Application.Tests.Services
{
    /// <summary>
    /// Unit tests for CouponService covering coupon management, validation, and usage tracking.
    /// </summary>
    public class CouponServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICouponRepository> _mockCouponRepository;
        private readonly Mock<IUserCouponRepository> _mockUserCouponRepository;
        private readonly CouponService _sut;

        public CouponServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCouponRepository = new Mock<ICouponRepository>();
            _mockUserCouponRepository = new Mock<IUserCouponRepository>();

            _mockUnitOfWork.Setup(x => x.Coupons).Returns(_mockCouponRepository.Object);
            _mockUnitOfWork.Setup(x => x.UserCoupons).Returns(_mockUserCouponRepository.Object);

            _sut = new CouponService(_mockUnitOfWork.Object);
        }

        #region Coupon Creation Tests

        /// <summary>
        /// Verifies that CreateAsync successfully creates a new coupon.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ValidData_ReturnsCouponDto()
        {
            // Arrange
            var createDto = new CreateCouponDto
            {
                Code = "SUMMER20",
                DiscountPercent = 20m,
                MinOrderValue = 100m,
                ExpiryDate = DateTime.UtcNow.AddMonths(1),
                UsageLimit = 100,
                IsActive = true
            };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(createDto.Code))
                .ReturnsAsync((Coupon?)null);
            _mockCouponRepository.Setup(x => x.CreateAsync(It.IsAny<Coupon>()))
                .ReturnsAsync((Coupon c) => c);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be("SUMMER20");
            result.DiscountPercent.Should().Be(20m);
            _mockCouponRepository.Verify(x => x.CreateAsync(It.IsAny<Coupon>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that CreateAsync throws exception when coupon code already exists.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DuplicateCode_ThrowsArgumentException()
        {
            // Arrange
            var createDto = new CreateCouponDto
            {
                Code = "EXISTING",
                DiscountPercent = 20m
            };

            var existingCoupon = new Coupon { CouponId = 1, Code = "EXISTING" };
            _mockCouponRepository.Setup(x => x.GetByCodeAsync(createDto.Code))
                .ReturnsAsync(existingCoupon);

            // Act
            Func<Task> act = async () => await _sut.CreateAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Coupon with this code already exists.");
        }

        /// <summary>
        /// Verifies that CreateAsync throws exception when discount percent is invalid.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(150)]
        public async Task CreateAsync_InvalidDiscountPercent_ThrowsArgumentException(decimal discountPercent)
        {
            // Arrange
            var createDto = new CreateCouponDto
            {
                Code = "INVALID",
                DiscountPercent = discountPercent
            };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(createDto.Code))
                .ReturnsAsync((Coupon?)null);

            // Act
            Func<Task> act = async () => await _sut.CreateAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Discount percent must be between 1 and 100.");
        }

        /// <summary>
        /// Verifies that CreateAsync converts coupon code to uppercase.
        /// </summary>
        [Fact]
        public async Task CreateAsync_LowercaseCode_ConvertsToUppercase()
        {
            // Arrange
            var createDto = new CreateCouponDto
            {
                Code = "summer20",
                DiscountPercent = 20m
            };

            Coupon? capturedCoupon = null;
            _mockCouponRepository.Setup(x => x.GetByCodeAsync(createDto.Code))
                .ReturnsAsync((Coupon?)null);
            _mockCouponRepository.Setup(x => x.CreateAsync(It.IsAny<Coupon>()))
                .Callback<Coupon>(c => capturedCoupon = c)
                .ReturnsAsync((Coupon c) => c);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateAsync(createDto);

            // Assert
            capturedCoupon.Should().NotBeNull();
            capturedCoupon!.Code.Should().Be("SUMMER20");
        }

        #endregion

        #region Coupon Validation Tests

        /// <summary>
        /// Verifies that ValidateCouponCodeAsync returns valid result for valid coupon.
        /// </summary>
        [Fact]
        public async Task ValidateCouponCodeAsync_ValidCoupon_ReturnsValidResult()
        {
            // Arrange
            var couponCode = "VALID20";
            var userId = "user-123";
            var orderValue = 150m;
            var coupon = new Coupon
            {
                CouponId = 1,
                Code = couponCode,
                DiscountPercent = 20m,
                MinOrderValue = 100m,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                UsageLimit = 100,
                TimesUsed = 10
            };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);
            _mockUserCouponRepository.Setup(x => x.HasUserUsedCouponAsync(userId, coupon.CouponId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ValidateCouponCodeAsync(couponCode, userId, orderValue);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.DiscountAmount.Should().Be(30m); // 20% of 150
            result.DiscountPercent.Should().Be(20m);
        }

        /// <summary>
        /// Verifies that ValidateCouponCodeAsync returns invalid result for non-existent coupon.
        /// </summary>
        [Fact]
        public async Task ValidateCouponCodeAsync_CouponNotFound_ReturnsInvalidResult()
        {
            // Arrange
            var couponCode = "NOTEXIST";
            var userId = "user-123";
            var orderValue = 100m;

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync((Coupon?)null);

            // Act
            var result = await _sut.ValidateCouponCodeAsync(couponCode, userId, orderValue);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Invalid coupon code");
        }

        /// <summary>
        /// Verifies that ValidateCouponCodeAsync returns invalid result for inactive coupon.
        /// </summary>
        [Fact]
        public async Task ValidateCouponCodeAsync_InactiveCoupon_ReturnsInvalidResult()
        {
            // Arrange
            var couponCode = "INACTIVE";
            var userId = "user-123";
            var orderValue = 100m;
            var coupon = new Coupon
            {
                CouponId = 1,
                Code = couponCode,
                DiscountPercent = 20m,
                IsActive = false
            };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);

            // Act
            var result = await _sut.ValidateCouponCodeAsync(couponCode, userId, orderValue);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Coupon is not active");
        }

        /// <summary>
        /// Verifies that ValidateCouponCodeAsync returns invalid result for expired coupon.
        /// </summary>
        [Fact]
        public async Task ValidateCouponCodeAsync_ExpiredCoupon_ReturnsInvalidResult()
        {
            // Arrange
            var couponCode = "EXPIRED";
            var userId = "user-123";
            var orderValue = 100m;
            var coupon = new Coupon
            {
                CouponId = 1,
                Code = couponCode,
                DiscountPercent = 20m,
                IsActive = true,
                ExpiryDate = DateTime.UtcNow.AddDays(-1) // Expired yesterday
            };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);

            // Act
            var result = await _sut.ValidateCouponCodeAsync(couponCode, userId, orderValue);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Coupon has expired");
        }

        /// <summary>
        /// Verifies that ValidateCouponCodeAsync returns invalid result when usage limit is reached.
        /// </summary>
        [Fact]
        public async Task ValidateCouponCodeAsync_UsageLimitReached_ReturnsInvalidResult()
        {
            // Arrange
            var couponCode = "LIMITED";
            var userId = "user-123";
            var orderValue = 100m;
            var coupon = new Coupon
            {
                CouponId = 1,
                Code = couponCode,
                DiscountPercent = 20m,
                IsActive = true,
                UsageLimit = 10,
                TimesUsed = 10 // Limit reached
            };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);

            // Act
            var result = await _sut.ValidateCouponCodeAsync(couponCode, userId, orderValue);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Coupon usage limit reached");
        }

        /// <summary>
        /// Verifies that ValidateCouponCodeAsync returns invalid result when order value is below minimum.
        /// </summary>
        [Fact]
        public async Task ValidateCouponCodeAsync_BelowMinOrderValue_ReturnsInvalidResult()
        {
            // Arrange
            var couponCode = "MINORDER";
            var userId = "user-123";
            var orderValue = 50m; // Below minimum
            var coupon = new Coupon
            {
                CouponId = 1,
                Code = couponCode,
                DiscountPercent = 20m,
                IsActive = true,
                MinOrderValue = 100m
            };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);

            // Act
            var result = await _sut.ValidateCouponCodeAsync(couponCode, userId, orderValue);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Message.Should().Contain("Minimum order value is");
        }

        /// <summary>
        /// Verifies that user cannot use the same coupon twice (single-use enforcement per user).
        /// </summary>
        [Fact]
        public async Task ValidateCouponCodeAsync_UserAlreadyUsedCoupon_ReturnsInvalidResult()
        {
            // Arrange
            var couponCode = "ONEUSE";
            var userId = "user-123";
            var orderValue = 150m;
            var coupon = new Coupon
            {
                CouponId = 1,
                Code = couponCode,
                DiscountPercent = 20m,
                IsActive = true,
                MinOrderValue = 100m
            };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);
            _mockUserCouponRepository.Setup(x => x.HasUserUsedCouponAsync(userId, coupon.CouponId))
                .ReturnsAsync(true); // User has already used this coupon

            // Act
            var result = await _sut.ValidateCouponCodeAsync(couponCode, userId, orderValue);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("You have already used this coupon");
        }

        #endregion

        #region Apply Coupon Tests

        /// <summary>
        /// Verifies that ApplyCouponAsync creates user coupon record and increments usage.
        /// </summary>
        [Fact]
        public async Task ApplyCouponAsync_ValidCoupon_CreatesUserCouponAndIncrementsUsage()
        {
            // Arrange
            var couponCode = "APPLY20";
            var userId = "user-123";
            var coupon = new Coupon { CouponId = 1, Code = couponCode, TimesUsed = 5 };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);
            _mockUserCouponRepository.Setup(x => x.CreateAsync(It.IsAny<UserCoupon>()))
                .ReturnsAsync((UserCoupon uc) => uc);
            _mockCouponRepository.Setup(x => x.IncrementUsageAsync(coupon.CouponId))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.ApplyCouponAsync(couponCode, userId);

            // Assert
            _mockUserCouponRepository.Verify(x => x.CreateAsync(It.Is<UserCoupon>(uc =>
                uc.UserId == userId && uc.CouponId == coupon.CouponId)), Times.Once);
            _mockCouponRepository.Verify(x => x.IncrementUsageAsync(coupon.CouponId), Times.Once);
        }

        /// <summary>
        /// Verifies that ApplyCouponAsync throws exception when coupon is not found.
        /// </summary>
        [Fact]
        public async Task ApplyCouponAsync_CouponNotFound_ThrowsArgumentException()
        {
            // Arrange
            var couponCode = "NOTEXIST";
            var userId = "user-123";

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync((Coupon?)null);

            // Act
            Func<Task> act = async () => await _sut.ApplyCouponAsync(couponCode, userId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Coupon not found.");
        }

        #endregion

        #region Toggle Active Status Tests

        /// <summary>
        /// Verifies that ToggleActiveStatusAsync toggles coupon status from active to inactive.
        /// </summary>
        [Fact]
        public async Task ToggleActiveStatusAsync_ActiveCoupon_BecomesInactive()
        {
            // Arrange
            var couponId = 1;
            var coupon = new Coupon { CouponId = couponId, Code = "TOGGLE", IsActive = true };

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId))
                .ReturnsAsync(coupon);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.ToggleActiveStatusAsync(couponId);

            // Assert
            coupon.IsActive.Should().BeFalse();
            _mockCouponRepository.Verify(x => x.Update(coupon), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that ToggleActiveStatusAsync toggles coupon status from inactive to active.
        /// </summary>
        [Fact]
        public async Task ToggleActiveStatusAsync_InactiveCoupon_BecomesActive()
        {
            // Arrange
            var couponId = 1;
            var coupon = new Coupon { CouponId = couponId, Code = "TOGGLE", IsActive = false };

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId))
                .ReturnsAsync(coupon);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.ToggleActiveStatusAsync(couponId);

            // Assert
            coupon.IsActive.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that ToggleActiveStatusAsync throws exception when coupon is not found.
        /// </summary>
        [Fact]
        public async Task ToggleActiveStatusAsync_CouponNotFound_ThrowsArgumentException()
        {
            // Arrange
            var couponId = 999;

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId))
                .ReturnsAsync((Coupon?)null);

            // Act
            Func<Task> act = async () => await _sut.ToggleActiveStatusAsync(couponId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Coupon not found.");
        }

        #endregion

        #region Delete Coupon Tests

        /// <summary>
        /// Verifies that DeleteAsync successfully deletes a coupon.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ExistingCoupon_DeletesSuccessfully()
        {
            // Arrange
            var couponId = 1;
            var coupon = new Coupon { CouponId = couponId, Code = "DELETE" };

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId))
                .ReturnsAsync(coupon);
            _mockCouponRepository.Setup(x => x.DeleteAsync(couponId))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.DeleteAsync(couponId);

            // Assert
            _mockCouponRepository.Verify(x => x.DeleteAsync(couponId), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that DeleteAsync throws exception when coupon is not found.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_CouponNotFound_ThrowsArgumentException()
        {
            // Arrange
            var couponId = 999;

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId))
                .ReturnsAsync((Coupon?)null);

            // Act
            Func<Task> act = async () => await _sut.DeleteAsync(couponId);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Coupon not found.");
        }

        #endregion

        #region Get Coupons Tests

        /// <summary>
        /// Verifies that GetAllAsync returns all coupons.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_HasCoupons_ReturnsAllCoupons()
        {
            // Arrange
            var coupons = new List<Coupon>
            {
                new Coupon { CouponId = 1, Code = "COUPON1", DiscountPercent = 10 },
                new Coupon { CouponId = 2, Code = "COUPON2", DiscountPercent = 20 }
            };

            _mockCouponRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(coupons);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Verifies that GetActiveCouponsAsync returns only active coupons.
        /// </summary>
        [Fact]
        public async Task GetActiveCouponsAsync_HasActiveCoupons_ReturnsActiveCouponsOnly()
        {
            // Arrange
            var activeCoupons = new List<Coupon>
            {
                new Coupon { CouponId = 1, Code = "ACTIVE1", DiscountPercent = 10, IsActive = true }
            };

            _mockCouponRepository.Setup(x => x.GetActiveCouponsAsync())
                .ReturnsAsync(activeCoupons);

            // Act
            var result = await _sut.GetActiveCouponsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().IsActive.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns coupon for valid ID.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsCoupon()
        {
            // Arrange
            var couponId = 1;
            var coupon = new Coupon { CouponId = couponId, Code = "TEST", DiscountPercent = 15 };

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId))
                .ReturnsAsync(coupon);

            // Act
            var result = await _sut.GetByIdAsync(couponId);

            // Assert
            result.Should().NotBeNull();
            result.CouponId.Should().Be(couponId);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns null when coupon is not found.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var couponId = 999;

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId))
                .ReturnsAsync((Coupon?)null);

            // Act
            var result = await _sut.GetByIdAsync(couponId);

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Verifies that GetByCodeAsync returns coupon for valid code.
        /// </summary>
        [Fact]
        public async Task GetByCodeAsync_ValidCode_ReturnsCoupon()
        {
            // Arrange
            var couponCode = "VALID";
            var coupon = new Coupon { CouponId = 1, Code = couponCode, DiscountPercent = 20 };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);

            // Act
            var result = await _sut.GetByCodeAsync(couponCode);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(couponCode);
        }

        #endregion

        #region Calculate Discount Tests

        /// <summary>
        /// Verifies that CalculateDiscountAsync returns correct discount amount.
        /// </summary>
        [Fact]
        public async Task CalculateDiscountAsync_ValidCoupon_ReturnsCorrectDiscount()
        {
            // Arrange
            var couponCode = "SAVE25";
            var orderValue = 200m;
            var coupon = new Coupon { CouponId = 1, Code = couponCode, DiscountPercent = 25m };

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);
            _mockCouponRepository.Setup(x => x.IsValidCouponAsync(couponCode, orderValue))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.CalculateDiscountAsync(couponCode, orderValue);

            // Assert
            result.Should().Be(50m); // 25% of 200
        }

        /// <summary>
        /// Verifies that CalculateDiscountAsync returns zero for invalid coupon.
        /// </summary>
        [Fact]
        public async Task CalculateDiscountAsync_InvalidCoupon_ReturnsZero()
        {
            // Arrange
            var couponCode = "INVALID";
            var orderValue = 200m;

            _mockCouponRepository.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync((Coupon?)null);

            // Act
            var result = await _sut.CalculateDiscountAsync(couponCode, orderValue);

            // Assert
            result.Should().Be(0m);
        }

        #endregion

        #region User Coupon History Tests

        /// <summary>
        /// Verifies that GetUserCouponsAsync returns user's coupon history.
        /// </summary>
        [Fact]
        public async Task GetUserCouponsAsync_HasHistory_ReturnsUserCoupons()
        {
            // Arrange
            var userId = "user-123";
            var userCoupons = new List<UserCoupon>
            {
                new UserCoupon
                {
                    Id = 1,
                    UserId = userId,
                    CouponId = 1,
                    UsedDate = DateTime.UtcNow.AddDays(-7),
                    Coupon = new Coupon { CouponId = 1, Code = "USED1", DiscountPercent = 10 }
                },
                new UserCoupon
                {
                    Id = 2,
                    UserId = userId,
                    CouponId = 2,
                    UsedDate = DateTime.UtcNow.AddDays(-3),
                    Coupon = new Coupon { CouponId = 2, Code = "USED2", DiscountPercent = 15 }
                }
            };

            _mockUserCouponRepository.Setup(x => x.GetUserCouponsAsync(userId))
                .ReturnsAsync(userCoupons);

            // Act
            var result = await _sut.GetUserCouponsAsync(userId);

            // Assert
            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Verifies that HasUserUsedCouponAsync returns true when user has used the coupon.
        /// </summary>
        [Fact]
        public async Task HasUserUsedCouponAsync_UserHasUsed_ReturnsTrue()
        {
            // Arrange
            var userId = "user-123";
            var couponId = 1;

            _mockUserCouponRepository.Setup(x => x.HasUserUsedCouponAsync(userId, couponId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.HasUserUsedCouponAsync(userId, couponId);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that HasUserUsedCouponAsync returns false when user has not used the coupon.
        /// </summary>
        [Fact]
        public async Task HasUserUsedCouponAsync_UserHasNotUsed_ReturnsFalse()
        {
            // Arrange
            var userId = "user-123";
            var couponId = 1;

            _mockUserCouponRepository.Setup(x => x.HasUserUsedCouponAsync(userId, couponId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.HasUserUsedCouponAsync(userId, couponId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
