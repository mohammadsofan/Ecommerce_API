using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data.Interfaces;

namespace Mshop.Api.Data.models
{
    [PrimaryKey(nameof(ProductId),nameof(ApplicationUserId))]
    public class Cart:IEntity
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
