using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data.models;
using Mshop.Api.DTOs.Responses;
using System;

namespace Mshop.Api.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> userManager;
        public static class ChangeRoleResult
        {
            public const int USER_NOT_FOUND = 0;
            public const int DELETING_ROLES_FAILED = 1;
            public const int ASSIGN_ROLE_FAILED = 2;
            public const int SUCCEEDED = 3;
        }
        public UserService(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<int> ChangeRoleAsync(string id, string role)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return ChangeRoleResult.USER_NOT_FOUND;
            }
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, roles);
                if (!removeResult.Succeeded)
                {
                    return ChangeRoleResult.DELETING_ROLES_FAILED;
                }

            }
            var addResult = await userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                return ChangeRoleResult.ASSIGN_ROLE_FAILED;
            }
            return ChangeRoleResult.SUCCEEDED;
        }

        public async Task<IEnumerable<ApplicationUserResponse>> GetAllAsync()
        {
            var users = await userManager.Users.ToListAsync();
            return users.Adapt<IEnumerable<ApplicationUserResponse>>();
        }

        public async Task<ApplicationUserResponse> GetOneAsync(string id)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user.Adapt<ApplicationUserResponse>();
        }

        public async Task<bool?> LockUserAsync(string id, DateTime dateTime)
        {
            var applicationUser= await userManager.FindByIdAsync(id);
            if(applicationUser == null)
            {
                return null;
            }
            if(dateTime <= DateTime.UtcNow)
            {
                return false;
            }
            applicationUser.LockoutEnabled = true;
            applicationUser.LockoutEnd = dateTime;
            var result = await userManager.UpdateAsync(applicationUser);
            return result.Succeeded;
        }

        public async Task<bool?> UnlockUserAsync(string id)
        {
            var applicationUser = await userManager.FindByIdAsync(id);
            if (applicationUser == null)
            {
                return null;
            }
            applicationUser.LockoutEnabled = false;
            applicationUser.LockoutEnd = null;
            var result = await userManager.UpdateAsync(applicationUser);
            return result.Succeeded;
        }
    }
}
