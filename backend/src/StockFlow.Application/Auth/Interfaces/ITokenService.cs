using StockFlow.Application.Auth.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Auth.Interfaces
{
    public interface ITokenService
    {
        AccessTokenResult CreateAccessToken(UserDto user);
        string GenerateRefreshToken();
        string HashToken(string token);
        DateTime GetRefreshTokenExpiry();
    }
}
