using StockFlow.Application.Auth.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> RefreshAsync(string refreshToken);
        Task LogoutAsync (string? refreshToken);
        Task<UserDto> GetCurrentUserAsync(string? userId);
    }
}
