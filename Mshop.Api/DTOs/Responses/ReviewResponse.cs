using Mshop.Api.Data.models;
using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.DTOs.Responses
{
    public class ReviewResponse
    {
        public Guid Id { get; set; }
        public string? Comment { get; set; } = null!;
        public int Rate { get; set; }
        public DateTime Created { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public Guid ProductId { get; set; }
        public ProductResponse Product { get; set; } = null!;
        public ApplicationUserResponse ApplicationUser { get; set; } = null!;
    }
}
