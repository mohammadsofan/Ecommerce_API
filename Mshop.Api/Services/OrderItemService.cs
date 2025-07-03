using Mshop.Api.Data;
using Mshop.Api.Data.models;
using Mshop.Api.Services.IService;

namespace Mshop.Api.Services
{
    public class OrderItemService : Service<OrderItem>, IOrderItemService
    {
        private readonly ApplicationDbContext context;
        public OrderItemService(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
