using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using StockFlow.Application.Auth.Dtos;
using StockFlow.Application.Auth.Interfaces;
using StockFlow.Application.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserService (UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

        }
        public async Task<UserDto?> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user == null ? null : await MapToDtoAsync(user);
        }
        public async Task<UserDto?> FindByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? null : await MapToDtoAsync(user);
        }
        public async Task<bool> CheckPasswordAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            return await _userManager.CheckPasswordAsync(user, password);
        }
        public async Task<UserDto> CreateAsync(string email, string password, string fullName, string role)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }
            await _userManager.AddToRoleAsync(user, role);
            return await MapToDtoAsync(user);
        }
        private async Task<UserDto> MapToDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto(user.Id, user.Email ?? string.Empty, user.FullName, roles);
        }
    }
}
