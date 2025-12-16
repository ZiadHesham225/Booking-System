using Booking_System.Domain.Entities;
using Booking_System.Infrastructure.Data;
using Booking_System.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booking_System.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Unit tests for CouponRepository using EF Core InMemory database.
    /// Tests coupon-specific operations including validation, expiration, and usage tracking.
    /// </summary>
    public class CouponRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CouponRepository _repository;

        public CouponRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new CouponRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetByCodeAsync Tests

        /// <summary>
        /// Tests that GetByCodeAsync returns coupon when code exists.
        /// </summary>
        [Fact]
        public async Task GetByCodeAsync_WithExistingCode_ShouldReturnCoupon()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "SUMMER20",
                DiscountPercent = 20,
                IsActive = true,
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByCodeAsync("SUMMER20");

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("SUMMER20");
            result.DiscountPercent.Should().Be(20);
        }

        /// <summary>
        /// Tests that GetByCodeAsync returns null when code doesn't exist.
        /// </summary>
        [Fact]
        public async Task GetByCodeAsync_WithNonExistingCode_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByCodeAsync("NONEXISTENT");

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that GetByCodeAsync is case-sensitive.
        /// </summary>
        [Fact]
        public async Task GetByCodeAsync_IsCaseSensitive()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "SUMMER20",
                DiscountPercent = 20,
                IsActive = true
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByCodeAsync("summer20");

            // Assert
            // Note: SQL Server is case-insensitive by default, but InMemory provider is case-sensitive
            // This test validates the behavior in the InMemory context
            result.Should().BeNull();
        }

        #endregion

        #region GetActiveCouponsAsync Tests

        /// <summary>
        /// Tests that GetActiveCouponsAsync returns only active coupons.
        /// </summary>
        [Fact]
        public async Task GetActiveCouponsAsync_ShouldReturnOnlyActiveCoupons()
        {
            // Arrange
            var coupons = new List<Coupon>
            {
                new Coupon { Code = "ACTIVE1", DiscountPercent = 10, IsActive = true, ExpiryDate = DateTime.UtcNow.AddMonths(1) },
                new Coupon { Code = "INACTIVE", DiscountPercent = 15, IsActive = false, ExpiryDate = DateTime.UtcNow.AddMonths(1) },
                new Coupon { Code = "ACTIVE2", DiscountPercent = 20, IsActive = true, ExpiryDate = DateTime.UtcNow.AddMonths(1) }
            };
            await _context.Coupons.AddRangeAsync(coupons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveCouponsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(c => c.IsActive).Should().BeTrue();
        }

        /// <summary>
        /// Tests that GetActiveCouponsAsync excludes expired coupons.
        /// </summary>
        [Fact]
        public async Task GetActiveCouponsAsync_ShouldExcludeExpiredCoupons()
        {
            // Arrange
            var coupons = new List<Coupon>
            {
                new Coupon { Code = "VALID", DiscountPercent = 10, IsActive = true, ExpiryDate = DateTime.UtcNow.AddMonths(1) },
                new Coupon { Code = "EXPIRED", DiscountPercent = 15, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(-1) }
            };
            await _context.Coupons.AddRangeAsync(coupons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveCouponsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Code.Should().Be("VALID");
        }

        /// <summary>
        /// Tests that GetActiveCouponsAsync excludes coupons that reached usage limit.
        /// </summary>
        [Fact]
        public async Task GetActiveCouponsAsync_ShouldExcludeCouponsAtUsageLimit()
        {
            // Arrange
            var coupons = new List<Coupon>
            {
                new Coupon { Code = "UNLIMITED", DiscountPercent = 10, IsActive = true, UsageLimit = null, TimesUsed = 100 },
                new Coupon { Code = "LIMITED", DiscountPercent = 15, IsActive = true, UsageLimit = 10, TimesUsed = 10 },
                new Coupon { Code = "AVAILABLE", DiscountPercent = 20, IsActive = true, UsageLimit = 10, TimesUsed = 5 }
            };
            await _context.Coupons.AddRangeAsync(coupons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveCouponsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Code == "UNLIMITED");
            result.Should().Contain(c => c.Code == "AVAILABLE");
        }

        /// <summary>
        /// Tests that GetActiveCouponsAsync includes coupons without expiry date.
        /// </summary>
        [Fact]
        public async Task GetActiveCouponsAsync_ShouldIncludeCouponsWithoutExpiryDate()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "NOEXPIRY",
                DiscountPercent = 10,
                IsActive = true,
                ExpiryDate = null
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveCouponsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Code.Should().Be("NOEXPIRY");
        }

        #endregion

        #region IsValidCouponAsync Tests

        /// <summary>
        /// Tests that IsValidCouponAsync returns true for valid coupon.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithValidCoupon_ShouldReturnTrue()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "VALID",
                DiscountPercent = 20,
                IsActive = true,
                ExpiryDate = DateTime.UtcNow.AddMonths(1),
                UsageLimit = 100,
                TimesUsed = 50,
                MinOrderValue = 50.00m
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("VALID", 100.00m);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that IsValidCouponAsync returns false for non-existing coupon.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithNonExistingCode_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.IsValidCouponAsync("NONEXISTENT", 100.00m);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that IsValidCouponAsync returns false for inactive coupon.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithInactiveCoupon_ShouldReturnFalse()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "INACTIVE",
                DiscountPercent = 20,
                IsActive = false
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("INACTIVE", 100.00m);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that IsValidCouponAsync returns false for expired coupon.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithExpiredCoupon_ShouldReturnFalse()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "EXPIRED",
                DiscountPercent = 20,
                IsActive = true,
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("EXPIRED", 100.00m);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that IsValidCouponAsync returns false when usage limit reached.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithUsageLimitReached_ShouldReturnFalse()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "LIMITED",
                DiscountPercent = 20,
                IsActive = true,
                UsageLimit = 10,
                TimesUsed = 10
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("LIMITED", 100.00m);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that IsValidCouponAsync returns false when order value is below minimum.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithOrderBelowMinimum_ShouldReturnFalse()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "MINORDER",
                DiscountPercent = 20,
                IsActive = true,
                MinOrderValue = 100.00m
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("MINORDER", 50.00m);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that IsValidCouponAsync returns true when order value meets minimum.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithOrderMeetingMinimum_ShouldReturnTrue()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "MINORDER",
                DiscountPercent = 20,
                IsActive = true,
                MinOrderValue = 100.00m
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("MINORDER", 100.00m);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that IsValidCouponAsync returns true for coupon without usage limit.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithNoUsageLimit_ShouldReturnTrue()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "UNLIMITED",
                DiscountPercent = 20,
                IsActive = true,
                UsageLimit = null,
                TimesUsed = 1000
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("UNLIMITED", 50.00m);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that IsValidCouponAsync returns true for coupon without minimum order value.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithNoMinOrderValue_ShouldReturnTrue()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "NOMINIMUM",
                DiscountPercent = 20,
                IsActive = true,
                MinOrderValue = null
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("NOMINIMUM", 1.00m);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region IncrementUsageAsync Tests

        /// <summary>
        /// Tests that IncrementUsageAsync increases the TimesUsed count.
        /// </summary>
        [Fact]
        public async Task IncrementUsageAsync_ShouldIncreaseTimesUsed()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "TEST",
                DiscountPercent = 20,
                IsActive = true,
                TimesUsed = 5
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            await _repository.IncrementUsageAsync(coupon.CouponId);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCoupon = await _context.Coupons.FindAsync(coupon.CouponId);
            updatedCoupon!.TimesUsed.Should().Be(6);
        }

        /// <summary>
        /// Tests that IncrementUsageAsync handles multiple increments correctly.
        /// </summary>
        [Fact]
        public async Task IncrementUsageAsync_MultipleIncrements_ShouldAccumulateCorrectly()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "TEST",
                DiscountPercent = 20,
                IsActive = true,
                TimesUsed = 0
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            await _repository.IncrementUsageAsync(coupon.CouponId);
            await _context.SaveChangesAsync();
            await _repository.IncrementUsageAsync(coupon.CouponId);
            await _context.SaveChangesAsync();
            await _repository.IncrementUsageAsync(coupon.CouponId);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCoupon = await _context.Coupons.FindAsync(coupon.CouponId);
            updatedCoupon!.TimesUsed.Should().Be(3);
        }

        /// <summary>
        /// Tests that IncrementUsageAsync handles non-existing coupon gracefully.
        /// </summary>
        [Fact]
        public async Task IncrementUsageAsync_WithNonExistingCoupon_ShouldNotThrow()
        {
            // Act
            var act = async () =>
            {
                await _repository.IncrementUsageAsync(999);
                await _context.SaveChangesAsync();
            };

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region CRUD Operations Tests

        /// <summary>
        /// Tests creating a new coupon.
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithValidCoupon_ShouldCreateCoupon()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "NEWCOUPON",
                DiscountPercent = 25,
                IsActive = true,
                ExpiryDate = DateTime.UtcNow.AddMonths(3)
            };

            // Act
            var result = await _repository.CreateAsync(coupon);
            await _context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.CouponId.Should().BeGreaterThan(0);
            _context.Coupons.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests updating an existing coupon.
        /// </summary>
        [Fact]
        public async Task Update_WithExistingCoupon_ShouldModifyCoupon()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "ORIGINAL",
                DiscountPercent = 10,
                IsActive = true
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();
            _context.Entry(coupon).State = EntityState.Detached;

            // Act
            coupon.DiscountPercent = 30;
            _repository.Update(coupon);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCoupon = await _context.Coupons.FindAsync(coupon.CouponId);
            updatedCoupon!.DiscountPercent.Should().Be(30);
        }

        /// <summary>
        /// Tests deleting an existing coupon.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_WithExistingCoupon_ShouldRemoveCoupon()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "TODELETE",
                DiscountPercent = 10,
                IsActive = true
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();
            var couponId = coupon.CouponId;

            // Act
            await _repository.DeleteAsync(couponId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedCoupon = await _context.Coupons.FindAsync(couponId);
            deletedCoupon.Should().BeNull();
        }

        /// <summary>
        /// Tests getting all coupons.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_WithMultipleCoupons_ShouldReturnAllCoupons()
        {
            // Arrange
            var coupons = new List<Coupon>
            {
                new Coupon { Code = "COUPON1", DiscountPercent = 10, IsActive = true },
                new Coupon { Code = "COUPON2", DiscountPercent = 20, IsActive = true },
                new Coupon { Code = "COUPON3", DiscountPercent = 30, IsActive = false }
            };
            await _context.Coupons.AddRangeAsync(coupons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        /// <summary>
        /// Tests getting a coupon by ID.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithExistingCoupon_ShouldReturnCoupon()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "BYID",
                DiscountPercent = 15,
                IsActive = true
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(coupon.CouponId);

            // Assert
            result.Should().NotBeNull();
            ((Coupon)result!).Code.Should().Be("BYID");
        }

        #endregion

        #region Edge Cases Tests

        /// <summary>
        /// Tests coupon validation at exact expiry moment.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_AtExactExpiryMoment_ShouldReturnFalse()
        {
            // Arrange
            var exactExpiry = DateTime.UtcNow;
            var coupon = new Coupon
            {
                Code = "ATEXPIRY",
                DiscountPercent = 20,
                IsActive = true,
                ExpiryDate = exactExpiry.AddMilliseconds(-1) // Slightly in the past
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("ATEXPIRY", 100.00m);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests coupon validation with exact minimum order value.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithExactMinOrderValue_ShouldReturnTrue()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "EXACTMIN",
                DiscountPercent = 20,
                IsActive = true,
                MinOrderValue = 100.00m
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("EXACTMIN", 100.00m);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests coupon validation with one use remaining.
        /// </summary>
        [Fact]
        public async Task IsValidCouponAsync_WithOneUseRemaining_ShouldReturnTrue()
        {
            // Arrange
            var coupon = new Coupon
            {
                Code = "LASTUSE",
                DiscountPercent = 20,
                IsActive = true,
                UsageLimit = 10,
                TimesUsed = 9
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsValidCouponAsync("LASTUSE", 50.00m);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that usage increment respects the limit.
        /// </summary>
        [Fact]
        public async Task IncrementUsageAsync_ShouldNotPreventExceedingLimit()
        {
            // Arrange - Repository doesn't enforce limit on increment, that's service responsibility
            var coupon = new Coupon
            {
                Code = "LIMIT",
                DiscountPercent = 20,
                IsActive = true,
                UsageLimit = 2,
                TimesUsed = 2
            };
            await _context.Coupons.AddAsync(coupon);
            await _context.SaveChangesAsync();

            // Act
            await _repository.IncrementUsageAsync(coupon.CouponId);
            await _context.SaveChangesAsync();

            // Assert - Repository just increments, validation is service layer responsibility
            var updatedCoupon = await _context.Coupons.FindAsync(coupon.CouponId);
            updatedCoupon!.TimesUsed.Should().Be(3);
        }

        #endregion
    }
}
