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

namespace StockFlow.Application.Auth
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Invalid email or password.";
        private const string InvalidRefreshTokenMessage = "Invalid or expired refresh token.";
        private const string NotAuthenticatedMessage = "User is not authenticated.";
        
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        public AuthService(IUserService userService, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
        {
            _userService = userService;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<UserDto> RegisterAsync(RegisterRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new BadRequestException("Email, password, and full name are required.");
            }
            var email = request.Email.Trim();
            var existingUser = await _userService.FindByEmailAsync(email);
            if (existingUser != null)
            {
                throw new BadRequestException("Email is already registered.");
            }
            // User tự đăng ký luôn là Staff. Admin chỉ được tạo bằng seed.
            return await _userService.CreateAsync(email, request.Password, request.FullName.Trim(), Roles.Staff);

        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new UnauthorizedException(InvalidCredentialsMessage);
            }
            var user = await _userService.FindByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                throw new UnauthorizedException(InvalidCredentialsMessage);
            }
            var isPasswordValid = await _userService.CheckPasswordAsync(user.Id, request.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedException(InvalidCredentialsMessage);
            }
            return await IssueTokensAsync(user);
        }


        public async Task<AuthResult> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }
            var tokenHash = _tokenService.HashToken(refreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
            if (storedToken == null || !storedToken.IsActive)
            {
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }
            var user = await _userService.FindByIdAsync(storedToken.UserId);
            if (user == null)
            {
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }
            // Rotation: mỗi refresh token chỉ dùng được 1 lần
            storedToken.Revoke();
            return await IssueTokensAsync(user);

        }


        public async Task LogoutAsync(string? refreshToken) {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }
            var tokenHash = _tokenService.HashToken(refreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
            if (storedToken == null)
            {
                return;
            }
            storedToken.Revoke();
            await _refreshTokenRepository.SaveChangesAsync();
        }


        public async Task<UserDto> GetCurrentUserAsync(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedException(NotAuthenticatedMessage);
            }
            var user = await _userService.FindByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedException(NotAuthenticatedMessage);
            }

            return user;
        }

        private async Task<AuthResult> IssueTokensAsync(UserDto user)
        {
            var accessToken = _tokenService.CreateAccessToken(user);
            var rawRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiresAt= _tokenService.GetRefreshTokenExpiry();
            var refreshTokenEntity= RefreshToken.Create(
                user.Id,
                _tokenService.HashToken(rawRefreshToken),
                refreshTokenExpiresAt
                );
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();
            return new AuthResult(
                accessToken.Token,
                accessToken.ExpiresAt,
                rawRefreshToken,
                refreshTokenExpiresAt,
                user);
        }
    }
}
