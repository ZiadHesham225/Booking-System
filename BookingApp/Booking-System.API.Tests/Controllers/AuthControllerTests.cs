using Booking_System.Application.DTOs.Auth;
using Booking_System.Application.Interfaces;
using Booking_System.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Security.Authentication;
using System.Security.Claims;

namespace Booking_System.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for AuthController covering authentication endpoints.
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _sut = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        #region Register Tests

        /// <summary>
        /// Verifies that Register returns Created status when registration is successful.
        /// </summary>
        [Fact]
        public async Task Register_ValidData_ReturnsCreatedStatus()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Password = "SecurePassword123!"
            };

            _mockAuthService.Setup(x => x.RegisterUserAsync(registerDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Register(registerDto);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Verifies that Register returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task Register_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto();
            _sut.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _sut.Register(registerDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Verifies that Register returns BadRequest when username already exists.
        /// </summary>
        [Fact]
        public async Task Register_UsernameExists_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "existinguser",
                Email = "john@example.com",
                Password = "SecurePassword123!"
            };

            _mockAuthService.Setup(x => x.RegisterUserAsync(registerDto))
                .ThrowsAsync(new AuthenticationException("User with this username already exists"));

            // Act
            var result = await _sut.Register(registerDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            objectResult.Value.Should().BeEquivalentTo(new { message = "User with this username already exists" });
        }

        /// <summary>
        /// Verifies that Register returns BadRequest when email already exists.
        /// </summary>
        [Fact]
        public async Task Register_EmailExists_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "existing@example.com",
                Password = "SecurePassword123!"
            };

            _mockAuthService.Setup(x => x.RegisterUserAsync(registerDto))
                .ThrowsAsync(new AuthenticationException("User with this email already exists"));

            // Act
            var result = await _sut.Register(registerDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            objectResult.Value.Should().BeEquivalentTo(new { message = "User with this email already exists" });
        }

        /// <summary>
        /// Verifies that Register returns 500 status when unexpected error occurs.
        /// </summary>
        [Fact]
        public async Task Register_UnexpectedError_ReturnsInternalServerError()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Password = "SecurePassword123!"
            };

            _mockAuthService.Setup(x => x.RegisterUserAsync(registerDto))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.Register(registerDto);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        #endregion

        #region Login Tests

        /// <summary>
        /// Verifies that Login returns Ok with auth response when credentials are valid.
        /// </summary>
        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "ValidPassword123!"
            };

            var authResponse = new AuthResponseDto
            {
                AccessToken = "access-token-123",
                RefreshToken = "refresh-token-456",
                AccessTokenExpiration = DateTime.UtcNow.AddHours(3),
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(7),
            };

            _mockAuthService.Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _sut.Login(loginDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            objectResult.Value.Should().BeEquivalentTo(authResponse);
        }

        /// <summary>
        /// Verifies that Login returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task Login_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto();
            _sut.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _sut.Login(loginDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Verifies that Login returns BadRequest when email is incorrect.
        /// </summary>
        [Fact]
        public async Task Login_InvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "wrong@example.com",
                Password = "Password123!"
            };

            _mockAuthService.Setup(x => x.LoginAsync(loginDto))
                .ThrowsAsync(new AuthenticationException("Invalid email or password"));

            // Act
            var result = await _sut.Login(loginDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Invalid email or password" });
        }

        /// <summary>
        /// Verifies that Login returns BadRequest when password is incorrect.
        /// </summary>
        [Fact]
        public async Task Login_InvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            _mockAuthService.Setup(x => x.LoginAsync(loginDto))
                .ThrowsAsync(new AuthenticationException("Invalid email or password"));

            // Act
            var result = await _sut.Login(loginDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Invalid email or password" });
        }

        #endregion

        #region RefreshToken Tests

        /// <summary>
        /// Verifies that RefreshToken returns Ok with new tokens when valid tokens are provided.
        /// </summary>
        [Fact]
        public async Task RefreshToken_ValidTokens_ReturnsOkWithNewTokens()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto
            {
                AccessToken = "old-access-token",
                RefreshToken = "valid-refresh-token"
            };

            var tokenResponse = new TokenResponseDto
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                AccessTokenExpiration = DateTime.UtcNow.AddHours(3),
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(7)
            };

            _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshTokenDto.AccessToken, refreshTokenDto.RefreshToken))
                .ReturnsAsync(tokenResponse);

            // Act
            var result = await _sut.RefreshToken(refreshTokenDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            objectResult.Value.Should().BeEquivalentTo(tokenResponse);
        }

        /// <summary>
        /// Verifies that RefreshToken returns BadRequest when model state is invalid.
        /// </summary>
        [Fact]
        public async Task RefreshToken_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto();
            _sut.ModelState.AddModelError("AccessToken", "AccessToken is required");

            // Act
            var result = await _sut.RefreshToken(refreshTokenDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Verifies that RefreshToken returns Unauthorized when token is invalid or expired.
        /// </summary>
        [Fact]
        public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto
            {
                AccessToken = "invalid-access-token",
                RefreshToken = "invalid-refresh-token"
            };

            _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshTokenDto.AccessToken, refreshTokenDto.RefreshToken))
                .ThrowsAsync(new SecurityTokenException("Invalid or expired token"));

            // Act
            var result = await _sut.RefreshToken(refreshTokenDto);

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Invalid or expired token" });
        }

        #endregion

        #region RevokeToken Tests

        /// <summary>
        /// Verifies that RevokeToken returns Ok when token is successfully revoked.
        /// </summary>
        [Fact]
        public async Task RevokeToken_ValidUser_ReturnsOk()
        {
            // Arrange
            var userId = "user-123";
            SetupControllerWithUser(userId);

            _mockAuthService.Setup(x => x.RevokeTokenAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RevokeToken();

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Token revoked successfully" });
        }

        /// <summary>
        /// Verifies that RevokeToken returns Unauthorized when user ID is not found in claims.
        /// </summary>
        [Fact]
        public async Task RevokeToken_NoUserIdInClaims_ReturnsUnauthorized()
        {
            // Arrange
            SetupControllerWithoutUser();

            // Act
            var result = await _sut.RevokeToken();

            // Assert
            result.Should().BeAssignableTo<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
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

        private void SetupControllerWithoutUser()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

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
