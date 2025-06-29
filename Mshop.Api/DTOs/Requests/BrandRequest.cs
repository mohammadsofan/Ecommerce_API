using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.DTOs.Requests
{
    public class BrandRequest
    {
        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = null!;
        [Required]
        public bool Status { get; set; }
    }
}
