using Booking_System.Domain.Entities;
using Booking_System.Infrastructure.Data;
using Booking_System.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Booking_System.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Unit tests for UserCouponRepository using EF Core InMemory database.
    /// Tests user coupon tracking including usage verification.
    /// </summary>
    public class UserCouponRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserCouponRepository _repository;

        public UserCouponRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserCouponRepository(_context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Create test users
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user-1", UserName = "testuser1", Email = "test1@example.com", FirstName = "Test1", LastName = "User" },
                new ApplicationUser { Id = "user-2", UserName = "testuser2", Email = "test2@example.com", FirstName = "Test2", LastName = "User" }
            };
            _context.Users.AddRange(users);

            // Create coupons
            var coupons = new List<Coupon>
            {
                new Coupon { CouponId = 1, Code = "COUPON1", DiscountPercent = 10, IsActive = true },
                new Coupon { CouponId = 2, Code = "COUPON2", DiscountPercent = 20, IsActive = true },
                new Coupon { CouponId = 3, Code = "COUPON3", DiscountPercent = 30, IsActive = true }
            };
            _context.Coupons.AddRange(coupons);

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetUserCouponsAsync Tests

        /// <summary>
        /// Tests that GetUserCouponsAsync returns all coupons used by a specific user.
        /// </summary>
        [Fact]
        public async Task GetUserCouponsAsync_WithExistingCoupons_ShouldReturnUserCoupons()
        {
            // Arrange
            var userCoupons = new List<UserCoupon>
            {
                new UserCoupon { UserId = "user-1", CouponId = 1, UsedDate = DateTime.UtcNow },
                new UserCoupon { UserId = "user-1", CouponId = 2, UsedDate = DateTime.UtcNow }
            };
            await _context.UserCoupons.AddRangeAsync(userCoupons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserCouponsAsync("user-1");

            // Assert
            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Tests that GetUserCouponsAsync returns empty when user has no coupons.
        /// </summary>
        [Fact]
        public async Task GetUserCouponsAsync_WithNoCoupons_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetUserCouponsAsync("user-1");

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that GetUserCouponsAsync only returns coupons for specified user.
        /// </summary>
        [Fact]
        public async Task GetUserCouponsAsync_ShouldOnlyReturnSpecifiedUserCoupons()
        {
            // Arrange
            var userCoupons = new List<UserCoupon>
            {
                new UserCoupon { UserId = "user-1", CouponId = 1, UsedDate = DateTime.UtcNow },
                new UserCoupon { UserId = "user-2", CouponId = 2, UsedDate = DateTime.UtcNow },
                new UserCoupon { UserId = "user-1", CouponId = 3, UsedDate = DateTime.UtcNow }
            };
            await _context.UserCoupons.AddRangeAsync(userCoupons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserCouponsAsync("user-1");

            // Assert
            result.Should().HaveCount(2);
            result.All(uc => uc.UserId == "user-1").Should().BeTrue();
        }

        /// <summary>
        /// Tests that GetUserCouponsAsync includes coupon details.
        /// </summary>
        [Fact]
        public async Task GetUserCouponsAsync_ShouldIncludeCouponDetails()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };
            await _context.UserCoupons.AddAsync(userCoupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserCouponsAsync("user-1");

            // Assert
            var coupon = result.First();
            coupon.Coupon.Should().NotBeNull();
            coupon.Coupon!.Code.Should().Be("COUPON1");
            coupon.Coupon.DiscountPercent.Should().Be(10);
        }

        #endregion

        #region HasUserUsedCouponAsync Tests

        /// <summary>
        /// Tests that HasUserUsedCouponAsync returns true when user has used the coupon.
        /// </summary>
        [Fact]
        public async Task HasUserUsedCouponAsync_WhenUserHasUsedCoupon_ShouldReturnTrue()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };
            await _context.UserCoupons.AddAsync(userCoupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasUserUsedCouponAsync("user-1", 1);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that HasUserUsedCouponAsync returns false when user has not used the coupon.
        /// </summary>
        [Fact]
        public async Task HasUserUsedCouponAsync_WhenUserHasNotUsedCoupon_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.HasUserUsedCouponAsync("user-1", 1);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that HasUserUsedCouponAsync returns false for different user.
        /// </summary>
        [Fact]
        public async Task HasUserUsedCouponAsync_ForDifferentUser_ShouldReturnFalse()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };
            await _context.UserCoupons.AddAsync(userCoupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasUserUsedCouponAsync("user-2", 1);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that HasUserUsedCouponAsync returns false for different coupon.
        /// </summary>
        [Fact]
        public async Task HasUserUsedCouponAsync_ForDifferentCoupon_ShouldReturnFalse()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };
            await _context.UserCoupons.AddAsync(userCoupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasUserUsedCouponAsync("user-1", 2);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a user can only use a coupon once.
        /// </summary>
        [Fact]
        public async Task HasUserUsedCouponAsync_PreventsMultipleUsageOfSameCoupon()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };
            await _context.UserCoupons.AddAsync(userCoupon);
            await _context.SaveChangesAsync();

            // Act - Check if user can use the same coupon again
            var canUseAgain = !await _repository.HasUserUsedCouponAsync("user-1", 1);

            // Assert
            canUseAgain.Should().BeFalse(); // User should not be able to use same coupon twice
        }

        #endregion

        #region CRUD Operations Tests

        /// <summary>
        /// Tests creating a new user coupon record.
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithValidUserCoupon_ShouldCreateRecord()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };

            // Act
            var result = await _repository.CreateAsync(userCoupon);
            await _context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            _context.UserCoupons.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests getting all user coupon records.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_WithMultipleRecords_ShouldReturnAll()
        {
            // Arrange
            var userCoupons = new List<UserCoupon>
            {
                new UserCoupon { UserId = "user-1", CouponId = 1, UsedDate = DateTime.UtcNow },
                new UserCoupon { UserId = "user-1", CouponId = 2, UsedDate = DateTime.UtcNow },
                new UserCoupon { UserId = "user-2", CouponId = 1, UsedDate = DateTime.UtcNow }
            };
            await _context.UserCoupons.AddRangeAsync(userCoupons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        /// <summary>
        /// Tests getting user coupon by ID.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnUserCoupon()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };
            await _context.UserCoupons.AddAsync(userCoupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(userCoupon.Id);

            // Assert
            result.Should().NotBeNull();
            ((UserCoupon)result!).UserId.Should().Be("user-1");
        }

        /// <summary>
        /// Tests deleting a user coupon record.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_WithExistingRecord_ShouldRemoveRecord()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };
            await _context.UserCoupons.AddAsync(userCoupon);
            await _context.SaveChangesAsync();
            var recordId = userCoupon.Id;

            // Act
            await _repository.DeleteAsync(recordId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedRecord = await _context.UserCoupons.FindAsync(recordId);
            deletedRecord.Should().BeNull();
        }

        #endregion

        #region Edge Cases Tests

        /// <summary>
        /// Tests getting user coupons for non-existing user.
        /// </summary>
        [Fact]
        public async Task GetUserCouponsAsync_WithNonExistingUser_ShouldReturnEmpty()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };
            await _context.UserCoupons.AddAsync(userCoupon);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserCouponsAsync("non-existing-user");

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Tests checking coupon usage for non-existing coupon.
        /// </summary>
        [Fact]
        public async Task HasUserUsedCouponAsync_WithNonExistingCoupon_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.HasUserUsedCouponAsync("user-1", 999);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that user can use multiple different coupons.
        /// </summary>
        [Fact]
        public async Task UserCanUseMultipleDifferentCoupons()
        {
            // Arrange
            var userCoupons = new List<UserCoupon>
            {
                new UserCoupon { UserId = "user-1", CouponId = 1, UsedDate = DateTime.UtcNow },
                new UserCoupon { UserId = "user-1", CouponId = 2, UsedDate = DateTime.UtcNow }
            };
            await _context.UserCoupons.AddRangeAsync(userCoupons);
            await _context.SaveChangesAsync();

            // Act - User should still be able to use coupon 3
            var canUseCoupon3 = !await _repository.HasUserUsedCouponAsync("user-1", 3);

            // Assert
            canUseCoupon3.Should().BeTrue();
        }

        /// <summary>
        /// Tests that same coupon can be used by different users.
        /// </summary>
        [Fact]
        public async Task SameCouponCanBeUsedByDifferentUsers()
        {
            // Arrange
            var userCoupon = new UserCoupon
            {
                UserId = "user-1",
                CouponId = 1,
                UsedDate = DateTime.UtcNow
            };
            await _context.UserCoupons.AddAsync(userCoupon);
            await _context.SaveChangesAsync();

            // Act - User 2 should be able to use the same coupon
            var canUser2UseCoupon1 = !await _repository.HasUserUsedCouponAsync("user-2", 1);

            // Assert
            canUser2UseCoupon1.Should().BeTrue();
        }

        #endregion
    }
}
