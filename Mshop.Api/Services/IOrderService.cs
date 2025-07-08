using Mshop.Api.Data.models;
using Mshop.Api.Services.IService;

namespace Mshop.Api.Services
{
    public interface IOrderService:IService<Order>
    {
        Task<bool> ChangeStatus(Guid id, OrderStatus status, CancellationToken cancellationToken = default);
    }
}
