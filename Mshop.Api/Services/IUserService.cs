using Mshop.Api.DTOs.Responses;

namespace Mshop.Api.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUserResponse>> GetAllAsync();
        Task<ApplicationUserResponse> GetOneAsync(string id);
        Task<int> ChangeRoleAsync(string id, string role);
        Task<bool?> LockUserAsync(string id,DateTime dateTime);
        Task<bool?> UnlockUserAsync(string id);

    }
}
