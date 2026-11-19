using StockFlow.Application.Auth.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Auth.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> FindByEmailAsync(string email);
        Task<UserDto?> FindByIdAsync(string UserId);
        Task<bool> CheckPasswordAsync(string userId, string password);
        Task<UserDto> CreateAsync(string email, string password, string fullName, string role);
    }
}
