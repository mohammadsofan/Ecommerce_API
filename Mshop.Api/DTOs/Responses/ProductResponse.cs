using Mshop.Api.Data.models;

namespace Mshop.Api.DTOs.Responses
{
    public class ProductResponse
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
        public Guid CategoryId { get; set; }
        public Guid BrandId { get; set; }
    }
}
