using Mshop.Api.Data.models;
using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.DTOs.Requests
{
    public class CheckOutRequest
    {
        [Required]
        public string Currency { get; set; } = null!;
        [AllowedValues(PaymentMethod.Visa,PaymentMethod.Cash,ErrorMessage ="PaymentMethod should be 0 for Cash or 1 for Visa.")]
        public PaymentMethod PaymentMethod { get; set; }
        public decimal DeliveryFee { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public string RecipientPhone { get; set; } = null!;
    }
}
