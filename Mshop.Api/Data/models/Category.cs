using Mshop.Api.Data.Interfaces;

namespace Mshop.Api.Data.models
{
    public class Category : IEntityStatus
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool Status { get; set; }
        public IEnumerable<Product> Products { get; set; } = null!;
    }
}
