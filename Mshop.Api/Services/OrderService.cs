using Mshop.Api.Data;
using Mshop.Api.Data.models;
using Mshop.Api.Services.IService;
using System.Linq.Expressions;

namespace Mshop.Api.Services
{
    public class OrderService : Service<Order>, IOrderService
    {
        public OrderService(ApplicationDbContext context) : base(context)
        {
        }
    }
}
