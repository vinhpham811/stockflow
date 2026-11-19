using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Auth.Dtos
{
    public record RegisterRequest(string Email, string Password, string FullName);
    public record LoginRequest(string Email, string Password);
    public record UserDto(string Id, string Email, string FullName, IList<string> Roles);
    public record AccessTokenResult(string Token, DateTime ExpiresAt);
    /// <summary>Kết quả nội bộ: có cả refresh token để Controller ghi vào cookie.</summary
    public record AuthResult(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt,
        UserDto User
        );
    /// <summary>Trả về cho FE: KHÔNG có refresh token (nó nằm trong cookie).</summary>
    public record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);
  
    /// <summary>
    /// Cach khac dung dung record
    /// </summary>
    //public class RegisterRequest
    //{
    //    public string Email { get; set; } = string.Empty;
    //    public string Password { get; set; } = string.Empty;
    //    public string FullName { get; set; } = string.Empty;
    //}

    //public class LoginRequest
    //{
    //    public string Email { get; set; } = string.Empty;
    //    public string Password { get; set; } = string.Empty;
    //}
    //public class LoginRequest
    //{
    //    public string Email { get; set; }
    //    public string Password { get; set; }

    //    // Tự tay viết Constructor nhận tham số
    //    public LoginRequest(string email, string password)
    //    {
    //        Email = email;
    //        Password = password;
    //    }
    //}
    //public class UserDto
    //{
    //    public string Id { get; set; } = string.Empty;
    //    public string Email { get; set; } = string.Empty;
    //    public string FullName { get; set; } = string.Empty;
    //    public IList<string> Roles { get; set; } = new List<string>();
    //}

    //public class AccessTokenResult
    //{
    //    public string Token { get; set; } = string.Empty;
    //    public DateTime ExpiresAt { get; set; }
    //}

}
