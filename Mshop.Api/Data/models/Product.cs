using Mshop.Api.Data.Interfaces;

namespace Mshop.Api.Data.models
{
    public class Product : IEntityStatus
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public string MainImage { get; set; } = null!;
        public bool Status { get; set; }
        public double Rate { get; set; }
        public bool IsPublished { get; set; }
        public Category Category { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public Brand Brand { get; set; } = null!;
        public Guid? BrandId { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; } = null!;


    }
}
