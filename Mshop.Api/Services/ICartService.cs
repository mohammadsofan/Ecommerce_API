using Mshop.Api.Data.models;
using Mshop.Api.Services.IService;

namespace Mshop.Api.Services
{
    public interface ICartService:IService<Cart>
    {
        Task<bool> CheckExists(Guid productId, string userId);
        Task<bool> DeleteAsync(string userId, Guid productId, CancellationToken cancellationToken = default);
        Task<int> EditQuantityAsync(string userId, Guid productId, int newQuantity, CancellationToken cancellationToken = default);
        Task<bool> ClearAsync(string userId,CancellationToken cancellationToken = default);
    }
}
