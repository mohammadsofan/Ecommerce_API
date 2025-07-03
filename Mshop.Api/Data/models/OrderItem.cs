using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data.Interfaces;

namespace Mshop.Api.Data.models
{
    [PrimaryKey(nameof(OrderId),nameof(ProductId))]
    public class OrderItem:IEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
    }
}
