using Mshop.Api.Data.models;

namespace Mshop.Api.DTOs.Responses
{
    public class CartResponse
    {
        public Guid Id { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public ProductResponse Product { get; set; } = null!;

    }
}
