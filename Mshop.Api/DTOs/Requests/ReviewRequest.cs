using Mshop.Api.Data.models;
using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.DTOs.Requests
{
    public class ReviewRequest
    {
        public string? Comment { get; set; } = null!;
        [Range(1, 5)]
        public int Rate { get; set; }
    }
}
