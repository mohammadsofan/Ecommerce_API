using Mshop.Api.Data.models;

namespace Mshop.Api.DTOs.Requests
{
    public class ChangeOrderStatusRequest
    {
        public OrderStatus OrderStatus { get; set; }
    }
}
