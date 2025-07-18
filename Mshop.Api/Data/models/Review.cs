using Microsoft.EntityFrameworkCore;
using Mshop.Api.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.Data.models
{
    [PrimaryKey(nameof(ProductId),nameof(ApplicationUserId))]
    public class Review : IEntity
    {
        public Guid Id { get; set; }
        public string? Comment { get; set; } = null!;
        [Range(1,5)]
        public int Rate { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public DateTime Created {  get; set; } = DateTime.Now;
        
    }
}
