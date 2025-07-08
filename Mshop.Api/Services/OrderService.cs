using Mshop.Api.Data;
using Mshop.Api.Data.models;
using Mshop.Api.Services.IService;
using System.Linq.Expressions;

namespace Mshop.Api.Services
{
    public class OrderService : Service<Order>, IOrderService
    {
        private readonly ApplicationDbContext _context;
        public OrderService(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ChangeStatus(Guid id, OrderStatus status,CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FindAsync(id);
            if(order is null)
            {
                return false;
            }
            order.OrderStatus = status;
            await _context.SaveChangesAsync(cancellationToken);
            return true;

        }
    }
}
