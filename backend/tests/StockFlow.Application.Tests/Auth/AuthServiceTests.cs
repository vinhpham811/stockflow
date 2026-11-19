using Moq;
using StockFlow.Application.Auth;
using StockFlow.Application.Auth.Dtos;
using StockFlow.Application.Auth.Interfaces;
using StockFlow.Application.Common;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Tests.Auth
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly UserDto _testUser = new("user-1", "user@test.com", "Test User", new List<string> { Roles.Staff });

        private readonly AuthService _authService;
        public AuthServiceTests()
        {
            // Giả lập token service: trả về giá trị cố định để dễ kiểm tra
            _tokenServiceMock
                .Setup(t => t.CreateAccessToken(It.IsAny<UserDto>()))
                .Returns(new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(15)));
            _tokenServiceMock
                .Setup(t => t.GenerateRefreshToken())
                .Returns("new-refresh-token");
            _tokenServiceMock
                .Setup(t => t.HashToken(It.IsAny<string>()))
                .Returns((string token) => "hash-" + token);
            _tokenServiceMock
                .Setup(t => t.GetRefreshTokenExpiry())
                .Returns(DateTime.UtcNow.AddDays(7));
            _authService = new AuthService(_userServiceMock.Object,_tokenServiceMock.Object, _refreshTokenRepositoryMock.Object);

        }

        // ================= REGISTER User Testing =================

        [Theory]
        [InlineData("", "Password1", "Name")]
        [InlineData("a@test.com", "", "Name")]
        [InlineData("a@test.com", "Password1", "")]
        public async Task RegisterAsync_WithMissingFields_ShouldThrowBadRequest(string email, string password, string fullName)
        {
            var request = new RegisterRequest(email, password, fullName);
            await Assert.ThrowsAsync<BadRequestException>(() => _authService.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowBadRequest()
        {
            _userServiceMock.Setup(u => u.FindByEmailAsync("")).ReturnsAsync(_testUser);
            var request = new RegisterRequest("user@test.com", "Password1", "Test User");
            await Assert.ThrowsAsync<BadRequestException>(()=> _authService.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateStaffUser()
        {
            _userServiceMock.Setup(u => u.FindByEmailAsync("new@test.com")).ReturnsAsync((UserDto?) null);
            _userServiceMock.Setup(u => u.CreateAsync("new@test.com", "Password1", "New User", Roles.Staff))
                .ReturnsAsync(_testUser);

            var request = new RegisterRequest(" new@test.com ", "Password1", " New User ");

            var result = await _authService.RegisterAsync(request);
            Assert.Equal(_testUser, result);

            _userServiceMock.Verify(u=>u.CreateAsync("new@test.com", "Password1", "New User", Roles.Staff), Times.Once);
        }

        // ================= LOGIN Testing =================
        [Theory]
        [InlineData("", "Password1")]
        [InlineData("user@test.com", "")]
        public async Task LoginAsync_WithEmptyEmailOrPassword_ShouldThrowUnauthorized(string email, string password)
        {
            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.LoginAsync(new LoginRequest(email, password)));
        }

        [Fact]
        public async Task LoginAsync_WithUnknownEmail_ShouldThrowUnauthorized()
        {
            _userServiceMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((UserDto?)null);

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _authService.LoginAsync(new LoginRequest("unknown@test.com", "Password1")));
        }
        [Fact]
        public async Task LoginAsync_WithWrongPassword_ShouldThrowUnauthorized()
        {
            _userServiceMock.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(_testUser);
            _userServiceMock.Setup(u => u.CheckPasswordAsync("user-1", "wrong")).ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _authService.LoginAsync(new LoginRequest("user@test.com", "wrong")));
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnTokensAndSaveRefeshToken()
        {
            _userServiceMock.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(_testUser);
            _userServiceMock.Setup(u => u.CheckPasswordAsync("user-1", "Password1")).ReturnsAsync(true);

            var result = await _authService.LoginAsync(new LoginRequest("user@test.com", "Password1"));
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("new-refresh-token", result.RefreshToken);
            Assert.Equal(_testUser, result.User);

            // Phải lưu bản HASH chứ không lưu token gốc
            _refreshTokenRepositoryMock.Verify(
                r => r.AddAsync(It.Is<RefreshToken>(t => t.UserId == "user-1" && t.TokenHash == "hash-new-refresh-token")),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ================= REFRESH Testing =================

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task RefreshAsync_WithEmptyToken_ShouldThrowUnauthorized(string? refreshToken)
        {
            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.RefreshAsync(refreshToken));
        }

        [Fact]
        public async Task RefreshAsync_WithUnknownToken_ShouldThrowUnauthorized()
        {
            _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.RefreshAsync("unknown-token"));
        }

        [Fact]
        public async Task RefreshAsync_WithRevokedToken_ShouldThrowUnauthorized()
        {
            var storedToken = RefreshToken.Create("user-1", "hash-old-token", DateTime.UtcNow.AddDays(7));
            storedToken.Revoke();
            _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash-old-token")).ReturnsAsync(storedToken);

            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.RefreshAsync("old-token"));
        }
        [Fact]
        public async Task RefreshAsync_WhenUserNoLongerExists_ShouldThrowUnauthorized()
        {
            var storedToken = RefreshToken.Create("user-1", "hash-old-token", DateTime.UtcNow.AddDays(7));
            _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash-old-token")).ReturnsAsync(storedToken);
            _userServiceMock.Setup(u => u.FindByIdAsync("user-1")).ReturnsAsync((UserDto?)null);
            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.RefreshAsync("old-token"));

        }

        [Fact]
        public async Task RefreshAsync_WithValidToken_ShouldRevokeOldTokenAndIssueToken()
        {
            var storedToken = RefreshToken.Create("user-1", "hash-old-token", DateTime.UtcNow.AddDays(7));
            _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash-old-token")).ReturnsAsync(storedToken);
            _userServiceMock.Setup(u => u.FindByIdAsync("user-1")).ReturnsAsync(_testUser);
            var result = await _authService.RefreshAsync("old-token");

            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("new-refresh-token", result.RefreshToken);
            Assert.False(storedToken.IsActive); // token cũ đã bị thu hồi

            _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
            _refreshTokenRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);

        }

        // ================= LOGOUT Testing =================
        [Fact]
        public async Task LogoutAsync_WithEmptyToken_ShouldDoNothing()
        {
            await _authService.LogoutAsync(null);

            _refreshTokenRepositoryMock.Verify(r => r.GetByTokenHashAsync(It.IsAny<string>()), Times.Never);
            _refreshTokenRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_WithUnknownToken_ShouldNotSave()
        {
            _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

            await _authService.LogoutAsync("unknown-token");

            _refreshTokenRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task LogoutAsync_WithValidToken_ShouldRevokeAndSave()
        {
            var storedToken = RefreshToken.Create("user-1", "hash-my-token", DateTime.UtcNow.AddDays(7));
            _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash-my-token")).ReturnsAsync(storedToken);

            await _authService.LogoutAsync("my-token");

            Assert.False(storedToken.IsActive);
            _refreshTokenRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ================= GET CURRENT USER =================
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetCurrentUserAsync_WithEmptyUserId_ShouldThrowUnauthorized(string? userId)
        {
            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.GetCurrentUserAsync(userId));
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithUnknownUser_ShouldThrowUnauthorized()
        {
            _userServiceMock.Setup(u => u.FindByIdAsync("ghost")).ReturnsAsync((UserDto?)null);

            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.GetCurrentUserAsync("ghost"));
        }

        [Fact]
        public async Task GetCurrentUserAsync_WithValidUser_ShouldReturnUser()
        {
            _userServiceMock.Setup(u => u.FindByIdAsync("user-1")).ReturnsAsync(_testUser);

            var result = await _authService.GetCurrentUserAsync("user-1");

            Assert.Equal(_testUser, result);
        }
    }
}
