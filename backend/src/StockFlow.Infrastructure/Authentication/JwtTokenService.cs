using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockFlow.Application.Auth.Dtos;
using StockFlow.Application.Auth.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Authentication
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        public JwtTokenService (IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }
        public AccessTokenResult CreateAccessToken(UserDto user)
        {
            var claim = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("fullName", user.FullName)
            };

            foreach (var role in user.Roles)
            {
                claim.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer:_settings.Issuer,
                audience:_settings.Audience,
                claims:claim,
                expires:expiresAt,
                signingCredentials:credentials);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AccessTokenResult(tokenString, expiresAt);

        }
        public string GenerateRefreshToken()
        {
            // 32 byte ngẫu nhiên an toàn → chuỗi hex 64 ký tự
            var randomBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToHexString(randomBytes);

        }
        public string HashToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes);

        }
        public DateTime GetRefreshTokenExpiry()
        {
            return DateTime.UtcNow.AddDays(_settings.RefreshTokenDays);

        }
    }
}
