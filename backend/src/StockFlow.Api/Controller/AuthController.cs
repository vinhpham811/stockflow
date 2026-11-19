using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.Auth.Dtos;
using StockFlow.Application.Auth.Interfaces;
using System.Security.Claims;

namespace StockFlow.Api.Controller
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private const string RefreshTokenCookieName = "refreshToken";
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterRequest request) {
            var user = await _authService.RegisterAsync(request);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login (LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
            return Ok(ToResponse(result));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            var result = await _authService.RefreshAsync(refreshToken);
            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
            return Ok(ToResponse(result));

        }
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            await _authService.LogoutAsync(refreshToken);
            Response.Cookies.Delete(RefreshTokenCookieName, BuildCookieOptions(DateTime.UtcNow));
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var user = await _authService.GetCurrentUserAsync(userId);
            return Ok(user);
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
        {
            Response.Cookies.Append(RefreshTokenCookieName, refreshToken, BuildCookieOptions(expiresAt));
        }

        private static CookieOptions BuildCookieOptions(DateTime expiresAt)
        {
            return new CookieOptions
            {
                HttpOnly = true,               // JavaScript không đọc được → chống XSS
                Secure = true,                 // Chỉ gửi qua HTTPS
                SameSite = SameSiteMode.None,  // FE và API khác domain (trên Azure)
                Expires = expiresAt,
                Path = "/api/auth"             // Chỉ gửi cookie tới các API auth
            };
        }
        private static AuthResponse ToResponse(AuthResult result)
        {
            return new AuthResponse(result.AccessToken, result.AccessTokenExpiresAt, result.User);
        }
    }
}
