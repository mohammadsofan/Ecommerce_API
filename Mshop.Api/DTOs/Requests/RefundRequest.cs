using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.DTOs.Requests
{
    public class RefundRequest
    {
        [Required]
        public string PaymentId { get; set; } = null!;
    }
}
