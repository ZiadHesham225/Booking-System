using Booking_System.Application.DTOs.Auth;
using Booking_System.Application.Interfaces;
using Booking_System.Application.Services;
using Booking_System.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;

namespace Booking_System.Application.Tests.Services
{
    /// <summary>
    /// Unit tests for AuthService covering authentication, registration, and token management functionality.
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            // Setup UserManager mock
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockTokenService = new Mock<ITokenService>();
            _mockEmailService = new Mock<IEmailService>();

            _sut = new AuthService(
                _mockUserManager.Object,
                _mockConfiguration.Object,
                _mockEmailService.Object,
                _mockTokenService.Object);
        }

        #region Login Tests

        /// <summary>
        /// Verifies that LoginAsync returns AuthResponseDto with valid tokens when credentials are correct.
        /// </summary>
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "ValidPassword123!" };
            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = "test@example.com",
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            _mockTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>()))
                .Returns("access-token-123");
            _mockTokenService.Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token-456");
            _mockTokenService.Setup(x => x.SaveRefreshTokenAsync(user.Id, It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().Be("access-token-123");
            result.RefreshToken.Should().Be("refresh-token-456");
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(user.Email);
            result.User.Id.Should().Be(user.Id);
        }

        /// <summary>
        /// Verifies that LoginAsync throws AuthenticationException when email is not found.
        /// </summary>
        [Fact]
        public async Task LoginAsync_InvalidEmail_ThrowsAuthenticationException()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "nonexistent@example.com", Password = "Password123!" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _sut.LoginAsync(loginDto);

            // Assert
            await act.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("Invalid email or password");
        }

        /// <summary>
        /// Verifies that LoginAsync throws AuthenticationException when password is incorrect.
        /// </summary>
        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsAuthenticationException()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword" };
            var user = new ApplicationUser { Id = "user-123", Email = "test@example.com" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _sut.LoginAsync(loginDto);

            // Assert
            await act.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("Invalid email or password");
        }

        #endregion

        #region Registration Tests

        /// <summary>
        /// Verifies that RegisterUserAsync successfully registers a new user with valid data.
        /// </summary>
        [Fact]
        public async Task RegisterUserAsync_ValidData_RegistersSuccessfully()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Password = "ValidPassword123!"
            };

            _mockUserManager.Setup(x => x.FindByNameAsync(registerDto.Username))
                .ReturnsAsync((ApplicationUser?)null);
            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _sut.RegisterUserAsync(registerDto);

            // Assert
            _mockUserManager.Verify(x => x.CreateAsync(
                It.Is<ApplicationUser>(u =>
                    u.FirstName == registerDto.FirstName &&
                    u.LastName == registerDto.LastName &&
                    u.UserName == registerDto.Username &&
                    u.Email == registerDto.Email),
                registerDto.Password), Times.Once);
        }

        /// <summary>
        /// Verifies that RegisterUserAsync throws exception when username already exists.
        /// </summary>
        [Fact]
        public async Task RegisterUserAsync_UsernameExists_ThrowsAuthenticationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "existinguser",
                Email = "new@example.com",
                Password = "Password123!"
            };

            var existingUser = new ApplicationUser { UserName = "existinguser" };
            _mockUserManager.Setup(x => x.FindByNameAsync(registerDto.Username))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _sut.RegisterUserAsync(registerDto);

            // Assert
            await act.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("User with this username already exists");
        }

        /// <summary>
        /// Verifies that RegisterUserAsync throws exception when email already exists.
        /// </summary>
        [Fact]
        public async Task RegisterUserAsync_EmailExists_ThrowsAuthenticationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Email = "existing@example.com",
                Password = "Password123!"
            };

            _mockUserManager.Setup(x => x.FindByNameAsync(registerDto.Username))
                .ReturnsAsync((ApplicationUser?)null);
            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync(new ApplicationUser { Email = "existing@example.com" });

            // Act
            Func<Task> act = async () => await _sut.RegisterUserAsync(registerDto);

            // Assert
            await act.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("User with this email already exists");
        }

        /// <summary>
        /// Verifies that RegisterUserAsync throws exception when password doesn't meet complexity requirements.
        /// </summary>
        [Fact]
        public async Task RegisterUserAsync_WeakPassword_ThrowsAuthenticationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Password = "weak"
            };

            _mockUserManager.Setup(x => x.FindByNameAsync(registerDto.Username))
                .ReturnsAsync((ApplicationUser?)null);
            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Password must be at least 6 characters." }));

            // Act
            Func<Task> act = async () => await _sut.RegisterUserAsync(registerDto);

            // Assert
            await act.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("*Password must be at least 6 characters*");
        }

        #endregion

        #region Token Refresh Tests

        /// <summary>
        /// Verifies that RefreshTokenAsync returns new tokens when valid tokens are provided.
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_ValidTokens_ReturnsNewTokens()
        {
            // Arrange
            var accessToken = "old-access-token";
            var refreshToken = "valid-refresh-token";
            var expectedResponse = new TokenResponseDto
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                AccessTokenExpiration = DateTime.UtcNow.AddHours(3),
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(7)
            };

            _mockTokenService.Setup(x => x.RefreshAccessTokenAsync(accessToken, refreshToken))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.RefreshTokenAsync(accessToken, refreshToken);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().Be("new-access-token");
            result.RefreshToken.Should().Be("new-refresh-token");
        }

        #endregion

        #region Token Revocation Tests

        /// <summary>
        /// Verifies that RevokeTokenAsync calls token service to revoke refresh token.
        /// </summary>
        [Fact]
        public async Task RevokeTokenAsync_ValidUserId_RevokesToken()
        {
            // Arrange
            var userId = "user-123";
            _mockTokenService.Setup(x => x.RevokeRefreshTokenAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.RevokeTokenAsync(userId);

            // Assert
            _mockTokenService.Verify(x => x.RevokeRefreshTokenAsync(userId), Times.Once);
        }

        #endregion

        #region Password Reset Tests

        /// <summary>
        /// Verifies that ForgotPasswordAsync sends reset email when user exists.
        /// </summary>
        [Fact]
        public async Task ForgotPasswordAsync_ValidEmail_SendsResetEmail()
        {
            // Arrange
            var model = new ForgotPasswordRequestDto { Email = "test@example.com" };
            var user = new ApplicationUser { Id = "user-123", Email = model.Email, UserName = "testuser" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token-123");
            _mockConfiguration.Setup(x => x["JWT:ValidIssuer"])
                .Returns("https://example.com");
            _mockEmailService.Setup(x => x.SendPasswordResetEmailAsync(
                model.Email, user.UserName, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ForgotPasswordAsync(model);

            // Assert
            _mockEmailService.Verify(x => x.SendPasswordResetEmailAsync(
                model.Email, user.UserName, It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Verifies that ForgotPasswordAsync does not send email when user doesn't exist (silent fail for security).
        /// </summary>
        [Fact]
        public async Task ForgotPasswordAsync_NonExistentEmail_DoesNotSendEmail()
        {
            // Arrange
            var model = new ForgotPasswordRequestDto { Email = "nonexistent@example.com" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            await _sut.ForgotPasswordAsync(model);

            // Assert
            _mockEmailService.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that ResetPasswordAsync successfully resets password with valid token.
        /// </summary>
        [Fact]
        public async Task ResetPasswordAsync_ValidToken_ResetsPassword()
        {
            // Arrange
            var model = new ResetPasswordRequestDto
            {
                Email = "test@example.com",
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };
            var user = new ApplicationUser { Id = "user-123", Email = model.Email };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), model.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _sut.ResetPasswordAsync(model);

            // Assert
            _mockUserManager.Verify(x => x.ResetPasswordAsync(user, It.IsAny<string>(), model.NewPassword), Times.Once);
        }

        /// <summary>
        /// Verifies that ResetPasswordAsync throws exception when email is not found.
        /// </summary>
        [Fact]
        public async Task ResetPasswordAsync_InvalidEmail_ThrowsApplicationException()
        {
            // Arrange
            var model = new ResetPasswordRequestDto
            {
                Email = "nonexistent@example.com",
                Token = "token",
                NewPassword = "NewPassword123!"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _sut.ResetPasswordAsync(model);

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage($"*{model.Email}*Not Found*");
        }

        /// <summary>
        /// Verifies that ResetPasswordAsync throws exception when token is invalid.
        /// </summary>
        [Fact]
        public async Task ResetPasswordAsync_InvalidToken_ThrowsApplicationException()
        {
            // Arrange
            var model = new ResetPasswordRequestDto
            {
                Email = "test@example.com",
                Token = "invalid-token",
                NewPassword = "NewPassword123!"
            };
            var user = new ApplicationUser { Id = "user-123", Email = model.Email };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), model.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

            // Act
            Func<Task> act = async () => await _sut.ResetPasswordAsync(model);

            // Assert
            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("*Failed to reset password*");
        }

        #endregion

        #region Password Reset Link Generation Tests

        /// <summary>
        /// Verifies that GeneratePasswordResetLink creates correct reset link format.
        /// </summary>
        [Theory]
        [InlineData("https://example.com", "token123", "user@test.com")]
        [InlineData("https://myapp.com/reset", "abc+def/ghi", "test@domain.com")]
        public void GeneratePasswordResetLink_ValidInputs_ReturnsCorrectLink(
            string baseUrl, string token, string email)
        {
            // Act
            var result = _sut.GeneratePasswordResetLink(baseUrl, token, email);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().StartWith(baseUrl);
            result.Should().Contain("email=");
            result.Should().Contain("token=");
        }

        /// <summary>
        /// Verifies that GeneratePasswordResetLink returns empty string when base URL is null.
        /// </summary>
        [Fact]
        public void GeneratePasswordResetLink_NullBaseUrl_ReturnsEmptyString()
        {
            // Act
            var result = _sut.GeneratePasswordResetLink(null!, "token", "email@test.com");

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}
