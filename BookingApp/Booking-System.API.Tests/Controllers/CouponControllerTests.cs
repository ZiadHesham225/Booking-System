using Booking_System.Application.DTOs.Coupon;
using Booking_System.Application.Interfaces;
using Booking_System.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Booking_System.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for CouponController covering coupon management endpoints.
    /// </summary>
    public class CouponControllerTests
    {
        private readonly Mock<ICouponService> _mockCouponService;
        private readonly CouponController _sut;

        public CouponControllerTests()
        {
            _mockCouponService = new Mock<ICouponService>();
            _sut = new CouponController(_mockCouponService.Object);
        }

        #region GetAllCoupons Tests (Admin Only)

        /// <summary>
        /// Verifies that GetAllCoupons returns Ok with all coupons for admin.
        /// </summary>
        [Fact]
        public async Task GetAllCoupons_AdminUser_ReturnsOkWithCoupons()
        {
            // Arrange
            var coupons = new List<CouponDto>
            {
                new CouponDto { CouponId = 1, Code = "SAVE10", DiscountPercent = 10 },
                new CouponDto { CouponId = 2, Code = "SAVE20", DiscountPercent = 20 }
            };

            _mockCouponService.Setup(x => x.GetAllAsync())
                .ReturnsAsync(coupons);

            // Act
            var result = await _sut.GetAllCoupons();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true, data = coupons });
        }

        /// <summary>
        /// Verifies that GetAllCoupons returns 500 when service throws exception.
        /// </summary>
        [Fact]
        public async Task GetAllCoupons_ServiceError_ReturnsInternalServerError()
        {
            // Arrange
            _mockCouponService.Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetAllCoupons();

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        #endregion

        #region GetActiveCoupons Tests

        /// <summary>
        /// Verifies that GetActiveCoupons returns Ok with active coupons only.
        /// </summary>
        [Fact]
        public async Task GetActiveCoupons_HasActiveCoupons_ReturnsOkWithActiveCoupons()
        {
            // Arrange
            var activeCoupons = new List<CouponDto>
            {
                new CouponDto { CouponId = 1, Code = "ACTIVE1", DiscountPercent = 15, IsActive = true }
            };

            _mockCouponService.Setup(x => x.GetActiveCouponsAsync())
                .ReturnsAsync(activeCoupons);

            // Act
            var result = await _sut.GetActiveCoupons();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true, data = activeCoupons });
        }

        #endregion

        #region GetCouponById Tests

        /// <summary>
        /// Verifies that GetCouponById returns Ok with coupon for valid ID.
        /// </summary>
        [Fact]
        public async Task GetCouponById_ValidId_ReturnsOkWithCoupon()
        {
            // Arrange
            var couponId = 1;
            var coupon = new CouponDto { CouponId = couponId, Code = "SAVE10", DiscountPercent = 10 };

            _mockCouponService.Setup(x => x.GetByIdAsync(couponId))
                .ReturnsAsync(coupon);

            // Act
            var result = await _sut.GetCouponById(couponId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true, data = coupon });
        }

        /// <summary>
        /// Verifies that GetCouponById returns NotFound when coupon doesn't exist.
        /// </summary>
        [Fact]
        public async Task GetCouponById_CouponNotFound_ReturnsNotFound()
        {
            // Arrange
            var couponId = 999;

            _mockCouponService.Setup(x => x.GetByIdAsync(couponId))
                .ReturnsAsync((CouponDto?)null);

            // Act
            var result = await _sut.GetCouponById(couponId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { success = false, message = "Coupon not found" });
        }

        #endregion

        #region GetCouponByCode Tests

        /// <summary>
        /// Verifies that GetCouponByCode returns Ok with coupon for valid code.
        /// </summary>
        [Fact]
        public async Task GetCouponByCode_ValidCode_ReturnsOkWithCoupon()
        {
            // Arrange
            var couponCode = "SAVE10";
            var coupon = new CouponDto { CouponId = 1, Code = couponCode, DiscountPercent = 10 };

            _mockCouponService.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync(coupon);

            // Act
            var result = await _sut.GetCouponByCode(couponCode);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true, data = coupon });
        }

        /// <summary>
        /// Verifies that GetCouponByCode returns NotFound when coupon doesn't exist.
        /// </summary>
        [Fact]
        public async Task GetCouponByCode_CouponNotFound_ReturnsNotFound()
        {
            // Arrange
            var couponCode = "INVALID";

            _mockCouponService.Setup(x => x.GetByCodeAsync(couponCode))
                .ReturnsAsync((CouponDto?)null);

            // Act
            var result = await _sut.GetCouponByCode(couponCode);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region CreateCoupon Tests (Admin Only)

        /// <summary>
        /// Verifies that CreateCoupon returns CreatedAtAction when coupon is created successfully.
        /// </summary>
        [Fact]
        public async Task CreateCoupon_ValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateCouponDto
            {
                Code = "NEWSAVE20",
                DiscountPercent = 20,
                MinOrderValue = 50,
                IsActive = true
            };

            var createdCoupon = new CouponDto
            {
                CouponId = 1,
                Code = "NEWSAVE20",
                DiscountPercent = 20,
                MinOrderValue = 50,
                IsActive = true
            };

            _mockCouponService.Setup(x => x.CreateAsync(createDto))
                .ReturnsAsync(createdCoupon);

            // Act
            var result = await _sut.CreateCoupon(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(CouponController.GetCouponById));
            createdResult.Value.Should().BeEquivalentTo(new { success = true, data = createdCoupon });
        }

        /// <summary>
        /// Verifies that CreateCoupon returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task CreateCoupon_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateCouponDto();
            _sut.ModelState.AddModelError("Code", "Code is required");

            // Act
            var result = await _sut.CreateCoupon(createDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        /// <summary>
        /// Verifies that CreateCoupon returns BadRequest when coupon code already exists.
        /// </summary>
        [Fact]
        public async Task CreateCoupon_DuplicateCode_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateCouponDto
            {
                Code = "EXISTING",
                DiscountPercent = 10
            };

            _mockCouponService.Setup(x => x.CreateAsync(createDto))
                .ThrowsAsync(new ArgumentException("Coupon with this code already exists."));

            // Act
            var result = await _sut.CreateCoupon(createDto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { success = false, message = "Coupon with this code already exists." });
        }

        /// <summary>
        /// Verifies that CreateCoupon returns BadRequest when discount percent is invalid.
        /// </summary>
        [Fact]
        public async Task CreateCoupon_InvalidDiscountPercent_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateCouponDto
            {
                Code = "INVALID",
                DiscountPercent = 150 // Invalid: > 100
            };

            _mockCouponService.Setup(x => x.CreateAsync(createDto))
                .ThrowsAsync(new ArgumentException("Discount percent must be between 1 and 100."));

            // Act
            var result = await _sut.CreateCoupon(createDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region UpdateCoupon Tests (Admin Only)

        /// <summary>
        /// Verifies that UpdateCoupon returns Ok when coupon is updated successfully.
        /// </summary>
        [Fact]
        public async Task UpdateCoupon_ValidData_ReturnsOk()
        {
            // Arrange
            var couponId = 1;
            var updateDto = new UpdateCouponDto
            {
                CouponId = couponId,
                Code = "UPDATED20",
                DiscountPercent = 20
            };

            _mockCouponService.Setup(x => x.UpdateAsync(updateDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateCoupon(couponId, updateDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true, message = "Coupon updated successfully" });
        }

        /// <summary>
        /// Verifies that UpdateCoupon returns BadRequest when ID mismatch.
        /// </summary>
        [Fact]
        public async Task UpdateCoupon_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var couponId = 1;
            var updateDto = new UpdateCouponDto
            {
                CouponId = 2, // Mismatch
                Code = "TEST"
            };

            // Act
            var result = await _sut.UpdateCoupon(couponId, updateDto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { success = false, message = "ID mismatch" });
        }

        /// <summary>
        /// Verifies that UpdateCoupon returns BadRequest when coupon not found.
        /// </summary>
        [Fact]
        public async Task UpdateCoupon_CouponNotFound_ReturnsBadRequest()
        {
            // Arrange
            var couponId = 999;
            var updateDto = new UpdateCouponDto
            {
                CouponId = couponId,
                Code = "NOTFOUND"
            };

            _mockCouponService.Setup(x => x.UpdateAsync(updateDto))
                .ThrowsAsync(new ArgumentException("Coupon not found."));

            // Act
            var result = await _sut.UpdateCoupon(couponId, updateDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region DeleteCoupon Tests (Admin Only)

        /// <summary>
        /// Verifies that DeleteCoupon returns Ok when coupon is deleted successfully.
        /// </summary>
        [Fact]
        public async Task DeleteCoupon_ValidId_ReturnsOk()
        {
            // Arrange
            var couponId = 1;

            _mockCouponService.Setup(x => x.DeleteAsync(couponId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteCoupon(couponId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true, message = "Coupon deleted successfully" });
        }

        /// <summary>
        /// Verifies that DeleteCoupon returns BadRequest when coupon not found.
        /// </summary>
        [Fact]
        public async Task DeleteCoupon_CouponNotFound_ReturnsBadRequest()
        {
            // Arrange
            var couponId = 999;

            _mockCouponService.Setup(x => x.DeleteAsync(couponId))
                .ThrowsAsync(new ArgumentException("Coupon not found."));

            // Act
            var result = await _sut.DeleteCoupon(couponId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region ToggleCouponStatus Tests (Admin Only)

        /// <summary>
        /// Verifies that ToggleCouponStatus returns Ok when status is toggled successfully.
        /// </summary>
        [Fact]
        public async Task ToggleCouponStatus_ValidId_ReturnsOk()
        {
            // Arrange
            var couponId = 1;

            _mockCouponService.Setup(x => x.ToggleActiveStatusAsync(couponId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ToggleCouponStatus(couponId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true, message = "Coupon status toggled successfully" });
        }

        /// <summary>
        /// Verifies that ToggleCouponStatus returns BadRequest when coupon not found.
        /// </summary>
        [Fact]
        public async Task ToggleCouponStatus_CouponNotFound_ReturnsBadRequest()
        {
            // Arrange
            var couponId = 999;

            _mockCouponService.Setup(x => x.ToggleActiveStatusAsync(couponId))
                .ThrowsAsync(new ArgumentException("Coupon not found."));

            // Act
            var result = await _sut.ToggleCouponStatus(couponId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Helper Methods

        private void SetupControllerWithUser(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        private void SetupControllerWithAdminUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-123"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        #endregion
    }
}
