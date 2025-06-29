using Mshop.Api.DTOs.Responses;

namespace Mshop.Api.DTOs.Requests
{
    public class CartRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
